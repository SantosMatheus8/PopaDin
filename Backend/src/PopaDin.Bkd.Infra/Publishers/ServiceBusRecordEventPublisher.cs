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
    public async Task PublishRecordCreatedAsync(int userId, decimal value, OperationEnum operation, decimal newBalance)
    {
        var eventPayload = new
        {
            UserId = userId,
            Value = value,
            Operation = operation.ToString(),
            NewBalance = newBalance
        };

        var messageBody = JsonSerializer.Serialize(eventPayload);
        var message = new ServiceBusMessage(messageBody)
        {
            ContentType = "application/json",
            Subject = "RecordCreated"
        };

        logger.LogInformation("Publicando evento RecordCreated no Service Bus para o usuário {UserId}", userId);

        await sender.SendMessageAsync(message);
    }
}
