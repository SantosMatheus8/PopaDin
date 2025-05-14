using PopaDin.Bkd.Domain.Models.User;

public interface IAuthService
{
    Task<string> GenerateToken(string email, string password);
    Task<User> GetProfile(string token);
}