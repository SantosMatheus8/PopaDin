using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Publishers;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Utils;

namespace PopaDin.Bkd.Service;

public class RecordService(
    IRecordRepository repository,
    ITagRepository tagRepository,
    ITagCacheRepository tagCacheRepository,
    IDashboardCacheRepository dashboardCacheRepository,
    IUserRepository userRepository,
    IBalanceService balanceService,
    IInstallmentService installmentService,
    IRecordEventPublisher recordEventPublisher,
    TimeProvider timeProvider,
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
            var createdRecords = await installmentService.CreateInstallmentRecordsAsync(record, installments.Value);
            var firstRecord = createdRecords.First();

            await balanceService.UpdateBalanceForNewRecordsAsync(userId, createdRecords);

            var user = await userRepository.FindUserByIdAsync(userId);
            var monthlyExpenses = await CalculateCurrentMonthExpensesAsync(userId);
            await recordEventPublisher.PublishRecordCreatedAsync(
                userId, record.Value, record.Operation, user.Balance, monthlyExpenses);

            await dashboardCacheRepository.InvalidateAsync(userId);

            return firstRecord;
        }

        Record? recordCreated = null;
        try
        {
            recordCreated = await repository.CreateRecordAsync(record);
            await balanceService.UpdateBalanceForNewRecordAsync(userId, recordCreated);
        }
        catch (Exception) when (recordCreated != null)
        {
            logger.LogWarning("Falha ao atualizar saldo. Revertendo criação do Record {RecordId}", recordCreated.Id);
            await repository.DeleteRecordAsync(recordCreated.Id!);
            throw;
        }

        var userAfter = await userRepository.FindUserByIdAsync(userId);
        var currentMonthExpenses = await CalculateCurrentMonthExpensesAsync(userId);
        await recordEventPublisher.PublishRecordCreatedAsync(
            userId, record.Value, record.Operation, userAfter.Balance, currentMonthExpenses);

        await dashboardCacheRepository.InvalidateAsync(userId);

        return recordCreated!;
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
            await repository.DeleteManyByInstallmentGroupAsync(record.InstallmentGroupId, userId);

            if (installments.HasValue && installments.Value > 1)
            {
                updateRecordRequest.Tags = tags;
                updateRecordRequest.User = new User { Id = userId };
                var newRecords = await installmentService.CreateInstallmentRecordsAsync(updateRecordRequest, installments.Value);

                await balanceService.RevertBalanceForRecordsAsync(userId, oldGroupRecords);
                await balanceService.UpdateBalanceForNewRecordsAsync(userId, newRecords);

                await dashboardCacheRepository.InvalidateAsync(userId);
                return newRecords.First();
            }
            else
            {
                updateRecordRequest.Tags = tags;
                updateRecordRequest.User = new User { Id = userId };
                var newRecord = await repository.CreateRecordAsync(updateRecordRequest);

                await balanceService.RevertBalanceForRecordsAsync(userId, oldGroupRecords);
                await balanceService.UpdateBalanceForNewRecordAsync(userId, newRecord);

                await dashboardCacheRepository.InvalidateAsync(userId);
                return newRecord;
            }
        }

        if (installments.HasValue && installments.Value > 1)
        {
            await balanceService.RevertBalanceForRecordAsync(userId, record);
            await repository.DeleteRecordAsync(recordId);

            updateRecordRequest.Tags = tags;
            updateRecordRequest.User = new User { Id = userId };
            var newRecords = await installmentService.CreateInstallmentRecordsAsync(updateRecordRequest, installments.Value);

            await balanceService.UpdateBalanceForNewRecordsAsync(userId, newRecords);

            await dashboardCacheRepository.InvalidateAsync(userId);
            return newRecords.First();
        }

        await balanceService.RevertBalanceForRecordAsync(userId, record);

        record.Name = updateRecordRequest.Name;
        record.Operation = updateRecordRequest.Operation;
        record.Value = updateRecordRequest.Value;
        record.Frequency = updateRecordRequest.Frequency;
        record.ReferenceDate = updateRecordRequest.ReferenceDate ?? record.ReferenceDate;
        record.RecurrenceEndDate = updateRecordRequest.RecurrenceEndDate;
        record.Tags = tags;
        await repository.UpdateRecordAsync(record);

        await balanceService.UpdateBalanceForNewRecordAsync(userId, record);

        await dashboardCacheRepository.InvalidateAsync(userId);

        return await FindRecordOrThrowAsync(recordId, userId);
    }

    public async Task DeleteRecordAsync(string recordId, int userId)
    {
        Record record = await FindRecordOrThrowAsync(recordId, userId);

        if (record.InstallmentGroupId != null)
        {
            var groupRecords = await repository.FindByInstallmentGroupAsync(record.InstallmentGroupId, userId);
            await repository.DeleteManyByInstallmentGroupAsync(record.InstallmentGroupId, userId);
            await balanceService.RevertBalanceForRecordsAsync(userId, groupRecords);
        }
        else
        {
            await repository.DeleteRecordAsync(recordId);
            await balanceService.RevertBalanceForRecordAsync(userId, record);
        }

        await dashboardCacheRepository.InvalidateAsync(userId);
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

    private async Task<decimal> CalculateCurrentMonthExpensesAsync(int userId)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

        var nonRecurring = await repository.GetNonRecurringByPeriodAsync(userId, monthStart, monthEnd);
        var recurring = await repository.GetRecurringRecordsAsync(userId);

        var expenses = nonRecurring
            .Where(r => r.Operation == OperationEnum.Outflow)
            .Sum(r => r.Value);

        foreach (var record in recurring.Where(r => r.Operation == OperationEnum.Outflow))
        {
            var occurrences = RecurrenceHelper.ProjectOccurrences(record, monthStart, monthEnd);
            expenses += occurrences.Count * record.Value;
        }

        return expenses;
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
