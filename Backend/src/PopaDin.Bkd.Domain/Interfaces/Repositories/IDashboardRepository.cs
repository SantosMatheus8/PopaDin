using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IDashboardRepository
{
    Task<DashboardResult> GetDashboardDataAsync(int userId, DateTime startDate, DateTime endDate);
}
