using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IAuthService
{
    Task<string> GenerateToken(string email, string password);
    Task<User> GetProfile(int userId);
}
