using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Interfaces.Publishers;

namespace PopaDin.Bkd.Infra.Publishers;

public class ServiceBusRecordEventPublisher(
    ServiceBusSender sender,
    ILogger<ServiceBusRecordEventPublisher> logger) : IRecordEventPublisher
{
    private const int MaxRetries = 3;

    public async Task PublishRecordCreatedAsync(int userId, decimal value, OperationEnum operation, decimal newBalance, decimal monthlyExpenses)
    {
        var eventPayload = new
        {
            UserId = userId,
            Value = value,
            Operation = operation.ToString(),
            NewBalance = newBalance,
            MonthlyExpenses = monthlyExpenses
        };

        var messageBody = JsonSerializer.Serialize(eventPayload);
        var message = new ServiceBusMessage(messageBody)
        {
            ContentType = "application/json",
            Subject = "RecordCreated"
        };

        await SendWithRetryAsync(message, "RecordCreated", userId);
    }

    private async Task SendWithRetryAsync(ServiceBusMessage message, string eventType, int userId)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                logger.LogInformation("Publicando evento {EventType} no Service Bus para o usuário {UserId} (tentativa {Attempt}/{MaxRetries})",
                    eventType, userId, attempt, MaxRetries);

                await sender.SendMessageAsync(message);
                return;
            }
            catch (Exception ex) when (attempt < MaxRetries)
            {
                logger.LogWarning(ex, "Falha ao publicar evento {EventType} para o usuário {UserId} (tentativa {Attempt}/{MaxRetries}). Retentando...",
                    eventType, userId, attempt, MaxRetries);

                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha definitiva ao publicar evento {EventType} no Service Bus para o usuário {UserId} após {MaxRetries} tentativas",
                    eventType, userId, MaxRetries);
            }
        }
    }
}
