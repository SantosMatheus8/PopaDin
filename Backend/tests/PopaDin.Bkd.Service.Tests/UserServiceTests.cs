using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Service;

namespace PopaDin.Bkd.Service.Tests;

public class UserServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUserCacheRepository _cacheRepository = Substitute.For<IUserCacheRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ILogger<UserService> _logger = Substitute.For<ILogger<UserService>>();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_userRepository, _cacheRepository, _passwordHasher, _logger);
    }

    #region CreateUserAsync

    [Fact]
    public async Task CreateUserAsync_WithValidUser_ShouldCreateAndReturn()
    {
        var user = new User { Name = "Test", Email = "test@test.com", Password = "pass123", Balance = 100 };
        var createdUser = new User { Id = 1, Name = "Test", Email = "test@test.com", Password = "hashed", Balance = 100 };

        _userRepository.FindUserByEmailAsync("test@test.com").Returns((User?)null);
        _passwordHasher.HashPassword("pass123").Returns("hashed");
        _userRepository.CreateUserAsync(Arg.Any<User>()).Returns(createdUser);

        var result = await _sut.CreateUserAsync(user);

        result.Should().Be(createdUser);
        _passwordHasher.Received(1).HashPassword("pass123");
    }

    [Fact]
    public async Task CreateUserAsync_WithExistingEmail_ShouldThrowUnprocessableEntityException()
    {
        var user = new User { Name = "Test", Email = "existing@test.com", Password = "pass123", Balance = 0 };
        _userRepository.FindUserByEmailAsync("existing@test.com").Returns(new User { Id = 1 });

        var act = () => _sut.CreateUserAsync(user);

        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .WithMessage("Email já cadastrado.");
    }

    [Fact]
    public async Task CreateUserAsync_WithNegativeBalance_ShouldThrowException()
    {
        var user = new User { Name = "Test", Email = "test@test.com", Password = "pass123", Balance = -1 };

        var act = () => _sut.CreateUserAsync(user);

        await act.Should().ThrowAsync<UnprocessableEntityException>();
    }

    #endregion

    #region GetUsersAsync

    [Fact]
    public async Task GetUsersAsync_ShouldSetUserIdAndReturnPaginatedResult()
    {
        var listUsers = new ListUsers { Page = 1, ItemsPerPage = 20 };
        var expected = new PaginatedResult<User> { Lines = [new User { Id = 1 }], Page = 1, TotalItems = 1 };
        _userRepository.GetUsersAsync(Arg.Any<ListUsers>()).Returns(expected);

        var result = await _sut.GetUsersAsync(listUsers, 1);

        result.Should().Be(expected);
        listUsers.Id.Should().Be(1);
    }

    #endregion

    #region FindUserByIdAsync

    [Fact]
    public async Task FindUserByIdAsync_WhenCached_ShouldReturnFromCache()
    {
        var cachedUser = new User { Id = 1, Name = "Cached" };
        _cacheRepository.GetAsync(1).Returns(cachedUser);

        var result = await _sut.FindUserByIdAsync(1, 1);

        result.Should().Be(cachedUser);
        await _userRepository.DidNotReceive().FindUserByIdAsync(1);
    }

    [Fact]
    public async Task FindUserByIdAsync_WhenNotCached_ShouldFetchAndCache()
    {
        var user = new User { Id = 1, Name = "Test" };
        _cacheRepository.GetAsync(1).Returns((User?)null);
        _userRepository.FindUserByIdAsync(1).Returns(user);

        var result = await _sut.FindUserByIdAsync(1, 1);

        result.Should().Be(user);
        await _cacheRepository.Received(1).SetAsync(1, user);
    }

    [Fact]
    public async Task FindUserByIdAsync_WhenDifferentUserId_ShouldThrowNotFoundException()
    {
        var act = () => _sut.FindUserByIdAsync(1, 2);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User não encontrado");
    }

    [Fact]
    public async Task FindUserByIdAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _cacheRepository.GetAsync(1).Returns((User?)null);
        _userRepository.FindUserByIdAsync(1).Returns((User?)null);

        var act = () => _sut.FindUserByIdAsync(1, 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region UpdateUserAsync

    [Fact]
    public async Task UpdateUserAsync_WithValidData_ShouldUpdateAndInvalidateCache()
    {
        var existingUser = new User { Id = 1, Name = "Old", Balance = 50, Password = "old" };
        var updateRequest = new User { Name = "New", Balance = 100 };
        var updatedUser = new User { Id = 1, Name = "New", Balance = 100 };

        _userRepository.FindUserByIdAsync(1).Returns(existingUser, updatedUser);

        var result = await _sut.UpdateUserAsync(updateRequest, 1, 1);

        result.Name.Should().Be("New");
        await _cacheRepository.Received(1).InvalidateAsync(1);
    }

    [Fact]
    public async Task UpdateUserAsync_WithPassword_ShouldHashPassword()
    {
        var existingUser = new User { Id = 1, Name = "Old", Balance = 50, Password = "old" };
        var updateRequest = new User { Name = "New", Balance = 100, Password = "newpass" };
        var updatedUser = new User { Id = 1, Name = "New", Balance = 100 };

        _userRepository.FindUserByIdAsync(1).Returns(existingUser, updatedUser);
        _passwordHasher.HashPassword("newpass").Returns("newhashed");

        await _sut.UpdateUserAsync(updateRequest, 1, 1);

        _passwordHasher.Received(1).HashPassword("newpass");
    }

    [Fact]
    public async Task UpdateUserAsync_WithDifferentUserId_ShouldThrowNotFoundException()
    {
        var act = () => _sut.UpdateUserAsync(new User(), 1, 2);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeleteUserAsync

    [Fact]
    public async Task DeleteUserAsync_WithValidUser_ShouldDeleteAndInvalidateCache()
    {
        var user = new User { Id = 1, Name = "Test" };
        _userRepository.FindUserByIdAsync(1).Returns(user);

        await _sut.DeleteUserAsync(1, 1);

        await _userRepository.Received(1).DeleteUserAsync(1);
        await _cacheRepository.Received(1).InvalidateAsync(1);
    }

    [Fact]
    public async Task DeleteUserAsync_WithDifferentUserId_ShouldThrowNotFoundException()
    {
        var act = () => _sut.DeleteUserAsync(1, 2);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteUserAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _userRepository.FindUserByIdAsync(1).Returns((User?)null);

        var act = () => _sut.DeleteUserAsync(1, 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
