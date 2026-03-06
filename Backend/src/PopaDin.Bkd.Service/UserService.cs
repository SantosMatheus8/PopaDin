using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Interfaces.Repositories;

namespace PopaDin.Bkd.Service;

public class UserService(
    IUserRepository repository,
    IPasswordHasher passwordHasher,
    ILogger<UserService> logger) : IUserService
{
    public async Task<User> CreateUserAsync(User user)
    {
        logger.LogInformation("Criando User");

        user.ValidateBalance();

        var existingUser = await repository.FindUserByEmailAsync(user.Email);
        if (existingUser != null)
            throw new UnprocessableEntityException("Email já cadastrado.");

        user.Password = passwordHasher.HashPassword(user.Password);

        return await repository.CreateUserAsync(user);
    }

    public async Task<PaginatedResult<User>> GetUsersAsync(ListUsers listUsers, int userId)
    {
        logger.LogInformation("Listando User");
        listUsers.Id = userId;
        return await repository.GetUsersAsync(listUsers);
    }

    public async Task<User> FindUserByIdAsync(int userId, int authenticatedUserId)
    {
        logger.LogInformation("Buscando um User");
        ValidateUserOwnership(userId, authenticatedUserId);
        return await FindUserOrThrowAsync(userId);
    }

    public async Task<User> UpdateUserAsync(User updateUserRequest, int userId, int authenticatedUserId)
    {
        logger.LogInformation("Editando um User");
        ValidateUserOwnership(userId, authenticatedUserId);
        User user = await FindUserOrThrowAsync(userId);

        user.Name = updateUserRequest.Name;
        user.Balance = updateUserRequest.Balance;

        if (!string.IsNullOrEmpty(updateUserRequest.Password))
        {
            user.Password = passwordHasher.HashPassword(updateUserRequest.Password);
        }

        await repository.UpdateUserAsync(user);

        return await repository.FindUserByIdAsync(userId);
    }

    public async Task DeleteUserAsync(int userId, int authenticatedUserId)
    {
        ValidateUserOwnership(userId, authenticatedUserId);
        await FindUserOrThrowAsync(userId);
        await repository.DeleteUserAsync(userId);
    }

    private void ValidateUserOwnership(int userId, int authenticatedUserId)
    {
        if (userId != authenticatedUserId)
        {
            logger.LogInformation("User tentou acessar dados de outro usuario");
            throw new NotFoundException("User não encontrado");
        }
    }

    private async Task<User> FindUserOrThrowAsync(int userId)
    {
        User user = await repository.FindUserByIdAsync(userId);

        if (user == null)
        {
            logger.LogInformation("User nao encontrado");
            throw new NotFoundException("User não encontrado");
        }

        return user;
    }
}
