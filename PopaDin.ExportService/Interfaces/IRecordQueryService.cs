using PopaDin.ExportService.Documents;

namespace PopaDin.ExportService.Interfaces;

public interface IRecordQueryService
{
    Task<List<RecordDocument>> GetRecordsByPeriodAsync(int userId, DateTime startDate, DateTime endDate);
}
