using Dapper;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Infra.Queries;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Utils;
using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Infra.Repositories;

public class RecordRepository(IDbConnectionFactory connectionFactory, ILogger<RecordRepository> logger) : IRecordRepository
{
    public async Task<Record> CreateRecordAsync(Record record, List<int> tagIds)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Criando Record no banco de dados");
            var recordCreated = await connection.QuerySingleAsync<Record>(RecordQueries.CreateRecord, new
            {
                Operation = record.Operation,
                Value = record.Value,
                Frequency = record.Frequency,
                UserId = record.User.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }, transaction);

            if (tagIds.Count > 0)
            {
                foreach (var tagId in tagIds)
                    await connection.ExecuteAsync(RecordQueries.CreateRecordTag,
                        new { RecordId = recordCreated.Id, TagId = tagId }, transaction);
            }

            transaction.Commit();

            return recordCreated;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao Criar Record: {Message}", e.Message);
            throw;
        }
    }

    public async Task<PaginatedResult<Record>> GetRecordsAsync(ListRecords listRecords)
    {
        var query = AddQueryPagination(listRecords);
        var countQuery = AddFilters(listRecords, RecordQueries.Count);

        logger.LogInformation("Listando Records com paginação");

        var parameters = new
        {
            Id = listRecords.Id,
            Operation = listRecords.Operation,
            Value = listRecords.Value,
            Frequency = listRecords.Frequency,
            UserId = listRecords.UserId,
            Offset = (listRecords.Page - 1) * listRecords.ItemsPerPage,
            listRecords.ItemsPerPage
        };

        var recordDictionary = new Dictionary<int, Record>();

        using var connection = connectionFactory.CreateConnection();

        await connection.QueryAsync<Record, User, Tag, Record>(
            query,
            (record, user, tag) =>
            {
                if (!recordDictionary.TryGetValue(record.Id!.Value, out var recordEntry))
                {
                    recordEntry = record;
                    recordEntry.User = user;
                    recordDictionary.Add(recordEntry.Id!.Value, recordEntry);
                }

                if (tag?.Id != null)
                    recordEntry.Tags.Add(tag);

                return recordEntry;
            },
            parameters,
            splitOn: "UserId,TagId"
        );

        var totalLines = await connection.QuerySingleAsync<int>(countQuery, parameters);

        return new PaginatedResult<Record>
        {
            Lines = recordDictionary.Values.ToList(),
            Page = listRecords.Page,
            TotalPages = (int)Math.Ceiling(totalLines / (double)listRecords.ItemsPerPage),
            TotalItems = totalLines,
            PageSize = listRecords.ItemsPerPage
        };
    }

    private static string AddQueryPagination(ListRecords listRecords)
    {
        var query = AddFilters(listRecords, RecordQueries.ListRecords);
        query +=
            @$"
                ORDER BY
                {listRecords.OrderBy.GetEnumDescription()}
                {listRecords.OrderDirection.GetEnumDescription()}
                OFFSET @Offset
                ROWS FETCH NEXT @ItemsPerPage ROWS ONLY
                ";
        return query;
    }

    private static string AddFilters(ListRecords listRecords, string query)
    {
        query += " AND r.UserId = @UserId ";
        if (listRecords.Id.HasValue)
            query += " AND r.Id = @Id ";
        if (listRecords.Operation.HasValue)
            query += " AND r.Operation = @Operation ";
        if (listRecords.Frequency.HasValue)
            query += " AND r.Frequency = @Frequency ";
        return query;
    }

    public async Task<Record> FindRecordByIdAsync(int recordId, int userId)
    {
        logger.LogInformation("Buscando Record: {RecordId}", recordId);

        var recordDictionary = new Dictionary<int, Record>();

        using var connection = connectionFactory.CreateConnection();

        await connection.QueryAsync<Record, User, Tag, Record>(
            RecordQueries.FindRecordById,
            (record, user, tag) =>
            {
                if (!recordDictionary.TryGetValue(record.Id!.Value, out var recordEntry))
                {
                    recordEntry = record;
                    recordEntry.User = user;
                    recordDictionary.Add(recordEntry.Id!.Value, recordEntry);
                }

                if (tag?.Id != null)
                    recordEntry.Tags.Add(tag);

                return recordEntry;
            },
            new { RecordId = recordId, UserId = userId },
            splitOn: "UserId,TagId"
        );

        return recordDictionary.Values.FirstOrDefault()!;
    }

    public async Task UpdateRecordAsync(Record record, List<int> tagIds)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Atualizando Record: {RecordId}", record.Id);
            await connection.ExecuteAsync(RecordQueries.UpdateRecord,
                new
                {
                    RecordId = record.Id,
                    Operation = record.Operation,
                    Value = record.Value,
                    Frequency = record.Frequency,
                    UpdatedAt = DateTime.UtcNow
                }, transaction);

            await connection.ExecuteAsync(RecordQueries.DeleteRecordTags,
                new { RecordId = record.Id }, transaction);

            if (tagIds.Count > 0)
            {
                foreach (var tagId in tagIds)
                    await connection.ExecuteAsync(RecordQueries.CreateRecordTag,
                        new { RecordId = record.Id, TagId = tagId }, transaction);
            }

            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao editar Record: {Message}", e.Message);
            throw;
        }
    }

    public async Task DeleteRecordAsync(int recordId)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Deletando Record: {RecordId}", recordId);
            await connection.ExecuteAsync(RecordQueries.DeleteRecord,
                new { RecordId = recordId }, transaction);
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao deletar Record: {Message}", e.Message);
            throw;
        }
    }
}
