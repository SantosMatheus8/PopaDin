using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.Record;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IRecordService
{
    Task<Record> CreateRecordAsync(Record record, List<int> tagIds, decimal userId);
    Task<PaginatedResult<Record>> GetRecordsAsync(ListRecords listRecords, decimal userId);
    Task<Record> FindRecordByIdAsync(decimal recordId, decimal userId);
    Task<Record> UpdateRecordAsync(Record updateRecordRequest, List<int> tagIds, decimal recordId, decimal userId);
    Task DeleteRecordAsync(decimal recordId, decimal userId);
}
