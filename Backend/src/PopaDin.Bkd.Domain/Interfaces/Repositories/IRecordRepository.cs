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
}
