using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces.Repositories;

namespace PopaDin.Bkd.Infra.Repositories;

public class BlobProfilePictureRepository(BlobContainerClient containerClient, ILogger<BlobProfilePictureRepository> logger) : IProfilePictureBlobRepository
{
    public async Task<string> UploadAsync(int userId, Stream fileStream, string contentType)
    {
        var extension = contentType switch
        {
            "image/png" => "png",
            "image/webp" => "webp",
            _ => "jpg"
        };

        var blobName = $"{userId}/profile.{extension}";
        var blobClient = containerClient.GetBlobClient(blobName);

        logger.LogInformation("Fazendo upload da foto de perfil do usuário {UserId}", userId);

        await DeleteExistingProfilePicturesAsync(userId);

        await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });

        return GenerateSasUrl(blobClient);
    }

    public async Task DeleteAsync(int userId)
    {
        logger.LogInformation("Deletando foto de perfil do usuário {UserId}", userId);
        await DeleteExistingProfilePicturesAsync(userId);
    }

    private async Task DeleteExistingProfilePicturesAsync(int userId)
    {
        var prefix = $"{userId}/profile.";
        await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix))
        {
            await containerClient.DeleteBlobIfExistsAsync(blobItem.Name);
        }
    }

    private static string GenerateSasUrl(BlobClient blobClient)
    {
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = blobClient.BlobContainerName,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddYears(5)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }
}
