using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.Record;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IRecordRepository
{
    Task<Record> CreateRecordAsync(Record record);
    Task<PaginatedResult<Record>> GetRecordsAsync(ListRecords listRecords);
    Task<Record> FindRecordByIdAsync(decimal recordId);
    Task UpdateRecordAsync(Record record);
    Task DeleteRecordAsync(decimal recordId);
}