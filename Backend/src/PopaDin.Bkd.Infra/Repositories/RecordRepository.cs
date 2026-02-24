using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Infra.Queries;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Utils;
using PopaDin.Bkd.Domain.Models.Record;
using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Infra.Repositories;

public class RecordRepository(SqlConnection connection, ILogger<RecordRepository> logger) : IRecordRepository
{
    public async Task<Record> CreateRecordAsync(Record record)
    {
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        try
        {
            logger.LogInformation("Query a ser executada: {Sql}.", RecordQueries.CreateRecord);
            var recordCreated = await connection.QueryAsync<Record>(RecordQueries.CreateRecord, new
            {
                Operation = record.Operation,
                Value = record.Value,
                Frequency = record.Frequency,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }, transaction);
            await transaction.CommitAsync();

            return recordCreated.FirstOrDefault()!;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError("Erro ao Criar Record : {Erro}", e);
            throw;
        }
    }

    public async Task<PaginatedResult<Record>> GetRecordsAsync(ListRecords listRecords)
    {
        var query = AddQueryPagination(listRecords);
        var countQuery = AddFilters(listRecords, RecordQueries.Count);

        logger.LogInformation("Query a ser executada: {Sql}. with parameters: {@Parameters}", query, listRecords);

        var result = await connection.QueryAsync<Record>(
            query, new
            {
                Id = listRecords.Id,
                Operation = listRecords.Operation,
                Value = listRecords.Value,
                Frequency = listRecords.Frequency,
                Offset = (listRecords.Page - 1) * listRecords.ItemsPerPage,
                listRecords.ItemsPerPage
            }
        );

        var totalLines = await connection.QuerySingleAsync<int>(countQuery, new
        {
            Id = listRecords.Id,
            Operation = listRecords.Operation,
            Value = listRecords.Value,
            Frequency = listRecords.Frequency,
            Offset = (listRecords.Page - 1) * listRecords.ItemsPerPage,
            listRecords.ItemsPerPage
        });


        logger.LogInformation("Resultado: {@Resultado}. ", result);

        return new PaginatedResult<Record>
        {
            Lines = result.ToList(),
            Page = listRecords.Page,
            TotalPages = (int)Math.Ceiling(totalLines / (double)listRecords.ItemsPerPage),
            TotalItens = totalLines,
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
        if (listRecords.Id.HasValue)
            query += " AND b.Id = @Id ";
        return query;
    }

    public async Task<Record> FindRecordByIdAsync(decimal recordId)
    {
        logger.LogInformation("Query executada: {Sql}.", RecordQueries.FindRecordById);

        var response = await connection.QueryFirstOrDefaultAsync<Record>(RecordQueries.FindRecordById,
            new { RecordId = recordId });

        logger.LogInformation("Resultado: {@Resultado}. ", response);

        return response!;
    }

    public async Task UpdateRecordAsync(Record record)
    {
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        try
        {
            logger.LogInformation("Query executada: {Sql}.", RecordQueries.UpdateRecord);
            var response = await connection.ExecuteAsync(RecordQueries.UpdateRecord,
                new
                {
                    RecordId = record.Id,
                    Operation = record.Operation,
                    Value = record.Value,
                    Frequency = record.Frequency,
                    UpdatedAt = DateTime.Now
                }, transaction);
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError("Erro ao editar Record : {Erro}", e);
            throw;
        }
    }

    public async Task DeleteRecordAsync(decimal recordId)
    {
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        try
        {
            logger.LogInformation("Query executada: {Sql}.", RecordQueries.DeleteRecord);
            var response = await connection.ExecuteAsync(RecordQueries.DeleteRecord,
                new
                {
                    RecordId = recordId
                }, transaction);
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError("Erro ao deletar Record : {Erro}", e);
            throw;
        }
    }
}

