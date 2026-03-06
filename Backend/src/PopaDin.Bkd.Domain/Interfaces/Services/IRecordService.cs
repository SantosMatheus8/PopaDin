using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IRecordService
{
    Task<Record> CreateRecordAsync(Record record, List<int> tagIds, int userId);
    Task<PaginatedResult<Record>> GetRecordsAsync(ListRecords listRecords, int userId);
    Task<Record> FindRecordByIdAsync(int recordId, int userId);
    Task<Record> UpdateRecordAsync(Record updateRecordRequest, List<int> tagIds, int recordId, int userId);
    Task DeleteRecordAsync(int recordId, int userId);
}
