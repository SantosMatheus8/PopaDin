using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IUserCacheRepository
{
    Task<User?> GetAsync(int userId);
    Task SetAsync(int userId, User user);
    Task InvalidateAsync(int userId);
}
