using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardResult> GetDashboardAsync(int userId, DateTime? startDate, DateTime? endDate);
}
