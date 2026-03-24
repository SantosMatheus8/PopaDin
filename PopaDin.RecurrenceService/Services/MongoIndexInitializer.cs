using MongoDB.Driver;
using PopaDin.RecurrenceService.Documents;

namespace PopaDin.RecurrenceService.Services;

public class MongoIndexInitializer(
    IMongoDatabase database,
    ILogger<MongoIndexInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Criando índices do RecurrenceService no MongoDB");

        var collection = database.GetCollection<RecurrenceLogDocument>("recurrence_logs");

        var indexModel = new CreateIndexModel<RecurrenceLogDocument>(
            Builders<RecurrenceLogDocument>.IndexKeys
                .Ascending(l => l.SourceRecordId)
                .Ascending(l => l.OccurrenceDate),
            new CreateIndexOptions { Unique = true, Name = "idx_source_occurrence_unique" }
        );

        await collection.Indexes.CreateOneAsync(indexModel, cancellationToken: cancellationToken);

        logger.LogInformation("Índices criados com sucesso");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
