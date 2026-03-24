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
    INotificationEventPublisher notificationEventPublisher,
    IRecurrenceLogRepository recurrenceLogRepository,
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

            await notificationEventPublisher.PublishAsync(
                userId, "RECORD_CREATED", "Registro Criado",
                $"Record '{record.Name}' criado com sucesso",
                new { recordId = firstRecord.Id, value = record.Value, operation = record.Operation.ToString() });

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

        await notificationEventPublisher.PublishAsync(
            userId, "RECORD_CREATED", "Registro Criado",
            $"Registro '{record.Name}' criado com sucesso",
            new { recordId = recordCreated!.Id, value = record.Value, operation = record.Operation.ToString() });

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
            return await UpdateFromInstallmentGroupAsync(record, updateRecordRequest, tags, userId, installments);

        if (installments.HasValue && installments.Value > 1)
            return await ConvertToInstallmentsAsync(record, updateRecordRequest, tags, userId, installments.Value);

        return await UpdateSingleRecordAsync(record, updateRecordRequest, tags, userId);
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

    private async Task<Record> UpdateFromInstallmentGroupAsync(Record existingRecord, Record updateRequest, List<Tag> tags, int userId, int? installments)
    {
        var oldGroupRecords = await repository.FindByInstallmentGroupAsync(existingRecord.InstallmentGroupId!, userId);
        await repository.DeleteManyByInstallmentGroupAsync(existingRecord.InstallmentGroupId!, userId);

        updateRequest.Tags = tags;
        updateRequest.User = new User { Id = userId };

        if (installments.HasValue && installments.Value > 1)
        {
            var newRecords = await installmentService.CreateInstallmentRecordsAsync(updateRequest, installments.Value);
            await balanceService.RevertBalanceForRecordsAsync(userId, oldGroupRecords);
            await balanceService.UpdateBalanceForNewRecordsAsync(userId, newRecords);
            await dashboardCacheRepository.InvalidateAsync(userId);
            return newRecords.First();
        }

        var newRecord = await repository.CreateRecordAsync(updateRequest);
        await balanceService.RevertBalanceForRecordsAsync(userId, oldGroupRecords);
        await balanceService.UpdateBalanceForNewRecordAsync(userId, newRecord);
        await dashboardCacheRepository.InvalidateAsync(userId);
        return newRecord;
    }

    private async Task<Record> ConvertToInstallmentsAsync(Record existingRecord, Record updateRequest, List<Tag> tags, int userId, int installmentCount)
    {
        await balanceService.RevertBalanceForRecordAsync(userId, existingRecord);
        await repository.DeleteRecordAsync(existingRecord.Id!);

        updateRequest.Tags = tags;
        updateRequest.User = new User { Id = userId };
        var newRecords = await installmentService.CreateInstallmentRecordsAsync(updateRequest, installmentCount);

        await balanceService.UpdateBalanceForNewRecordsAsync(userId, newRecords);
        await dashboardCacheRepository.InvalidateAsync(userId);
        return newRecords.First();
    }

    private async Task<Record> UpdateSingleRecordAsync(Record existingRecord, Record updateRequest, List<Tag> tags, int userId)
    {
        await balanceService.RevertBalanceForRecordAsync(userId, existingRecord);

        existingRecord.Name = updateRequest.Name;
        existingRecord.Operation = updateRequest.Operation;
        existingRecord.Value = updateRequest.Value;
        existingRecord.Frequency = updateRequest.Frequency;
        existingRecord.ReferenceDate = updateRequest.ReferenceDate ?? existingRecord.ReferenceDate;
        existingRecord.RecurrenceEndDate = updateRequest.RecurrenceEndDate;
        existingRecord.Tags = tags;
        await repository.UpdateRecordAsync(existingRecord);

        await balanceService.UpdateBalanceForNewRecordAsync(userId, existingRecord);
        await dashboardCacheRepository.InvalidateAsync(userId);
        return existingRecord;
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

        var nonRecurringTask = repository.GetNonRecurringByPeriodAsync(userId, monthStart, monthEnd);
        var recurringTask = repository.GetRecurringRecordsAsync(userId);
        var materializedTask = recurrenceLogRepository.GetMaterializedOccurrencesAsync(monthStart, monthEnd);

        await Task.WhenAll(nonRecurringTask, recurringTask, materializedTask);

        // Records reais (OneTime + installments + materializados pelo worker)
        var expenses = nonRecurringTask.Result
            .Where(r => r.Operation == OperationEnum.Outflow)
            .Sum(r => r.Value);

        // Projeções virtuais para ocorrências que o worker ainda não materializou
        var materializedOccurrences = materializedTask.Result;
        foreach (var record in recurringTask.Result.Where(r => r.Operation == OperationEnum.Outflow))
        {
            var occurrences = RecurrenceHelper.ProjectOccurrences(record, monthStart, monthEnd);
            var unmaterialized = occurrences
                .Count(date => !materializedOccurrences.Contains((record.Id!, date.Date)));

            expenses += unmaterialized * record.Value;
        }

        return expenses;
    }

    private async Task<Record> FindRecordOrThrowAsync(string recordId, int userId)
    {
        Record? record = await repository.FindRecordByIdAsync(recordId, userId);

        if (record == null)
        {
            logger.LogWarning("Record não encontrado. RecordId: {RecordId}, UserId: {UserId}", recordId, userId);
            throw new NotFoundException("Record não encontrado");
        }

        return record;
    }
}
