using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Utils;

namespace PopaDin.Bkd.Service;

public class DashboardService(
    IDashboardRepository dashboardRepository,
    IDashboardCacheRepository cacheRepository,
    IBudgetRepository budgetRepository,
    IUserRepository userRepository,
    IRecordRepository recordRepository,
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
        var budgetsTask = budgetRepository.GetBudgetsAsync(new ListBudgets
        {
            UserId = userId,
            Page = 1,
            ItemsPerPage = 100
        });
        var userTask = userRepository.FindUserByIdAsync(userId);
        var recurringTask = recordRepository.GetRecurringRecordsAsync(userId);

        await Task.WhenAll(dashboardTask, budgetsTask, userTask, recurringTask);

        var dashboard = dashboardTask.Result;
        var budgets = budgetsTask.Result;
        var user = userTask.Result;
        var recurringRecords = recurringTask.Result;

        var projectedRecords = recurringRecords
            .SelectMany(r => RecurrenceHelper.ProjectRecordsForPeriod(r, resolvedStart, resolvedEnd))
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

        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var isFuturePeriod = resolvedStart > currentMonthStart;

        if (isFuturePeriod)
        {
            var cumulativeRecurringImpact = recurringRecords
                .SelectMany(r => RecurrenceHelper.ProjectRecordsForPeriod(r, now, resolvedEnd))
                .Sum(r => r.CalculateBalanceImpact());

            var futureNonRecurring = await recordRepository.GetNonRecurringByPeriodAsync(userId, now, resolvedEnd);
            var futureNonRecurringImpact = futureNonRecurring.Sum(r => r.CalculateBalanceImpact());

            dashboard.Summary.Balance = user.Balance + cumulativeRecurringImpact + futureNonRecurringImpact;
        }
        else
        {
            dashboard.Summary.Balance = user.Balance;
        }

        dashboard.Budgets = budgets.Lines
            .Where(b => b.FinishAt == null)
            .Select(b =>
            {
                var currentBalance = dashboard.Summary.Balance;
                var usedPercentage = b.Goal > 0 ? Math.Round(currentBalance / b.Goal * 100, 2) : 0;
                var status = usedPercentage >= 100 ? "exceeded"
                    : usedPercentage >= 80 ? "alert"
                    : "ok";

                return new DashboardBudget
                {
                    Id = b.Id!.Value,
                    Name = b.Name,
                    Goal = b.Goal,
                    TotalSpent = currentBalance,
                    UsedPercentage = usedPercentage,
                    Status = status
                };
            }).ToList();

        await cacheRepository.SetAsync(userId, resolvedStart, resolvedEnd, dashboard);

        return dashboard;
    }
}
