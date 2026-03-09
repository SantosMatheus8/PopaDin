using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Infra.Documents;

namespace PopaDin.Bkd.Infra.Repositories;

public class MongoRecordRepository(IMongoDatabase database, ILogger<MongoRecordRepository> logger) : IRecordRepository
{
    private IMongoCollection<RecordDocument> Collection =>
        database.GetCollection<RecordDocument>("records");

    public async Task<Record> CreateRecordAsync(Record record)
    {
        logger.LogInformation("Criando Record no MongoDB");

        var document = MapToDocument(record);
        document.CreatedAt = DateTime.UtcNow;
        document.UpdatedAt = DateTime.UtcNow;

        await Collection.InsertOneAsync(document);

        logger.LogInformation("Record criado com Id: {Id}", document.Id);

        return MapToRecord(document);
    }

    public async Task<PaginatedResult<Record>> GetRecordsAsync(ListRecords listRecords)
    {
        logger.LogInformation("Listando Records com paginação");

        var filter = BuildFilter(listRecords);
        var sort = BuildSort(listRecords);
        var skip = (listRecords.Page - 1) * listRecords.ItemsPerPage;

        var documents = await Collection
            .Find(filter)
            .Sort(sort)
            .Skip(skip)
            .Limit(listRecords.ItemsPerPage)
            .ToListAsync();

        var totalItems = await Collection.CountDocumentsAsync(filter);

        return new PaginatedResult<Record>
        {
            Lines = documents.Select(MapToRecord).ToList(),
            Page = listRecords.Page,
            TotalPages = (int)Math.Ceiling(totalItems / (double)listRecords.ItemsPerPage),
            TotalItems = (int)totalItems,
            PageSize = listRecords.ItemsPerPage
        };
    }

    public async Task<Record?> FindRecordByIdAsync(string recordId, int userId)
    {
        logger.LogInformation("Buscando Record: {RecordId}", recordId);

        if (!ObjectId.TryParse(recordId, out _))
            return null;

        var filter = Builders<RecordDocument>.Filter.Eq(r => r.Id, recordId)
                     & Builders<RecordDocument>.Filter.Eq(r => r.UserId, userId);

        var document = await Collection.Find(filter).FirstOrDefaultAsync();

        return document == null ? null : MapToRecord(document);
    }

    public async Task UpdateRecordAsync(Record record)
    {
        logger.LogInformation("Atualizando Record: {RecordId}", record.Id);

        var filter = Builders<RecordDocument>.Filter.Eq(r => r.Id, record.Id);
        var document = MapToDocument(record);
        document.UpdatedAt = DateTime.UtcNow;

        await Collection.ReplaceOneAsync(filter, document);
    }

    public async Task DeleteRecordAsync(string recordId)
    {
        logger.LogInformation("Deletando Record: {RecordId}", recordId);

        var filter = Builders<RecordDocument>.Filter.Eq(r => r.Id, recordId);
        await Collection.DeleteOneAsync(filter);
    }

    private static FilterDefinition<RecordDocument> BuildFilter(ListRecords listRecords)
    {
        var builder = Builders<RecordDocument>.Filter;
        var filter = builder.Eq(r => r.UserId, listRecords.UserId);

        if (listRecords.Id.HasValue)
            filter &= builder.Eq(r => r.Id, listRecords.Id.Value.ToString());

        if (listRecords.Operation.HasValue)
            filter &= builder.Eq(r => r.Operation, (int)listRecords.Operation.Value);

        if (listRecords.Frequency.HasValue)
            filter &= builder.Eq(r => r.Frequency, (int)listRecords.Frequency.Value);

        return filter;
    }

    private static SortDefinition<RecordDocument> BuildSort(ListRecords listRecords)
    {
        var builder = Builders<RecordDocument>.Sort;

        var fieldName = listRecords.OrderBy switch
        {
            RecordOrderBy.Id => "Id",
            RecordOrderBy.CreatedAt => "CreatedAt",
            RecordOrderBy.Frequency => "Frequency",
            RecordOrderBy.Value => "Value",
            RecordOrderBy.Operation => "Operation",
            _ => "CreatedAt"
        };

        return listRecords.OrderDirection == OrderDirection.ASC
            ? builder.Ascending(fieldName)
            : builder.Descending(fieldName);
    }

    private static RecordDocument MapToDocument(Record record)
    {
        return new RecordDocument
        {
            Id = record.Id,
            UserId = record.User.Id,
            Operation = (int)record.Operation,
            Value = record.Value,
            Frequency = (int)record.Frequency,
            Tags = record.Tags.Select(t => new RecordTagSubDocument
            {
                OriginalTagId = t.Id!.Value,
                Name = t.Name,
                TagType = t.TagType.HasValue ? (int)t.TagType.Value : null,
                Description = t.Description
            }).ToList(),
            CreatedAt = record.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = record.UpdatedAt ?? DateTime.UtcNow
        };
    }

    private static Record MapToRecord(RecordDocument document)
    {
        return new Record
        {
            Id = document.Id,
            Operation = (OperationEnum)document.Operation,
            Value = document.Value,
            Frequency = (FrequencyEnum)document.Frequency,
            Tags = document.Tags.Select(t => new Domain.Models.Tag
            {
                Id = t.OriginalTagId,
                Name = t.Name,
                TagType = t.TagType.HasValue ? (OperationEnum)t.TagType.Value : null,
                Description = t.Description
            }).ToList(),
            User = new User { Id = document.UserId },
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }
}
