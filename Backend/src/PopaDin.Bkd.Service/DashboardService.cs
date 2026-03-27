using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Utils;

namespace PopaDin.Bkd.Service;

public class DashboardService(
    IDashboardRepository dashboardRepository,
    IDashboardCacheRepository cacheRepository,
    IGoalRepository goalRepository,
    IUserRepository userRepository,
    IRecordRepository recordRepository,
    IRecurrenceLogRepository recurrenceLogRepository,
    ILogger<DashboardService> logger) : IDashboardService
{
    public async Task<DashboardResult> GetDashboardAsync(int userId, DateTime? startDate, DateTime? endDate)
    {
        var now = DateTime.UtcNow;
        var resolvedStart = startDate ?? new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var resolvedEnd = endDate ?? resolvedStart.AddMonths(1).AddTicks(-1);

        logger.LogInformation("Buscando dashboard para o usuário {UserId} de {Start} até {End}",
            userId, resolvedStart, resolvedEnd);

        var cached = await cacheRepository.GetAsync(userId, resolvedStart, resolvedEnd);
        if (cached != null)
        {
            logger.LogInformation("Dashboard encontrado no cache para o usuário {UserId}", userId);
            return cached;
        }

        var dashboardTask = dashboardRepository.GetDashboardDataAsync(userId, resolvedStart, resolvedEnd);
        var goalsTask = goalRepository.GetGoalsAsync(new ListGoals
        {
            UserId = userId,
            Page = 1,
            ItemsPerPage = 100
        });
        var userTask = userRepository.FindUserByIdAsync(userId);
        var recurringTask = recordRepository.GetRecurringRecordsAsync(userId);
        var materializedTask = recurrenceLogRepository.GetMaterializedOccurrencesAsync(resolvedStart, resolvedEnd);

        await Task.WhenAll(dashboardTask, goalsTask, userTask, recurringTask, materializedTask);

        var dashboard = dashboardTask.Result;
        var goals = goalsTask.Result;
        var user = userTask.Result;
        var recurringRecords = recurringTask.Result;
        var materializedOccurrences = materializedTask.Result;

        // Projeta ocorrências recorrentes para o período completo,
        // mas exclui as que já foram materializadas pelo RecurrenceService (worker).
        // Isso garante que:
        //   - Se o worker já rodou: usa o record real (editável pelo usuário), sem duplicar
        //   - Se o worker ainda não rodou: projeção virtual preenche o gap
        var projectedRecords = recurringRecords
            .SelectMany(r => RecurrenceHelper.ProjectRecordsForPeriod(r, resolvedStart, resolvedEnd)
                .Where(projected =>
                    !materializedOccurrences.Contains((r.Id!, projected.ReferenceDate!.Value.Date))))
            .ToList();

        if (projectedRecords.Count > 0)
        {
            var projectedDeposits = projectedRecords
                .Where(r => r.Operation == OperationEnum.Deposit)
                .Sum(r => r.Value);

            var projectedOutflows = projectedRecords
                .Where(r => r.Operation == OperationEnum.Outflow)
                .Sum(r => r.Value);

            dashboard.Summary.TotalDeposits += projectedDeposits;
            dashboard.Summary.TotalOutflows += projectedOutflows;
            dashboard.Summary.RecordCount += projectedRecords.Count;

            var projectedSpending = projectedRecords
                .Where(r => r.Operation == OperationEnum.Outflow)
                .SelectMany(r => r.Tags.Select(t => new { Tag = t, r.Value }))
                .GroupBy(x => new { x.Tag.Id, x.Tag.Name })
                .Select(g => new DashboardSpendingByTag
                {
                    TagId = g.Key.Id!.Value,
                    TagName = g.Key.Name,
                    TotalSpent = g.Sum(x => x.Value)
                })
                .ToList();

            foreach (var projected in projectedSpending)
            {
                var existing = dashboard.SpendingByTag.FirstOrDefault(s => s.TagId == projected.TagId);
                if (existing != null)
                    existing.TotalSpent += projected.TotalSpent;
                else
                    dashboard.SpendingByTag.Add(projected);
            }

            dashboard.SpendingByTag = dashboard.SpendingByTag.OrderByDescending(s => s.TotalSpent).ToList();

            var allLatest = dashboard.LatestRecords.Concat(projectedRecords)
                .OrderByDescending(r => r.ReferenceDate)
                .Take(5)
                .ToList();
            dashboard.LatestRecords = allLatest;

            var allTopDeposits = dashboard.TopDeposits
                .Concat(projectedRecords.Where(r => r.Operation == OperationEnum.Deposit))
                .OrderByDescending(r => r.Value)
                .Take(5)
                .ToList();
            dashboard.TopDeposits = allTopDeposits;

            var allTopOutflows = dashboard.TopOutflows
                .Concat(projectedRecords.Where(r => r.Operation == OperationEnum.Outflow))
                .OrderByDescending(r => r.Value)
                .Take(5)
                .ToList();
            dashboard.TopOutflows = allTopOutflows;
        }

        dashboard.Summary.Balance = await CalculatePeriodBalanceAsync(
            userId, resolvedEnd, now, recurringRecords, materializedOccurrences, user.Balance);

        dashboard.Goals = goals.Lines
            .Where(g => g.FinishAt == null)
            .Select(g =>
            {
                var currentBalance = dashboard.Summary.Balance;
                var savedPercentage = g.TargetAmount > 0 ? Math.Round(currentBalance / g.TargetAmount * 100, 2) : 0;
                var status = savedPercentage >= 100 ? "achieved"
                    : savedPercentage >= 80 ? "close"
                    : "ok";

                return new DashboardGoal
                {
                    Id = g.Id!.Value,
                    Name = g.Name,
                    TargetAmount = g.TargetAmount,
                    TotalSaved = currentBalance,
                    SavedPercentage = savedPercentage,
                    Status = status
                };
            }).ToList();

        dashboard.Comparison = await BuildComparisonAsync(userId, resolvedStart, resolvedEnd, dashboard.Summary);
        dashboard.MonthlyTrend = await BuildMonthlyTrendAsync(userId, resolvedStart);

        await cacheRepository.SetAsync(userId, resolvedStart, resolvedEnd, dashboard);

        return dashboard;
    }

    private async Task<DashboardComparison> BuildComparisonAsync(
        int userId, DateTime periodStart, DateTime periodEnd, DashboardSummary currentSummary)
    {
        var periodLength = periodEnd - periodStart;
        var previousStart = periodStart - periodLength - TimeSpan.FromTicks(1);
        var previousEnd = periodStart - TimeSpan.FromTicks(1);

        var previousSummary = await dashboardRepository.GetPeriodSummaryAsync(userId, previousStart, previousEnd);

        static decimal ChangePercent(decimal current, decimal previous) =>
            previous == 0 ? 0 : Math.Round((current - previous) / previous * 100, 2);

        return new DashboardComparison
        {
            PreviousTotalDeposits = previousSummary.TotalDeposits,
            PreviousTotalOutflows = previousSummary.TotalOutflows,
            DepositsChangePercent = ChangePercent(currentSummary.TotalDeposits, previousSummary.TotalDeposits),
            OutflowsChangePercent = ChangePercent(currentSummary.TotalOutflows, previousSummary.TotalOutflows)
        };
    }

    private async Task<List<DashboardMonthlyTrend>> BuildMonthlyTrendAsync(int userId, DateTime periodStart)
    {
        // Busca os últimos 6 meses completos até o início do período selecionado
        var trendEnd = periodStart.AddMonths(1).AddTicks(-1);
        var trendStart = new DateTime(periodStart.Year, periodStart.Month, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMonths(-5);

        return await dashboardRepository.GetMonthlyTrendAsync(userId, trendStart, trendEnd);
    }

    /// <summary>
    /// Calcula o saldo cumulativo até o fim do período solicitado.
    /// Abordagem unificada para qualquer período (passado, atual ou futuro):
    ///   Saldo = soma de todos records reais até periodEnd
    ///         + projeções recorrentes não materializadas até periodEnd
    /// </summary>
    private async Task<decimal> CalculatePeriodBalanceAsync(
        int userId,
        DateTime periodEnd,
        DateTime now,
        List<Record> recurringRecords,
        HashSet<(string SourceRecordId, DateTime OccurrenceDate)> materializedOccurrences,
        decimal currentBalance)
    {
        // 1. Soma de todos os records reais (OneTime + installments + materializados) até o fim do período
        var cumulativeBalance = await recordRepository.GetCumulativeBalanceUpToAsync(userId, periodEnd);

        // 2. Busca TODAS as ocorrências materializadas até o fim do período
        //    (não apenas as do período visível do dashboard)
        var allMaterialized = await recurrenceLogRepository.GetMaterializedOccurrencesUpToAsync(periodEnd);

        // 3. Soma o impacto de projeções recorrentes não materializadas até o fim do período
        var unmaterializedRecurringImpact = recurringRecords
            .SelectMany(r =>
            {
                var baseDate = r.ReferenceDate ?? r.CreatedAt ?? now;
                return RecurrenceHelper.ProjectRecordsForPeriod(r, baseDate, periodEnd)
                    .Where(projected =>
                        !allMaterialized.Contains((r.Id!, projected.ReferenceDate!.Value.Date)));
            })
            .Sum(r => r.CalculateBalanceImpact());

        return cumulativeBalance + unmaterializedRecurringImpact;
    }
}
