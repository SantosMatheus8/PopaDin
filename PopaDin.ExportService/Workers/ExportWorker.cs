using System.Text.Json;
using Azure.Messaging.ServiceBus;
using PopaDin.ExportService.Interfaces;
using PopaDin.ExportService.Models;

namespace PopaDin.ExportService.Workers;

public class ExportWorker(
    ServiceBusProcessor processor,
    IServiceScopeFactory scopeFactory,
    ILogger<ExportWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        processor.ProcessMessageAsync += ProcessMessageAsync;
        processor.ProcessErrorAsync += ProcessErrorAsync;

        logger.LogInformation("ExportWorker iniciado. Aguardando mensagens da fila...");

        await processor.StartProcessingAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("ExportWorker recebeu sinal de parada");
        }

        await processor.StopProcessingAsync();
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            logger.LogInformation("Mensagem recebida da fila: {Body}", body);

            var exportRequest = JsonSerializer.Deserialize<ExportRequestEvent>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (exportRequest is null)
            {
                logger.LogWarning("Mensagem com payload inválido, completando sem processar");
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            using var scope = scopeFactory.CreateScope();
            var recordQueryService = scope.ServiceProvider.GetRequiredService<IRecordQueryService>();
            var pdfGeneratorService = scope.ServiceProvider.GetRequiredService<IPdfGeneratorService>();
            var blobStorageService = scope.ServiceProvider.GetRequiredService<IBlobStorageService>();

            var records = await recordQueryService.GetRecordsByPeriodAsync(
                exportRequest.UserId, exportRequest.StartDate, exportRequest.EndDate);

            logger.LogInformation("Gerando PDF para o usuário {UserId} com {Count} Records",
                exportRequest.UserId, records.Count);

            using var pdfStream = pdfGeneratorService.GenerateRecordsReportStream(
                records, exportRequest.StartDate, exportRequest.EndDate);

            var blobUri = await blobStorageService.UploadPdfStreamAsync(pdfStream, exportRequest.UserId);

            logger.LogInformation("PDF exportado com sucesso para o usuário {UserId}: {BlobUri}",
                exportRequest.UserId, blobUri);

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar mensagem de exportação");
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception, "Erro no processamento do Service Bus. Source: {Source}", args.ErrorSource);
        return Task.CompletedTask;
    }
}
