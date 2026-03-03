using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.User;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User> CreateUserAsync(User user);
    Task<PaginatedResult<User>> GetUsersAsync(ListUsers listUsers);
    Task<User> FindUserByIdAsync(decimal userId);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(decimal userId);
    Task<User> FindUserByEmailAsync(string userEmail);
    Task UpdateBalanceAsync(decimal userId, double amount);
}