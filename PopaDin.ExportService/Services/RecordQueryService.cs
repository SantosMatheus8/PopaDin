using MongoDB.Driver;
using PopaDin.ExportService.Documents;
using PopaDin.ExportService.Interfaces;

namespace PopaDin.ExportService.Services;

public class RecordQueryService(IMongoDatabase database, ILogger<RecordQueryService> logger) : IRecordQueryService
{
    private IMongoCollection<RecordDocument> Collection =>
        database.GetCollection<RecordDocument>("records");

    public async Task<List<RecordDocument>> GetRecordsByPeriodAsync(int userId, DateTime startDate, DateTime endDate)
    {
        logger.LogInformation("Buscando Records do usuário {UserId} no período {StartDate} a {EndDate}",
            userId, startDate, endDate);

        var filter = Builders<RecordDocument>.Filter.Eq(r => r.UserId, userId)
                     & Builders<RecordDocument>.Filter.Gte(r => r.CreatedAt, startDate)
                     & Builders<RecordDocument>.Filter.Lte(r => r.CreatedAt, endDate);

        var sort = Builders<RecordDocument>.Sort.Ascending(r => r.CreatedAt);

        var records = await Collection.Find(filter).Sort(sort).ToListAsync();

        logger.LogInformation("Encontrados {Count} Records no período", records.Count);

        return records;
    }
}
