using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.Record;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IRecordRepository
{
    Task<Record> CreateRecordAsync(Record record, List<int> tagIds);
    Task<PaginatedResult<Record>> GetRecordsAsync(ListRecords listRecords);
    Task<Record> FindRecordByIdAsync(decimal recordId, decimal userId);
    Task UpdateRecordAsync(Record record, List<int> tagIds);
    Task DeleteRecordAsync(decimal recordId);
}
