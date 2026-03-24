using MongoDB.Driver;
using PopaDin.RecurrenceService.Documents;
using PopaDin.RecurrenceService.Interfaces;

namespace PopaDin.RecurrenceService.Services;

public class RecurrenceProcessor(
    IMongoDatabase database,
    IBalanceUpdater balanceUpdater,
    INotificationPublisher notificationPublisher,
    IConfiguration configuration,
    ILogger<RecurrenceProcessor> logger) : IRecurrenceProcessor
{
    private const int FrequencyOneTime = 5;
    private const int OperationDeposit = 1;

    private IMongoCollection<RecordDocument> Records =>
        database.GetCollection<RecordDocument>("records");

    private IMongoCollection<RecurrenceLogDocument> RecurrenceLogs =>
        database.GetCollection<RecurrenceLogDocument>("recurrence_logs");

    public async Task ProcessPendingRecurrencesAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var batchSize = int.Parse(configuration["RecurrenceSettings:BatchSize"] ?? "100");

        logger.LogInformation("Iniciando processamento de recorrências para {Date}", today);

        var recurringRecords = await GetAllRecurringRecordsAsync();
        logger.LogInformation("Encontrados {Count} registros recorrentes", recurringRecords.Count);

        var processedCount = 0;

        foreach (var record in recurringRecords)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var created = await ProcessRecordWithBackfillAsync(record, today);
                processedCount += created;

                if (processedCount >= batchSize)
                {
                    logger.LogInformation(
                        "Batch de {BatchSize} atingido, próximos registros serão processados na próxima execução",
                        batchSize);
                    break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao processar recorrência do registro {RecordId} do usuário {UserId}",
                    record.Id, record.UserId);
            }
        }

        logger.LogInformation("Processamento finalizado. {Count} novas ocorrências criadas", processedCount);
    }

    private async Task<int> ProcessRecordWithBackfillAsync(RecordDocument record, DateTime today)
    {
        var pendingDates = GetPendingOccurrenceDates(record, today);

        if (pendingDates.Count == 0)
            return 0;

        var createdCount = 0;

        foreach (var occurrenceDate in pendingDates)
        {
            if (await WasAlreadyProcessedAsync(record.Id!, occurrenceDate))
                continue;

            await CreateOccurrenceAsync(record, occurrenceDate);
            createdCount++;
        }

        if (createdCount > 0)
        {
            await NotifyUserAsync(record, createdCount);
        }

        return createdCount;
    }

    private async Task CreateOccurrenceAsync(RecordDocument record, DateTime occurrenceDate)
    {
        var newRecord = BuildOccurrenceRecord(record, occurrenceDate);
        await Records.InsertOneAsync(newRecord);

        await LogProcessedOccurrenceAsync(record.Id!, newRecord.Id!, occurrenceDate);

        var balanceImpact = record.Operation == OperationDeposit ? record.Value : -record.Value;
        await balanceUpdater.UpdateBalanceAsync(record.UserId, balanceImpact);

        logger.LogInformation(
            "Ocorrência criada para registro {RecordId} do usuário {UserId} na data {Date}",
            record.Id, record.UserId, occurrenceDate);
    }

    private static List<DateTime> GetPendingOccurrenceDates(RecordDocument record, DateTime today)
    {
        var interval = GetMonthInterval(record.Frequency);
        if (interval == 0) return [];

        var baseDate = record.ReferenceDate.Date;

        var endLimit = record.RecurrenceEndDate.HasValue && record.RecurrenceEndDate.Value.Date < today
            ? record.RecurrenceEndDate.Value.Date
            : today;

        if (baseDate > endLimit)
            return [];

        var dates = new List<DateTime>();
        var current = baseDate;

        while (current <= endLimit)
        {
            // Inclui a data base (primeiro registro) e todas as ocorrências até hoje
            dates.Add(current);
            current = current.AddMonths(interval);
        }

        return dates;
    }

    private async Task<List<RecordDocument>> GetAllRecurringRecordsAsync()
    {
        var builder = Builders<RecordDocument>.Filter;
        var filter = builder.Ne(r => r.Frequency, FrequencyOneTime)
                     & builder.Or(
                         builder.Eq(r => r.InstallmentGroupId, (string?)null),
                         builder.Exists(r => r.InstallmentGroupId, false)
                     );

        return await Records.Find(filter).ToListAsync();
    }

    private static int GetMonthInterval(int frequency)
    {
        return frequency switch
        {
            0 => 1,   // Monthly
            1 => 2,   // Bimonthly
            2 => 3,   // Quarterly
            3 => 6,   // Semiannual
            4 => 12,  // Annual
            _ => 0
        };
    }

    private async Task<bool> WasAlreadyProcessedAsync(string sourceRecordId, DateTime occurrenceDate)
    {
        var startOfDay = occurrenceDate.Date;
        var endOfDay = startOfDay.AddDays(1);

        var filter = Builders<RecurrenceLogDocument>.Filter.Eq(l => l.SourceRecordId, sourceRecordId)
                     & Builders<RecurrenceLogDocument>.Filter.Gte(l => l.OccurrenceDate, startOfDay)
                     & Builders<RecurrenceLogDocument>.Filter.Lt(l => l.OccurrenceDate, endOfDay);

        var count = await RecurrenceLogs.CountDocumentsAsync(filter);
        return count > 0;
    }

    private static RecordDocument BuildOccurrenceRecord(RecordDocument source, DateTime occurrenceDate)
    {
        return new RecordDocument
        {
            Name = source.Name,
            UserId = source.UserId,
            Operation = source.Operation,
            Value = source.Value,
            Frequency = FrequencyOneTime,
            Tags = source.Tags,
            ReferenceDate = occurrenceDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private async Task LogProcessedOccurrenceAsync(string sourceRecordId, string generatedRecordId, DateTime occurrenceDate)
    {
        var log = new RecurrenceLogDocument
        {
            SourceRecordId = sourceRecordId,
            GeneratedRecordId = generatedRecordId,
            OccurrenceDate = occurrenceDate,
            ProcessedAt = DateTime.UtcNow
        };

        await RecurrenceLogs.InsertOneAsync(log);
    }

    private async Task NotifyUserAsync(RecordDocument record, int occurrenceCount)
    {
        var operationType = record.Operation == OperationDeposit ? "depósito" : "saída";

        var message = occurrenceCount == 1
            ? $"A transação \"{record.Name}\" ({operationType}) de R$ {record.Value:N2} foi registrada automaticamente."
            : $"{occurrenceCount} ocorrências da transação \"{record.Name}\" ({operationType}) de R$ {record.Value:N2} foram registradas automaticamente.";

        await notificationPublisher.PublishAsync(
            record.UserId,
            "recurrence",
            "Transação recorrente registrada",
            message,
            new { recordName = record.Name, value = record.Value, operation = record.Operation, count = occurrenceCount }
        );
    }
}
