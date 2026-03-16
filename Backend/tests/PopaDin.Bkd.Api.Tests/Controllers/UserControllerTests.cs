using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PopaDin.Bkd.Api.Controllers;
using PopaDin.Bkd.Api.Dtos.User;
using PopaDin.Bkd.Api.Tests.Helpers;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Api.Tests.Controllers;

public class UserControllerTests
{
    private readonly IUserService _userService = Substitute.For<IUserService>();
    private readonly UserController _sut;

    private const int AuthUserId = 1;

    public UserControllerTests()
    {
        _sut = new UserController(_userService);
        ControllerTestHelper.SetupAuthenticatedUser(_sut, AuthUserId);
    }

    #region CreateUser

    [Fact]
    public async Task CreateUser_WithValidRequest_ShouldReturn201Created()
    {
        var request = new CreateUserRequest { Name = "Test", Email = "test@test.com", Password = "pass123", Balance = 100 };
        _userService.CreateUserAsync(Arg.Any<User>()).Returns(new User { Id = 1, Name = "Test", Email = "test@test.com", Balance = 100 });

        var result = await _sut.CreateUser(request);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task CreateUser_WithExistingEmail_ShouldThrowUnprocessableEntityException()
    {
        var request = new CreateUserRequest { Name = "Test", Email = "existing@test.com", Password = "pass123" };
        _userService.CreateUserAsync(Arg.Any<User>())
            .Throws(new UnprocessableEntityException("Email já cadastrado."));

        var act = () => _sut.CreateUser(request);

        await act.Should().ThrowAsync<UnprocessableEntityException>();
    }

    #endregion

    #region GetUsers

    [Fact]
    public async Task GetUsers_ShouldReturnOkWithPaginatedResult()
    {
        var request = new ListUsersRequest();
        _userService.GetUsersAsync(Arg.Any<ListUsers>(), AuthUserId)
            .Returns(new PaginatedResult<User> { Lines = [new User { Id = 1 }], Page = 1, TotalItems = 1 });

        var result = await _sut.GetUsers(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    #endregion

    #region FindUserById

    [Fact]
    public async Task FindUserById_WhenExists_ShouldReturnOk()
    {
        _userService.FindUserByIdAsync(1, AuthUserId)
            .Returns(new User { Id = 1, Name = "Test" });

        var result = await _sut.FindUserById(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task FindUserById_WhenNotFound_ShouldThrowNotFoundException()
    {
        _userService.FindUserByIdAsync(999, AuthUserId)
            .Throws(new NotFoundException("User não encontrado"));

        var act = () => _sut.FindUserById(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region UpdateUser

    [Fact]
    public async Task UpdateUser_WithValidRequest_ShouldReturnOk()
    {
        var request = new UpdateUserRequest { Name = "Updated" };
        _userService.UpdateUserAsync(Arg.Any<User>(), 1, AuthUserId)
            .Returns(new User { Id = 1, Name = "Updated" });

        var result = await _sut.UpdateUser(request, 1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateUser_WhenNotFound_ShouldThrowNotFoundException()
    {
        var request = new UpdateUserRequest { Name = "Updated" };
        _userService.UpdateUserAsync(Arg.Any<User>(), 999, AuthUserId)
            .Throws(new NotFoundException("User não encontrado"));

        var act = () => _sut.UpdateUser(request, 999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeleteUser

    [Fact]
    public async Task DeleteUser_WhenExists_ShouldReturn204NoContent()
    {
        var result = await _sut.DeleteUser(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteUser_WhenNotFound_ShouldThrowNotFoundException()
    {
        _userService.DeleteUserAsync(999, AuthUserId)
            .Throws(new NotFoundException("User não encontrado"));

        var act = () => _sut.DeleteUser(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
