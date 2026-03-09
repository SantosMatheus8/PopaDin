using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IExportBlobRepository
{
    Task<List<ExportFile>> ListExportsAsync(int userId);
    Task<Stream?> DownloadExportAsync(int userId, string fileName);
}
