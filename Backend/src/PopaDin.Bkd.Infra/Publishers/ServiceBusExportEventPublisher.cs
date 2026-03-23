using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces.Publishers;

namespace PopaDin.Bkd.Infra.Publishers;

public class ServiceBusExportEventPublisher : IExportEventPublisher
{
    private const int MaxRetries = 3;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusExportEventPublisher> _logger;

    public ServiceBusExportEventPublisher(
        ServiceBusClient client,
        IConfiguration configuration,
        ILogger<ServiceBusExportEventPublisher> logger)
    {
        var queueName = configuration["ServiceBusSettings:ExportQueueName"] ?? "pdf-relatorios";
        _sender = client.CreateSender(queueName);
        _logger = logger;
    }

    public async Task PublishExportRequestAsync(int userId, DateTime startDate, DateTime endDate)
    {
        var eventPayload = new
        {
            UserId = userId,
            StartDate = startDate,
            EndDate = endDate
        };

        var messageBody = JsonSerializer.Serialize(eventPayload);
        var message = new ServiceBusMessage(messageBody)
        {
            ContentType = "application/json",
            Subject = "ExportRequest"
        };

        await SendWithRetryAsync(message, userId);
    }

    private async Task SendWithRetryAsync(ServiceBusMessage message, int userId)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation("Publicando evento ExportRequest no Service Bus para o usuário {UserId} (tentativa {Attempt}/{MaxRetries})",
                    userId, attempt, MaxRetries);

                await _sender.SendMessageAsync(message);
                return;
            }
            catch (Exception ex) when (attempt < MaxRetries)
            {
                _logger.LogWarning(ex, "Falha ao publicar evento ExportRequest para o usuário {UserId} (tentativa {Attempt}/{MaxRetries}). Retentando...",
                    userId, attempt, MaxRetries);

                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha definitiva ao publicar evento ExportRequest no Service Bus para o usuário {UserId} após {MaxRetries} tentativas",
                    userId, MaxRetries);
            }
        }
    }
}
