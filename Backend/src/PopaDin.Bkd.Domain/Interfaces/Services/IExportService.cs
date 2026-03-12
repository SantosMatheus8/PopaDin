using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IExportService
{
    Task RequestExportAsync(int userId, DateTime startDate, DateTime endDate);
    Task<List<ExportFile>> ListExportsAsync(int userId);
    Task<Stream?> DownloadExportAsync(int userId, string fileName);
}
