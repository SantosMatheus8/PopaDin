using PopaDin.ExportService.Documents;

namespace PopaDin.ExportService.Interfaces;

public interface IPdfGeneratorService
{
    byte[] GenerateRecordsReport(List<RecordDocument> records, DateTime startDate, DateTime endDate);
}
