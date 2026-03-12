using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using PopaDin.ExportService.Interfaces;

namespace PopaDin.ExportService.Services;

public class BlobStorageService(BlobContainerClient containerClient, ILogger<BlobStorageService> logger) : IBlobStorageService
{
    public async Task<string> UploadPdfAsync(byte[] pdfContent, int userId)
    {
        using var stream = new MemoryStream(pdfContent);
        return await UploadPdfStreamAsync(stream, userId);
    }

    public async Task<string> UploadPdfStreamAsync(Stream pdfStream, int userId)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var blobName = $"{userId}/export_{timestamp}.pdf";

        logger.LogInformation("Fazendo upload do PDF para o Blob Storage: {BlobName}", blobName);

        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(pdfStream, new BlobHttpHeaders
        {
            ContentType = "application/pdf"
        });

        logger.LogInformation("Upload concluído: {BlobUri}", blobClient.Uri);

        return blobClient.Uri.ToString();
    }
}
