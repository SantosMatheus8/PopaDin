using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Service;

public class InstallmentService(
    IRecordRepository repository,
    TimeProvider timeProvider) : IInstallmentService
{
    public async Task<List<Record>> CreateInstallmentRecordsAsync(Record baseRecord, int installmentCount)
    {
        var groupId = Guid.NewGuid().ToString("N");
        var installmentValue = Math.Round(baseRecord.Value / installmentCount, 2);
        var baseDate = baseRecord.ReferenceDate ?? timeProvider.GetUtcNow().UtcDateTime;

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
}
