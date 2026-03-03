using PopaDin.Bkd.Domain.Models.Alert;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IAlertService
{
    Task<Alert> CreateAlertAsync(Alert alert, decimal userId);
    Task<List<Alert>> GetAlertsByUserIdAsync(decimal userId);
    Task ToggleAlertAsync(string alertId, bool active, decimal userId);
    Task DeleteAlertAsync(string alertId, decimal userId);
}
