using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Publishers;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Service;

public class ExportService(
    IExportEventPublisher exportEventPublisher,
    IExportBlobRepository exportBlobRepository,
    ILogger<ExportService> logger) : IExportService
{
    private const int MaxExportPeriodDays = 365;

    public async Task RequestExportAsync(int userId, DateTime startDate, DateTime endDate)
    {
        logger.LogInformation("Solicitando exportação para o usuário {UserId}", userId);

        if (startDate >= endDate)
            throw new UnprocessableEntityException("A data inicial deve ser anterior à data final.");

        if ((endDate - startDate).TotalDays > MaxExportPeriodDays)
            throw new UnprocessableEntityException($"O período máximo de exportação é de {MaxExportPeriodDays} dias.");

        await exportEventPublisher.PublishExportRequestAsync(userId, startDate, endDate);
    }

    public async Task<List<ExportFile>> ListExportsAsync(int userId)
    {
        logger.LogInformation("Listando exportações do usuário {UserId}", userId);
        return await exportBlobRepository.ListExportsAsync(userId);
    }

    public async Task<Stream?> DownloadExportAsync(int userId, string fileName)
    {
        logger.LogInformation("Baixando exportação {FileName} do usuário {UserId}", fileName, userId);
        return await exportBlobRepository.DownloadExportAsync(userId, fileName);
    }
}
