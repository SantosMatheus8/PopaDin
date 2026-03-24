using PopaDin.RecurrenceService.Interfaces;

namespace PopaDin.RecurrenceService.Workers;

public class RecurrenceWorker(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<RecurrenceWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("RecurrenceWorker iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateDelayUntilNextExecution();
            logger.LogInformation("Próxima execução em {Delay}", delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("RecurrenceWorker recebeu sinal de parada");
                break;
            }

            await ExecuteProcessingAsync(stoppingToken);
        }
    }

    private async Task ExecuteProcessingAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IRecurrenceProcessor>();

            await processor.ProcessPendingRecurrencesAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro durante o processamento de recorrências");
        }
    }

    private TimeSpan CalculateDelayUntilNextExecution()
    {
        var executionHour = int.Parse(configuration["RecurrenceSettings:ExecutionHourUtc"] ?? "6");

        var now = DateTime.UtcNow;
        var nextExecution = now.Date.AddHours(executionHour);

        if (now >= nextExecution)
            nextExecution = nextExecution.AddDays(1);

        var delay = nextExecution - now;

        return delay < TimeSpan.FromMinutes(1) ? TimeSpan.FromMinutes(1) : delay;
    }
}
