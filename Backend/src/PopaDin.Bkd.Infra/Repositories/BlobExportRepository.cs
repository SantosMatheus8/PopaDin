using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Infra.Repositories;

public class BlobExportRepository(BlobContainerClient containerClient, ILogger<BlobExportRepository> logger) : IExportBlobRepository
{
    public async Task<List<ExportFile>> ListExportsAsync(int userId)
    {
        logger.LogInformation("Listando exportações do usuário {UserId}", userId);

        var exports = new List<ExportFile>();
        var prefix = $"{userId}/";

        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(prefix: prefix))
        {
            var fileName = blobItem.Name.Replace(prefix, "");

            exports.Add(new ExportFile
            {
                Name = fileName,
                Url = $"/v1/record/export/files/{fileName}",
                Size = blobItem.Properties.ContentLength ?? 0,
                CreatedAt = blobItem.Properties.CreatedOn?.UtcDateTime
            });
        }

        logger.LogInformation("Encontradas {Count} exportações para o usuário {UserId}", exports.Count, userId);

        return exports.OrderByDescending(e => e.CreatedAt).ToList();
    }

    public async Task<Stream?> DownloadExportAsync(int userId, string fileName)
    {
        logger.LogInformation("Baixando exportação {FileName} do usuário {UserId}", fileName, userId);

        var blobName = $"{userId}/{fileName}";
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync())
        {
            logger.LogWarning("Exportação {BlobName} não encontrada", blobName);
            return null;
        }

        var response = await blobClient.DownloadStreamingAsync();
        return response.Value.Content;
    }
}
