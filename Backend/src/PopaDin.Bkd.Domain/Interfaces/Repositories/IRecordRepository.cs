using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IRecordRepository
{
    Task<Record> CreateRecordAsync(Record record);
    Task<List<Record>> CreateManyRecordsAsync(List<Record> records);
    Task<PaginatedResult<Record>> GetRecordsAsync(ListRecords listRecords);
    Task<Record?> FindRecordByIdAsync(string recordId, int userId);
    Task UpdateRecordAsync(Record record);
    Task DeleteRecordAsync(string recordId);
    Task DeleteManyByInstallmentGroupAsync(string installmentGroupId, int userId);
    Task<List<Record>> FindByInstallmentGroupAsync(string installmentGroupId, int userId);
    Task<List<Record>> GetRecurringRecordsAsync(int userId);
    Task<List<Record>> GetNonRecurringByPeriodAsync(int userId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Calcula o saldo cumulativo de todos os records não-recorrentes (OneTime + installments)
    /// com ReferenceDate até a data informada. Depósitos somam, saídas subtraem.
    /// </summary>
    Task<decimal> GetCumulativeBalanceUpToAsync(int userId, DateTime endDate);
}
