using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Service;

public class DashboardService(
    IDashboardRepository dashboardRepository,
    IDashboardCacheRepository cacheRepository,
    IBudgetRepository budgetRepository,
    IUserRepository userRepository,
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

        await Task.WhenAll(dashboardTask, budgetsTask, userTask);

        var dashboard = dashboardTask.Result;
        var budgets = budgetsTask.Result;
        var user = userTask.Result;

        dashboard.Summary.Balance = user.Balance;

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
