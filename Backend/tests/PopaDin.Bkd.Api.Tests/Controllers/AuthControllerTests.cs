using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PopaDin.Bkd.Api.Controllers;
using PopaDin.Bkd.Api.Dtos.Auth;
using PopaDin.Bkd.Api.Tests.Helpers;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly IAuthService _authService = Substitute.For<IAuthService>();
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _sut = new AuthController(_authService);
        ControllerTestHelper.SetupAuthenticatedUser(_sut, 1);
    }

    #region Login

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithToken()
    {
        var request = new LoginRequest { Email = "test@test.com", Password = "pass123" };
        _authService.GenerateToken("test@test.com", "pass123").Returns("jwt-token");

        var result = await _sut.Login(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var response = okResult.Value.Should().BeOfType<LoginResponse>().Subject;
        response.AccessToken.Should().Be("jwt-token");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldThrowUnauthorizedException()
    {
        var request = new LoginRequest { Email = "bad@test.com", Password = "wrong" };
        _authService.GenerateToken("bad@test.com", "wrong")
            .Throws(new UnauthorizedException("Credenciais inválidas"));

        var act = () => _sut.Login(request);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    #endregion

    #region GetProfile

    [Fact]
    public async Task GetProfile_WithValidUser_ShouldReturnOkWithUserResponse()
    {
        var user = new User { Id = 1, Name = "Test", Email = "test@test.com", Balance = 500 };
        _authService.GetProfile(1).Returns(user);

        var result = await _sut.GetProfile();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetProfile_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        _authService.GetProfile(1).Throws(new NotFoundException("User não encontrado"));

        var act = () => _sut.GetProfile();

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
