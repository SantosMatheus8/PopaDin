namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IBalanceService
{
    Task UpdateBalanceForNewRecordAsync(int userId, Domain.Models.Record record);
    Task UpdateBalanceForNewRecordsAsync(int userId, List<Domain.Models.Record> records);
    Task RevertBalanceForRecordAsync(int userId, Domain.Models.Record record);
    Task RevertBalanceForRecordsAsync(int userId, List<Domain.Models.Record> records);
    Task AdjustBalanceAsync(int userId, decimal oldImpact, decimal newImpact);
}
