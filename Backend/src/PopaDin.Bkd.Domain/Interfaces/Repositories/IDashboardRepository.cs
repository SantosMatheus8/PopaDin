using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IDashboardRepository
{
    Task<DashboardResult> GetDashboardDataAsync(int userId, DateTime startDate, DateTime endDate);
    Task<DashboardSummary> GetPeriodSummaryAsync(int userId, DateTime startDate, DateTime endDate);
    Task<List<DashboardMonthlyTrend>> GetMonthlyTrendAsync(int userId, DateTime startDate, DateTime endDate);
}
