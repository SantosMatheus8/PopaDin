using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Service;

public class DashboardService(
    IDashboardRepository dashboardRepository,
    IDashboardCacheRepository cacheRepository,
    IBudgetRepository budgetRepository,
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

        await Task.WhenAll(dashboardTask, budgetsTask);

        var dashboard = dashboardTask.Result;
        var budgets = budgetsTask.Result;

        dashboard.Budgets = budgets.Lines
            .Where(b => b.FinishAt == null || b.FinishAt >= resolvedStart)
            .Select(b =>
            {
                var totalSpent = dashboard.Summary.TotalOutflows;
                var usedPercentage = b.Goal > 0 ? Math.Round(totalSpent / b.Goal * 100, 2) : 0;
                var status = usedPercentage > 100 ? "exceeded"
                    : usedPercentage >= 80 ? "alert"
                    : "ok";

                return new DashboardBudget
                {
                    Id = b.Id!.Value,
                    Name = b.Name,
                    Goal = b.Goal,
                    TotalSpent = totalSpent,
                    UsedPercentage = usedPercentage,
                    Status = status
                };
            }).ToList();

        await cacheRepository.SetAsync(userId, resolvedStart, resolvedEnd, dashboard);

        return dashboard;
    }
}
