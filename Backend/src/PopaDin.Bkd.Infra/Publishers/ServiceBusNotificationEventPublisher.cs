using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces.Publishers;

namespace PopaDin.Bkd.Infra.Publishers;

public class ServiceBusNotificationEventPublisher : INotificationEventPublisher
{
    private const int MaxRetries = 3;
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

        await SendWithRetryAsync(serviceBusMessage, type, userId);
    }

    private async Task SendWithRetryAsync(ServiceBusMessage message, string eventType, int userId)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation("Publicando notificação do tipo {Type} na fila notifications para o usuário {UserId} (tentativa {Attempt}/{MaxRetries})",
                    eventType, userId, attempt, MaxRetries);

                await _sender.SendMessageAsync(message);
                return;
            }
            catch (Exception ex) when (attempt < MaxRetries)
            {
                _logger.LogWarning(ex, "Falha ao publicar notificação do tipo {Type} para o usuário {UserId} (tentativa {Attempt}/{MaxRetries}). Retentando...",
                    eventType, userId, attempt, MaxRetries);

                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha definitiva ao publicar notificação do tipo {Type} para o usuário {UserId} após {MaxRetries} tentativas",
                    eventType, userId, MaxRetries);
            }
        }
    }
}
