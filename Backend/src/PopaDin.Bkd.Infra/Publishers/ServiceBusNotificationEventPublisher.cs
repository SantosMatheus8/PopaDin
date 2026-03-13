using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces.Publishers;

namespace PopaDin.Bkd.Infra.Publishers;

public class ServiceBusNotificationEventPublisher : INotificationEventPublisher
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusNotificationEventPublisher> _logger;

    public ServiceBusNotificationEventPublisher(
        ServiceBusClient client,
        IConfiguration configuration,
        ILogger<ServiceBusNotificationEventPublisher> logger)
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

        _logger.LogInformation("Publicando notificação do tipo {Type} na fila notifications para o usuário {UserId}", type, userId);

        await _sender.SendMessageAsync(serviceBusMessage);
    }
}
