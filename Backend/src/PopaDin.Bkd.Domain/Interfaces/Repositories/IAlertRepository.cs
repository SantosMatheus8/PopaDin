using PopaDin.Bkd.Domain.Models.Alert;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IAlertRepository
{
    Task<Alert> CreateAlertAsync(Alert alert);
    Task<List<Alert>> GetAlertsByUserIdAsync(int userId);
    Task<Alert?> FindAlertByIdAsync(string alertId, int userId);
    Task ToggleAlertAsync(string alertId, bool active);
    Task DeleteAlertAsync(string alertId);
}
