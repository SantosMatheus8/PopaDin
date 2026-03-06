using PopaDin.AlertService.Models;

namespace PopaDin.AlertService.Interfaces;

public interface INotificationService
{
    Task SendAlertNotificationAsync(AlertRule rule, RecordCreatedEvent recordEvent);
}
