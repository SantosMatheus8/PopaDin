using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IAlertService
{
    Task<Alert> CreateAlertAsync(Alert alert, int userId);
    Task<List<Alert>> GetAlertsByUserIdAsync(int userId);
    Task ToggleAlertAsync(string alertId, bool active, int userId);
    Task DeleteAlertAsync(string alertId, int userId);
}
