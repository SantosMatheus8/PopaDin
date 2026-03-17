namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IProfilePictureBlobRepository
{
    Task<string> UploadAsync(int userId, Stream fileStream, string contentType);
    Task DeleteAsync(int userId);
}
