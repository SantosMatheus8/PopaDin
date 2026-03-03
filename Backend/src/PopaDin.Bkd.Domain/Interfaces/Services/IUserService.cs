using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.User;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IUserService
{
    Task<User> CreateUserAsync(User User);
    Task<PaginatedResult<User>> GetUsersAsync(ListUsers listUsers, decimal userId);
    Task<User> FindUserByIdAsync(decimal UserId, decimal authenticatedUserId);
    Task<User> UpdateUserAsync(User updateUserRequest, decimal UserId, decimal authenticatedUserId);
    Task DeleteUserAsync(decimal UserId, decimal authenticatedUserId);
}