using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.User;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IUserService
{
    Task<User> CreateUserAsync(User User);
    Task<PaginatedResult<User>> GetUsersAsync(ListUsers listUsers);
    Task<User> FindUserByIdAsync(decimal UserId);
    Task<User> UpdateUserAsync(User updateUserRequest, decimal UserId);
    Task DeleteUserAsync(decimal UserId);
}