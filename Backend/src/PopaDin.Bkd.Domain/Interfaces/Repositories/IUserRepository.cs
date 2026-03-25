using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User> CreateUserAsync(User user);
    Task<PaginatedResult<User>> GetUsersAsync(ListUsers listUsers);
    Task<User> FindUserByIdAsync(int userId);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int userId);
    Task<User> FindUserByEmailAsync(string userEmail);
    Task UpdateBalanceAsync(int userId, decimal amount);
    Task SetBalanceAsync(int userId, decimal balance);
    Task UpdateProfilePictureUrlAsync(int userId, string? url);
}
