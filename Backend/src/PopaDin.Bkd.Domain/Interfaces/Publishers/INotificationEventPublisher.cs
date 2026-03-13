namespace PopaDin.Bkd.Domain.Interfaces.Publishers;

public interface INotificationEventPublisher
{
    Task PublishAsync(int userId, string type, string title, string message, object? metadata = null);
}
