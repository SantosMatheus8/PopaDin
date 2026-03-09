using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces.Publishers;

namespace PopaDin.Bkd.Infra.Publishers;

public class ServiceBusExportEventPublisher : IExportEventPublisher
{
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

        _logger.LogInformation("Publicando evento ExportRequest no Service Bus para o usuário {UserId}", userId);

        await _sender.SendMessageAsync(message);
    }
}
