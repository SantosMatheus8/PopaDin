using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IRecordService
{
    Task<Record> CreateRecordAsync(Record record, List<int> tagIds, int userId, int? installments = null);
    Task<PaginatedResult<Record>> GetRecordsAsync(ListRecords listRecords, int userId);
    Task<Record> FindRecordByIdAsync(string recordId, int userId);
    Task<Record> UpdateRecordAsync(Record updateRecordRequest, List<int> tagIds, string recordId, int userId, int? installments = null);
    Task DeleteRecordAsync(string recordId, int userId);
}
