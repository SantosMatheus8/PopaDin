using Microsoft.IdentityModel.Tokens;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Helpers;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PopaDin.Application.Services;

public class AuthService(IUserRepository repository, ILogger<AuthService> logger) : IAuthService
{
    public async Task<string> GenerateToken(string email, string password)
    {
        var user = await repository.FindUserByEmailAsync(email);

        if (user == null || !Hash.CheckPassword(password, user.Password))
        {
            logger.LogInformation("User nao encontrado");
            throw new PopaBaseException("User não encontrado", 404);
        }

        var issuer = string.Empty;
        var audience = string.Empty;
        var expiry = DateTime.Now.AddDays(7);
        var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build();

        var secret = configuration["AppSettings:Secret"];
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("name", user.Name)
        };

        var token = new JwtSecurityToken

        (
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiry,
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var stringToken = tokenHandler.WriteToken(token);

        return stringToken;
        // return new LoginResponseDTO { Access_token = stringToken };
    }

    // public async Task<UserTokenDTO> GetProfile(string token)
    // {

    //     var tokenHandler = new JwtSecurityTokenHandler();
    //     var configuration = new ConfigurationBuilder()
    //         .SetBasePath(Directory.GetCurrentDirectory())
    //         .AddJsonFile("appsettings.json")
    //         .Build();

    //     var secret = configuration["AppSettings:Secret"];
    //     var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

    //     var tokenValidationParameters = new TokenValidationParameters
    //     {
    //         ValidateIssuerSigningKey = true,
    //         IssuerSigningKey = securityKey,
    //         ValidateIssuer = false,
    //         ValidateAudience = false
    //     };

    //     ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
    //     var tokenString = tokenHandler.WriteToken(validatedToken);
    //     var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);

    //     var userClaims = new UserTokenDTO
    //     {
    //         Id = int.Parse(jwtToken.Claims.First(claim => claim.Type == "sub").Value),
    //         Email = jwtToken.Claims.First(claim => claim.Type == "email").Value,
    //         Name = jwtToken.Claims.First(claim => claim.Type == "name").Value,
    //     };
    //     userClaims.Balance = await _userRepository.GetBalance(userClaims.Id);

    //     return userClaims;
    // }
}
