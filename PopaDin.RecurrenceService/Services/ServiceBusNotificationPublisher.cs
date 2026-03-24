using System.Text.Json;
using Azure.Messaging.ServiceBus;
using PopaDin.RecurrenceService.Interfaces;

namespace PopaDin.RecurrenceService.Services;

public class ServiceBusNotificationPublisher : INotificationPublisher
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusNotificationPublisher> _logger;

    public ServiceBusNotificationPublisher(
        ServiceBusClient client,
        IConfiguration configuration,
        ILogger<ServiceBusNotificationPublisher> logger)
    {
        var queueName = configuration["ServiceBusSettings:NotificationsQueueName"] ?? "notifications";
        _sender = client.CreateSender(queueName);
        _logger = logger;
    }

    public async Task PublishAsync(int userId, string type, string title, string message, object? metadata = null)
    {
        var eventPayload = new
        {
            userId,
            type,
            title,
            message,
            metadata = metadata ?? new { }
        };

        var messageBody = JsonSerializer.Serialize(eventPayload);
        var serviceBusMessage = new ServiceBusMessage(messageBody)
        {
            ContentType = "application/json",
            Subject = "Notification"
        };

        _logger.LogInformation("Publicando notificação de recorrência para o usuário {UserId}", userId);

        await _sender.SendMessageAsync(serviceBusMessage);
    }
}
