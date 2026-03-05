using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Publishers;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.Record;
using PopaDin.Bkd.Domain.Models.User;

namespace PopaDin.Bkd.Service;

public class RecordService(
    IRecordRepository repository,
    ITagRepository tagRepository,
    IUserRepository userRepository,
    IRecordEventPublisher recordEventPublisher,
    ILogger<RecordService> logger) : IRecordService
{
    public async Task<Record> CreateRecordAsync(Record record, List<int> tagIds, decimal userId)
    {
        logger.LogInformation("Criando Record");

        if (record.Value < 0)
            throw new UnprocessableEntityException("O valor deve ser maior que zero.");

        if (tagIds.Count > 0)
        {
            var foundTags = await tagRepository.FindTagsByIdsAsync(tagIds, userId);
            var foundIds = foundTags.Select(t => t.Id!.Value).ToHashSet();
            var missingIds = tagIds.Where(id => !foundIds.Contains(id)).ToList();

            if (missingIds.Count > 0)
                throw new NotFoundException($"As seguintes tags não existem: {string.Join(", ", missingIds)}");
        }

        record.User = new User { Id = (int)userId };
        var recordCreated = await repository.CreateRecordAsync(record, tagIds);

        var balanceAmount = record.Operation == OperationEnum.Deposit ? record.Value : -record.Value;
        await userRepository.UpdateBalanceAsync(userId, balanceAmount);

        var user = await userRepository.FindUserByIdAsync(userId);
        await recordEventPublisher.PublishRecordCreatedAsync(
            (int)userId, record.Value, record.Operation, user.Balance);

        return await FindRecordOrThrowExceptionAsync(recordCreated.Id!.Value, userId);
    }

    public async Task<PaginatedResult<Record>> GetRecordsAsync(ListRecords listRecords, decimal userId)
    {
        logger.LogInformation("Listando Record");
        listRecords.UserId = (int)userId;
        return await repository.GetRecordsAsync(listRecords);
    }

    public async Task<Record> FindRecordByIdAsync(decimal recordId, decimal userId)
    {
        logger.LogInformation("Buscando um Record");
        return await FindRecordOrThrowExceptionAsync(recordId, userId);
    }

    public async Task<Record> UpdateRecordAsync(Record updateRecordRequest, List<int> tagIds, decimal recordId, decimal userId)
    {
        logger.LogInformation("Editando um Record");
        Record record = await FindRecordOrThrowExceptionAsync(recordId, userId);

        if (tagIds.Count > 0)
        {
            var foundTags = await tagRepository.FindTagsByIdsAsync(tagIds, userId);
            var foundIds = foundTags.Select(t => t.Id!.Value).ToHashSet();
            var missingIds = tagIds.Where(id => !foundIds.Contains(id)).ToList();

            if (missingIds.Count > 0)
                throw new NotFoundException($"As seguintes tags não existem: {string.Join(", ", missingIds)}");
        }

        var revertOld = record.Operation == OperationEnum.Deposit ? -record.Value : record.Value;
        var applyNew = updateRecordRequest.Operation == OperationEnum.Deposit
            ? updateRecordRequest.Value
            : -updateRecordRequest.Value;
        var netAmount = revertOld + applyNew;

        record.Operation = updateRecordRequest.Operation;
        record.Value = updateRecordRequest.Value;
        record.Frequency = updateRecordRequest.Frequency;
        await repository.UpdateRecordAsync(record, tagIds);

        await userRepository.UpdateBalanceAsync(userId, netAmount);

        return await FindRecordOrThrowExceptionAsync(recordId, userId);
    }

    public async Task DeleteRecordAsync(decimal recordId, decimal userId)
    {
        Record record = await FindRecordOrThrowExceptionAsync(recordId, userId);

        await repository.DeleteRecordAsync(recordId);

        var revertAmount = record.Operation == OperationEnum.Deposit ? -record.Value : record.Value;
        await userRepository.UpdateBalanceAsync(userId, revertAmount);
    }

    private async Task<Record> FindRecordOrThrowExceptionAsync(decimal recordId, decimal userId)
    {
        Record record = await repository.FindRecordByIdAsync(recordId, userId);

        if (record == null)
        {
            logger.LogInformation("Record nao encontrado");
            throw new NotFoundException("Record não encontrado");
        }

        return record;
    }
}
