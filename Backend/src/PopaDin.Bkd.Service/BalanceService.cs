using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Service;

public class BalanceService(
    IUserRepository userRepository,
    TimeProvider timeProvider) : IBalanceService
{
    public async Task UpdateBalanceForNewRecordAsync(int userId, Record record)
    {
        var impact = CalculateSingleRecordBalanceImpact(record);
        if (impact != 0)
            await userRepository.UpdateBalanceAsync(userId, impact);
    }

    public async Task UpdateBalanceForNewRecordsAsync(int userId, List<Record> records)
    {
        var impact = CalculateBalanceImpactByDate(records);
        if (impact != 0)
            await userRepository.UpdateBalanceAsync(userId, impact);
    }

    public async Task RevertBalanceForRecordAsync(int userId, Record record)
    {
        var impact = CalculateSingleRecordBalanceImpact(record);
        if (impact != 0)
            await userRepository.UpdateBalanceAsync(userId, -impact);
    }

    public async Task RevertBalanceForRecordsAsync(int userId, List<Record> records)
    {
        var impact = CalculateBalanceImpactByDate(records);
        if (impact != 0)
            await userRepository.UpdateBalanceAsync(userId, -impact);
    }

    public async Task AdjustBalanceAsync(int userId, decimal oldImpact, decimal newImpact)
    {
        var net = -oldImpact + newImpact;
        if (net != 0)
            await userRepository.UpdateBalanceAsync(userId, net);
    }

    private decimal CalculateSingleRecordBalanceImpact(Record record)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        // Records recorrentes são apenas "templates". O impacto real no saldo
        // vem dos records OneTime criados pelo RecurrenceService (worker).
        if (record.IsRecurring)
            return 0;

        var refDate = record.ReferenceDate ?? record.CreatedAt ?? now;
        if (refDate > now)
            return 0;

        return record.CalculateBalanceImpact();
    }

    private decimal CalculateBalanceImpactByDate(List<Record> records)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        return records
            .Where(r =>
            {
                var refDate = r.ReferenceDate ?? r.CreatedAt ?? now;
                return refDate <= now;
            })
            .Sum(r => r.CalculateBalanceImpact());
    }
}
