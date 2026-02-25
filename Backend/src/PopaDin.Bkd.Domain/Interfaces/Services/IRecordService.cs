using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.Record;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IRecordService
{
    Task<Record> CreateRecordAsync(Record record, List<int> tagIds);
    Task<PaginatedResult<Record>> GetRecordsAsync(ListRecords listRecords);
    Task<Record> FindRecordByIdAsync(decimal recordId);
    Task<Record> UpdateRecordAsync(Record updateRecordRequest, List<int> tagIds, decimal recordId);
    Task DeleteRecordAsync(decimal recordId);
}

