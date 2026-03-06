using System.Text.Json;
using Azure.Messaging.ServiceBus;
using PopaDin.AlertService.Interfaces;
using PopaDin.AlertService.Models;

namespace PopaDin.AlertService.Workers;

public class AlertWorker(
    ServiceBusProcessor processor,
    IServiceScopeFactory scopeFactory,
    ILogger<AlertWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        processor.ProcessMessageAsync += ProcessMessageAsync;
        processor.ProcessErrorAsync += ProcessErrorAsync;

        logger.LogInformation("AlertWorker iniciado. Aguardando mensagens da fila...");

        await processor.StartProcessingAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("AlertWorker recebeu sinal de parada");
        }

        await processor.StopProcessingAsync();
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            logger.LogInformation("Mensagem recebida da fila: {Body}", body);

            var recordEvent = JsonSerializer.Deserialize<RecordCreatedEvent>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (recordEvent is null)
            {
                logger.LogWarning("Mensagem com payload inválido, completando sem processar");
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            using var scope = scopeFactory.CreateScope();
            var alertRuleService = scope.ServiceProvider.GetRequiredService<IAlertRuleService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var rules = await alertRuleService.GetActiveAlertsByUserIdAsync(recordEvent.UserId);

            foreach (var rule in rules)
            {
                try
                {
                    if (alertRuleService.IsRuleTriggered(rule, recordEvent))
                    {
                        logger.LogInformation("Alerta disparado: {AlertId} do tipo {Type} para o usuário {UserId}",
                            rule.Id, rule.Type, rule.UserId);

                        await notificationService.SendAlertNotificationAsync(rule, recordEvent);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro ao processar alerta {AlertId} do usuário {UserId}",
                        rule.Id, rule.UserId);
                }
            }

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar mensagem da fila");
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception, "Erro no processamento do Service Bus. Source: {Source}", args.ErrorSource);
        return Task.CompletedTask;
    }
}
