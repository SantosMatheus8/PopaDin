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
    IDashboardCacheRepository dashboardCacheRepository,
    IUserRepository userRepository,
    IRecordEventPublisher recordEventPublisher,
    ILogger<RecordService> logger) : IRecordService
{
    public async Task<Record> CreateRecordAsync(Record record, List<int> tagIds, int userId, int? installments = null)
    {
        logger.LogInformation("Criando Record");

        record.ValidateValue();

        var tags = await GetValidatedTagsAsync(tagIds, userId);

        record.User = new User { Id = userId };
        record.Tags = tags;

        if (installments.HasValue && installments.Value > 1)
        {
            var createdRecords = await CreateInstallmentRecordsAsync(record, installments.Value);
            var firstRecord = createdRecords.First();

            var totalBalanceImpact = createdRecords.Sum(r => r.CalculateBalanceImpact());
            await userRepository.UpdateBalanceAsync(userId, totalBalanceImpact);

            var user = await userRepository.FindUserByIdAsync(userId);
            await recordEventPublisher.PublishRecordCreatedAsync(
                userId, record.Value, record.Operation, user.Balance);

            await dashboardCacheRepository.InvalidateAsync(userId);

            return firstRecord;
        }

        var recordCreated = await repository.CreateRecordAsync(record);

        var balanceAmount = record.CalculateBalanceImpact();
        await userRepository.UpdateBalanceAsync(userId, balanceAmount);

        var userAfter = await userRepository.FindUserByIdAsync(userId);
        await recordEventPublisher.PublishRecordCreatedAsync(
            userId, record.Value, record.Operation, userAfter.Balance);

        await dashboardCacheRepository.InvalidateAsync(userId);

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

    public async Task<Record> UpdateRecordAsync(Record updateRecordRequest, List<int> tagIds, string recordId, int userId, int? installments = null)
    {
        logger.LogInformation("Editando um Record");
        Record record = await FindRecordOrThrowAsync(recordId, userId);

        var tags = await GetValidatedTagsAsync(tagIds, userId);

        if (record.InstallmentGroupId != null)
        {
            var oldGroupRecords = await repository.FindByInstallmentGroupAsync(record.InstallmentGroupId, userId);
            var oldTotalBalance = oldGroupRecords.Sum(r => r.CalculateBalanceImpact());

            await repository.DeleteManyByInstallmentGroupAsync(record.InstallmentGroupId, userId);

            if (installments.HasValue && installments.Value > 1)
            {
                updateRecordRequest.Tags = tags;
                updateRecordRequest.User = new User { Id = userId };
                var newRecords = await CreateInstallmentRecordsAsync(updateRecordRequest, installments.Value);

                var newTotalBalance = newRecords.Sum(r => r.CalculateBalanceImpact());
                var netAmount = -oldTotalBalance + newTotalBalance;
                await userRepository.UpdateBalanceAsync(userId, netAmount);

                await dashboardCacheRepository.InvalidateAsync(userId);
                return newRecords.First();
            }
            else
            {
                updateRecordRequest.Tags = tags;
                updateRecordRequest.User = new User { Id = userId };
                var newRecord = await repository.CreateRecordAsync(updateRecordRequest);

                var netAmount = -oldTotalBalance + updateRecordRequest.CalculateBalanceImpact();
                await userRepository.UpdateBalanceAsync(userId, netAmount);

                await dashboardCacheRepository.InvalidateAsync(userId);
                return newRecord;
            }
        }

        if (installments.HasValue && installments.Value > 1)
        {
            var revertOld = record.Operation == Domain.Enums.OperationEnum.Deposit ? -record.Value : record.Value;
            await repository.DeleteRecordAsync(recordId);

            updateRecordRequest.Tags = tags;
            updateRecordRequest.User = new User { Id = userId };
            var newRecords = await CreateInstallmentRecordsAsync(updateRecordRequest, installments.Value);

            var newTotalBalance = newRecords.Sum(r => r.CalculateBalanceImpact());
            var netAmount = revertOld + newTotalBalance;
            await userRepository.UpdateBalanceAsync(userId, netAmount);

            await dashboardCacheRepository.InvalidateAsync(userId);
            return newRecords.First();
        }

        var revertOldBalance = record.Operation == Domain.Enums.OperationEnum.Deposit ? -record.Value : record.Value;
        var applyNew = updateRecordRequest.CalculateBalanceImpact();
        var balanceNet = revertOldBalance + applyNew;

        record.Name = updateRecordRequest.Name;
        record.Operation = updateRecordRequest.Operation;
        record.Value = updateRecordRequest.Value;
        record.Frequency = updateRecordRequest.Frequency;
        record.ReferenceDate = updateRecordRequest.ReferenceDate ?? record.ReferenceDate;
        record.Tags = tags;
        await repository.UpdateRecordAsync(record);

        await userRepository.UpdateBalanceAsync(userId, balanceNet);

        await dashboardCacheRepository.InvalidateAsync(userId);

        return await FindRecordOrThrowAsync(recordId, userId);
    }

    public async Task DeleteRecordAsync(string recordId, int userId)
    {
        Record record = await FindRecordOrThrowAsync(recordId, userId);

        if (record.InstallmentGroupId != null)
        {
            var groupRecords = await repository.FindByInstallmentGroupAsync(record.InstallmentGroupId, userId);
            var totalRevert = groupRecords.Sum(r =>
                r.Operation == Domain.Enums.OperationEnum.Deposit ? -r.Value : r.Value);

            await repository.DeleteManyByInstallmentGroupAsync(record.InstallmentGroupId, userId);
            await userRepository.UpdateBalanceAsync(userId, totalRevert);
        }
        else
        {
            await repository.DeleteRecordAsync(recordId);

            var revertAmount = record.Operation == Domain.Enums.OperationEnum.Deposit ? -record.Value : record.Value;
            await userRepository.UpdateBalanceAsync(userId, revertAmount);
        }

        await dashboardCacheRepository.InvalidateAsync(userId);
    }

    private async Task<List<Record>> CreateInstallmentRecordsAsync(Record baseRecord, int installmentCount)
    {
        var groupId = Guid.NewGuid().ToString("N");
        var installmentValue = Math.Round(baseRecord.Value / installmentCount, 2);
        var baseDate = baseRecord.ReferenceDate ?? DateTime.UtcNow;

        var records = new List<Record>();

        for (int i = 0; i < installmentCount; i++)
        {
            records.Add(new Record
            {
                Name = baseRecord.Name,
                Operation = baseRecord.Operation,
                Value = installmentValue,
                Frequency = baseRecord.Frequency,
                ReferenceDate = baseDate.AddMonths(i),
                Tags = baseRecord.Tags,
                User = baseRecord.User,
                InstallmentGroupId = groupId,
                InstallmentIndex = i + 1,
                InstallmentTotal = installmentCount
            });
        }

        return await repository.CreateManyRecordsAsync(records);
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
