using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Publishers;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Service;

public class RecordService(
    IRecordRepository repository,
    ITagRepository tagRepository,
    ITagCacheRepository tagCacheRepository,
    IUserRepository userRepository,
    IRecordEventPublisher recordEventPublisher,
    ILogger<RecordService> logger) : IRecordService
{
    public async Task<Record> CreateRecordAsync(Record record, List<int> tagIds, int userId)
    {
        logger.LogInformation("Criando Record");

        record.ValidateValue();

        var tags = await GetValidatedTagsAsync(tagIds, userId);

        record.User = new User { Id = userId };
        record.Tags = tags;
        var recordCreated = await repository.CreateRecordAsync(record);

        var balanceAmount = record.CalculateBalanceImpact();
        await userRepository.UpdateBalanceAsync(userId, balanceAmount);

        var user = await userRepository.FindUserByIdAsync(userId);
        await recordEventPublisher.PublishRecordCreatedAsync(
            userId, record.Value, record.Operation, user.Balance);

        return recordCreated;
    }

    public async Task<PaginatedResult<Record>> GetRecordsAsync(ListRecords listRecords, int userId)
    {
        logger.LogInformation("Listando Record");
        listRecords.UserId = userId;
        return await repository.GetRecordsAsync(listRecords);
    }

    public async Task<Record> FindRecordByIdAsync(string recordId, int userId)
    {
        logger.LogInformation("Buscando um Record");
        return await FindRecordOrThrowAsync(recordId, userId);
    }

    public async Task<Record> UpdateRecordAsync(Record updateRecordRequest, List<int> tagIds, string recordId, int userId)
    {
        logger.LogInformation("Editando um Record");
        Record record = await FindRecordOrThrowAsync(recordId, userId);

        var tags = await GetValidatedTagsAsync(tagIds, userId);

        var revertOld = record.Operation == Domain.Enums.OperationEnum.Deposit ? -record.Value : record.Value;
        var applyNew = updateRecordRequest.CalculateBalanceImpact();
        var netAmount = revertOld + applyNew;

        record.Operation = updateRecordRequest.Operation;
        record.Value = updateRecordRequest.Value;
        record.Frequency = updateRecordRequest.Frequency;
        record.Tags = tags;
        await repository.UpdateRecordAsync(record);

        await userRepository.UpdateBalanceAsync(userId, netAmount);

        return await FindRecordOrThrowAsync(recordId, userId);
    }

    public async Task DeleteRecordAsync(string recordId, int userId)
    {
        Record record = await FindRecordOrThrowAsync(recordId, userId);

        await repository.DeleteRecordAsync(recordId);

        var revertAmount = record.Operation == Domain.Enums.OperationEnum.Deposit ? -record.Value : record.Value;
        await userRepository.UpdateBalanceAsync(userId, revertAmount);
    }

    private async Task<List<Tag>> GetValidatedTagsAsync(List<int> tagIds, int userId)
    {
        if (tagIds.Count == 0) return [];

        var allUserTags = await GetOrLoadUserTagsAsync(userId);
        var tagIdSet = tagIds.ToHashSet();
        var foundTags = allUserTags.Where(t => tagIdSet.Contains(t.Id!.Value)).ToList();
        var foundIds = foundTags.Select(t => t.Id!.Value).ToHashSet();
        var missingIds = tagIds.Where(id => !foundIds.Contains(id)).ToList();

        if (missingIds.Count > 0)
            throw new NotFoundException($"As seguintes tags não existem: {string.Join(", ", missingIds)}");

        return foundTags;
    }

    private async Task<List<Tag>> GetOrLoadUserTagsAsync(int userId)
    {
        var cachedTags = await tagCacheRepository.GetUserTagsAsync(userId);
        if (cachedTags != null) return cachedTags;

        var allTags = await tagRepository.FindAllTagsByUserIdAsync(userId);
        await tagCacheRepository.SetUserTagsAsync(userId, allTags);

        return allTags;
    }

    private async Task<Record> FindRecordOrThrowAsync(string recordId, int userId)
    {
        Record? record = await repository.FindRecordByIdAsync(recordId, userId);

        if (record == null)
        {
            logger.LogInformation("Record nao encontrado");
            throw new NotFoundException("Record não encontrado");
        }

        return record;
    }
}
