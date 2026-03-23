using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PopaDin.Bkd.Service;

public class AuthService(
    IUserRepository repository,
    IPasswordHasher passwordHasher,
    IConfiguration configuration,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<string> GenerateToken(string email, string password)
    {
        var user = await repository.FindUserByEmailAsync(email);

        if (user == null || !passwordHasher.VerifyPassword(password, user.Password))
        {
            logger.LogWarning("Tentativa de login com credenciais inválidas para o email: {Email}", email);
            throw new UnauthorizedException("Credenciais inválidas");
        }

        var secret = configuration["AppSettings:Secret"];
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("name", user.Name)
        };

        var issuer = configuration["JwtSettings:Issuer"] ?? "PopaDin.Api";
        var audience = configuration["JwtSettings:Audience"] ?? "PopaDin.Client";

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            expires: DateTime.UtcNow.AddDays(7),
            claims: claims,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<User> GetProfile(int userId)
    {
        var user = await repository.FindUserByIdAsync(userId);

        if (user == null)
        {
            throw new NotFoundException("User não encontrado");
        }

        return user;
    }
}
