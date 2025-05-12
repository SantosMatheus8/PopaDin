public interface IAuthService
{
    Task<string> GenerateToken(string email, string password);
    // Task<UserTokenDTO> GetProfile(string token);
}