using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IDashboardCacheRepository
{
    Task<DashboardResult?> GetAsync(int userId, DateTime startDate, DateTime endDate);
    Task SetAsync(int userId, DateTime startDate, DateTime endDate, DashboardResult dashboard);
    Task InvalidateAsync(int userId);
}
