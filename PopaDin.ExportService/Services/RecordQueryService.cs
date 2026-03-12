using MongoDB.Driver;
using PopaDin.ExportService.Documents;
using PopaDin.ExportService.Interfaces;
using PopaDin.ExportService.Models;

namespace PopaDin.ExportService.Services;

public class RecordQueryService(IMongoDatabase database, ILogger<RecordQueryService> logger) : IRecordQueryService
{
    private IMongoCollection<RecordDocument> Collection =>
        database.GetCollection<RecordDocument>("records");

    public async Task<List<RecordDocument>> GetRecordsByPeriodAsync(int userId, DateTime startDate, DateTime endDate)
    {
        logger.LogInformation("Buscando Records do usuário {UserId} no período {StartDate} a {EndDate}",
            userId, startDate, endDate);

        var builder = Builders<RecordDocument>.Filter;

        var notRecurring = builder.Or(
            builder.Eq(r => r.Frequency, FrequencyType.OneTime),
            builder.And(
                builder.Ne(r => r.InstallmentGroupId, (string?)null),
                builder.Exists(r => r.InstallmentGroupId, true)
            )
        );

        var filter = builder.Eq(r => r.UserId, userId)
                     & builder.Gte(r => r.ReferenceDate, startDate)
                     & builder.Lte(r => r.ReferenceDate, endDate)
                     & notRecurring;

        var sort = Builders<RecordDocument>.Sort.Ascending(r => r.ReferenceDate);

        var records = await Collection.Find(filter).Sort(sort).ToListAsync();

        var recurringFilter = builder.Eq(r => r.UserId, userId)
                              & builder.Ne(r => r.Frequency, FrequencyType.OneTime)
                              & builder.Or(
                                  builder.Eq(r => r.InstallmentGroupId, (string?)null),
                                  builder.Exists(r => r.InstallmentGroupId, false)
                              );

        var recurringRecords = await Collection.Find(recurringFilter).ToListAsync();

        foreach (var recurring in recurringRecords)
        {
            var projectedDocs = ProjectRecordOccurrences(recurring, startDate, endDate);
            records.AddRange(projectedDocs);
        }

        records = records.OrderBy(r => r.ReferenceDate ?? r.CreatedAt).ToList();

        logger.LogInformation("Encontrados {Count} Records no período (incluindo projeções recorrentes)", records.Count);

        return records;
    }

    private static List<RecordDocument> ProjectRecordOccurrences(RecordDocument record, DateTime periodStart, DateTime periodEnd)
    {
        var interval = FrequencyType.GetMonthInterval(record.Frequency);
        if (interval == 0) return [];

        var baseDate = record.ReferenceDate ?? record.CreatedAt;
        var endLimit = record.RecurrenceEndDate.HasValue && record.RecurrenceEndDate.Value < periodEnd
            ? record.RecurrenceEndDate.Value
            : periodEnd;

        var results = new List<RecordDocument>();
        var current = baseDate;

        if (current < periodStart)
        {
            var monthsAhead = (periodStart.Year - current.Year) * 12 + (periodStart.Month - current.Month);
            var steps = monthsAhead / interval;
            current = current.AddMonths(steps * interval);
            if (current < periodStart)
                current = current.AddMonths(interval);
        }

        while (current <= endLimit)
        {
            if (current >= periodStart && current <= periodEnd)
            {
                results.Add(new RecordDocument
                {
                    Id = record.Id,
                    Name = record.Name,
                    UserId = record.UserId,
                    Operation = record.Operation,
                    Value = record.Value,
                    Frequency = record.Frequency,
                    ReferenceDate = current,
                    Tags = record.Tags,
                    CreatedAt = record.CreatedAt,
                    UpdatedAt = record.UpdatedAt,
                    InstallmentGroupId = record.InstallmentGroupId,
                    InstallmentIndex = record.InstallmentIndex,
                    InstallmentTotal = record.InstallmentTotal,
                    RecurrenceEndDate = record.RecurrenceEndDate
                });
            }

            current = current.AddMonths(interval);
        }

        return results;
    }

}
