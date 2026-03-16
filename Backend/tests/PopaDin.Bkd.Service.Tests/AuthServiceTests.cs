using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Service;

namespace PopaDin.Bkd.Service.Tests;

public class AuthServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IConfiguration _configuration = Substitute.For<IConfiguration>();
    private readonly ILogger<AuthService> _logger = Substitute.For<ILogger<AuthService>>();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _configuration["AppSettings:Secret"].Returns("SuperSecretKeyForTestingPurposes1234567890!");
        _sut = new AuthService(_userRepository, _passwordHasher, _configuration, _logger);
    }

    #region GenerateToken

    [Fact]
    public async Task GenerateToken_WithValidCredentials_ShouldReturnToken()
    {
        var user = new User { Id = 1, Name = "Test", Email = "test@test.com", Password = "hashed" };
        _userRepository.FindUserByEmailAsync("test@test.com").Returns(user);
        _passwordHasher.VerifyPassword("password123", "hashed").Returns(true);

        var token = await _sut.GenerateToken("test@test.com", "password123");

        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateToken_WithInvalidEmail_ShouldThrowUnauthorizedException()
    {
        _userRepository.FindUserByEmailAsync("invalid@test.com").Returns((User?)null);

        var act = () => _sut.GenerateToken("invalid@test.com", "password123");

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Credenciais inválidas");
    }

    [Fact]
    public async Task GenerateToken_WithInvalidPassword_ShouldThrowUnauthorizedException()
    {
        var user = new User { Id = 1, Name = "Test", Email = "test@test.com", Password = "hashed" };
        _userRepository.FindUserByEmailAsync("test@test.com").Returns(user);
        _passwordHasher.VerifyPassword("wrongpassword", "hashed").Returns(false);

        var act = () => _sut.GenerateToken("test@test.com", "wrongpassword");

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Credenciais inválidas");
    }

    #endregion

    #region GetProfile

    [Fact]
    public async Task GetProfile_WithExistingUser_ShouldReturnUser()
    {
        var user = new User { Id = 1, Name = "Test", Email = "test@test.com" };
        _userRepository.FindUserByIdAsync(1).Returns(user);

        var result = await _sut.GetProfile(1);

        result.Should().Be(user);
    }

    [Fact]
    public async Task GetProfile_WithNonExistingUser_ShouldThrowNotFoundException()
    {
        _userRepository.FindUserByIdAsync(999).Returns((User?)null);

        var act = () => _sut.GetProfile(999);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User não encontrado");
    }

    #endregion
}
