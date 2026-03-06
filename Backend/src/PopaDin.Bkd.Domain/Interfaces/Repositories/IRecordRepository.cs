using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IRecordRepository
{
    Task<Record> CreateRecordAsync(Record record, List<int> tagIds);
    Task<PaginatedResult<Record>> GetRecordsAsync(ListRecords listRecords);
    Task<Record> FindRecordByIdAsync(int recordId, int userId);
    Task UpdateRecordAsync(Record record, List<int> tagIds);
    Task DeleteRecordAsync(int recordId);
}
