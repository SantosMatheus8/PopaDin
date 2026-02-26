using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.Record;

namespace PopaDin.Bkd.Service;

public class RecordService(IRecordRepository repository, ITagRepository tagRepository, ILogger<RecordService> logger) : IRecordService
{
    public async Task<Record> CreateRecordAsync(Record record, List<int> tagIds)
    {
        logger.LogInformation("Criando Record");

        if (record.Value < 0)
            throw new UnprocessableEntityException("O valor deve ser maior que zero.");

        if (tagIds.Count > 0)
        {
            var foundTags = await tagRepository.FindTagsByIdsAsync(tagIds);
            var foundIds = foundTags.Select(t => t.Id!.Value).ToHashSet();
            var missingIds = tagIds.Where(id => !foundIds.Contains(id)).ToList();

            if (missingIds.Count > 0)
                throw new NotFoundException($"As seguintes tags não existem: {string.Join(", ", missingIds)}");
        }

        var recordCreated = await repository.CreateRecordAsync(record, tagIds);
        return await repository.FindRecordByIdAsync(recordCreated.Id!.Value);
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

    public async Task<Record> UpdateRecordAsync(Record updateRecordRequest, List<int> tagIds, decimal recordId)
    {
        logger.LogInformation("Editando um Record");
        Record record = await FindRecordOrThrowExceptionAsync(recordId);

        if (tagIds.Count > 0)
        {
            var foundTags = await tagRepository.FindTagsByIdsAsync(tagIds);
            var foundIds = foundTags.Select(t => t.Id!.Value).ToHashSet();
            var missingIds = tagIds.Where(id => !foundIds.Contains(id)).ToList();

            if (missingIds.Count > 0)
                throw new NotFoundException($"As seguintes tags não existem: {string.Join(", ", missingIds)}");
        }

        record.Operation = updateRecordRequest.Operation;
        record.Value = updateRecordRequest.Value;
        record.Frequency = updateRecordRequest.Frequency;
        await repository.UpdateRecordAsync(record, tagIds);

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
            throw new NotFoundException("Record não encontrado");
        }

        return record;
    }
}