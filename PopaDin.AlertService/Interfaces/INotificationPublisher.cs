namespace PopaDin.AlertService.Interfaces;

public interface INotificationPublisher
{
    Task PublishAsync(int userId, string type, string title, string message, object? metadata = null);
}
