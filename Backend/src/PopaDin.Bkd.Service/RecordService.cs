using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.Record;

namespace PopaDin.Bkd.Service;

public class RecordService(IRecordRepository repository, ILogger<RecordService> logger) : IRecordService
{
    public async Task<Record> CreateRecordAsync(Record record)
    {
        logger.LogInformation("Criando Record");

        if (record.Value < 0)
        {
            throw new PopaBaseException("O valor deve ser maior que zero.", 422);
        }

        return await repository.CreateRecordAsync(record);
    }

    public async Task<PaginatedResult<Record>> GetRecordsAsync(ListRecords listRecords)
    {
        logger.LogInformation("Listando Record");
        return await repository.GetRecordsAsync(listRecords);
    }

    public async Task<Record> FindRecordByIdAsync(decimal recordId)
    {
        logger.LogInformation("Buscando um Record");
        return await FindRecordOrThrowExceptionAsync(recordId);
    }

    public async Task<Record> UpdateRecordAsync(Record updateRecordRequest, decimal recordId)
    {
        logger.LogInformation("Editando um Record");
        Record record = await FindRecordOrThrowExceptionAsync(recordId);

        record.Operation = updateRecordRequest.Operation;
        record.Value = updateRecordRequest.Value;
        record.Frequency = updateRecordRequest.Frequency;
        await repository.UpdateRecordAsync(record);

        return await repository.FindRecordByIdAsync(recordId);
    }

    public async Task DeleteRecordAsync(decimal recordId)
    {
        await FindRecordOrThrowExceptionAsync(recordId);
        await repository.DeleteRecordAsync(recordId);
    }

    private async Task<Record> FindRecordOrThrowExceptionAsync(decimal recordId)
    {
        Record record = await repository.FindRecordByIdAsync(recordId);

        if (record == null)
        {
            logger.LogInformation("Record nao encontrado");
            throw new PopaBaseException("Record não encontrado", 404);
        }

        return record;
    }
}