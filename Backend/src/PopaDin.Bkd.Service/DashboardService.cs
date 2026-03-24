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

        await cacheRepository.SetAsync(userId, resolvedStart, resolvedEnd, dashboard);

        return dashboard;
    }

    /// <summary>
    /// Calcula o saldo cumulativo até o fim do período solicitado.
    /// - Mês atual: usa user.Balance (saldo armazenado e atualizado em tempo real)
    /// - Mês passado: soma todos os records reais até o fim do período + projeções não materializadas
    /// - Mês futuro: user.Balance + impacto projetado entre agora e o fim do período
    /// </summary>
    private async Task<decimal> CalculatePeriodBalanceAsync(
        int userId,
        DateTime periodEnd,
        DateTime now,
        List<Record> recurringRecords,
        HashSet<(string SourceRecordId, DateTime OccurrenceDate)> materializedOccurrences,
        decimal currentBalance)
    {
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var currentMonthEnd = currentMonthStart.AddMonths(1).AddTicks(-1);

        // Mês atual: saldo real armazenado no banco
        if (periodEnd >= currentMonthStart && periodEnd <= currentMonthEnd)
            return currentBalance;

        // Mês futuro: saldo atual + projeção futura
        if (periodEnd > currentMonthEnd)
        {
            var futureRecurringImpact = recurringRecords
                .SelectMany(r => RecurrenceHelper.ProjectRecordsForPeriod(r, now, periodEnd))
                .Sum(r => r.CalculateBalanceImpact());

            var futureNonRecurring = await recordRepository.GetNonRecurringByPeriodAsync(userId, now, periodEnd);
            var futureNonRecurringImpact = futureNonRecurring.Sum(r => r.CalculateBalanceImpact());

            return currentBalance + futureRecurringImpact + futureNonRecurringImpact;
        }

        // Mês passado: calcula saldo histórico
        // 1. Soma de todos os records reais (OneTime + installments + materializados) até o fim do período
        var cumulativeBalance = await recordRepository.GetCumulativeBalanceUpToAsync(userId, periodEnd);

        // 2. Soma o impacto de projeções recorrentes que o worker ainda não materializou até o fim do período
        var unmaterializedRecurringImpact = recurringRecords
            .SelectMany(r =>
            {
                var beginningOfTime = r.ReferenceDate ?? r.CreatedAt ?? now;
                return RecurrenceHelper.ProjectRecordsForPeriod(r, beginningOfTime, periodEnd)
                    .Where(projected =>
                        !materializedOccurrences.Contains((r.Id!, projected.ReferenceDate!.Value.Date)));
            })
            .Sum(r => r.CalculateBalanceImpact());

        return cumulativeBalance + unmaterializedRecurringImpact;
    }
}
