using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Infra.Documents;

namespace PopaDin.Bkd.Infra.Repositories;

public class MongoRecurrenceLogRepository(
    IMongoDatabase database,
    ILogger<MongoRecurrenceLogRepository> logger) : IRecurrenceLogRepository
{
    private IMongoCollection<RecurrenceLogDocument> Collection =>
        database.GetCollection<RecurrenceLogDocument>("recurrence_logs");

    public async Task<HashSet<(string SourceRecordId, DateTime OccurrenceDate)>> GetMaterializedOccurrencesAsync(
        DateTime startDate, DateTime endDate)
    {
        logger.LogInformation("Buscando ocorrências materializadas entre {Start} e {End}", startDate, endDate);

        var filter = Builders<RecurrenceLogDocument>.Filter.Gte(l => l.OccurrenceDate, startDate)
                     & Builders<RecurrenceLogDocument>.Filter.Lte(l => l.OccurrenceDate, endDate);

        var logs = await Collection.Find(filter).ToListAsync();

        return logs
            .Select(l => (l.SourceRecordId, l.OccurrenceDate.Date))
            .ToHashSet();
    }
}
