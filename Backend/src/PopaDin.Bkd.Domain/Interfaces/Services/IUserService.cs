using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IUserService
{
    Task<User> CreateUserAsync(User user);
    Task<PaginatedResult<User>> GetUsersAsync(ListUsers listUsers, int userId);
    Task<User> FindUserByIdAsync(int userId, int authenticatedUserId);
    Task<User> UpdateUserAsync(User updateUserRequest, int userId, int authenticatedUserId);
    Task DeleteUserAsync(int userId, int authenticatedUserId);
    Task<User> AdjustBalanceAsync(int userId, int authenticatedUserId, decimal newBalance);
    Task<string> UploadProfilePictureAsync(int userId, int authenticatedUserId, Stream fileStream, string contentType);
    Task DeleteProfilePictureAsync(int userId, int authenticatedUserId);
}
