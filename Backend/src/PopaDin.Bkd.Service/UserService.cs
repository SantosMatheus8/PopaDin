using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.User;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Helpers;

namespace PopaDin.Bkd.Service;

public class UserService(IUserRepository repository, ILogger<UserService> logger) : IUserService
{
    public async Task<User> CreateUserAsync(User user)
    {
        logger.LogInformation("Criando User");

        if (user.Balance < 0)
        {
            throw new UnprocessableEntityException("O valor deve ser maior que zero.");
        }
        user.Password = Hash.HashPassword(user.Password);
        
        return await repository.CreateUserAsync(user);
    }

    public async Task<PaginatedResult<User>> GetUsersAsync(ListUsers listUsers, decimal userId)
    {
        logger.LogInformation("Listando User");
        listUsers.Id = (int)userId;
        return await repository.GetUsersAsync(listUsers);
    }

    public async Task<User> FindUserByIdAsync(decimal userId, decimal authenticatedUserId)
    {
        logger.LogInformation("Buscando um User");
        ValidateUserOwnership(userId, authenticatedUserId);
        return await FindUserOrThrowExceptionAsync(userId);
    }

    public async Task<User> UpdateUserAsync(User updateUserRequest, decimal userId, decimal authenticatedUserId)
    {
        logger.LogInformation("Editando um User");
        ValidateUserOwnership(userId, authenticatedUserId);
        User user = await FindUserOrThrowExceptionAsync(userId);

        user.Name = updateUserRequest.Name;
        user.Balance = updateUserRequest.Balance;

        if (!string.IsNullOrEmpty(updateUserRequest.Password))
        {
            user.Password = Hash.HashPassword(updateUserRequest.Password);
        }

        await repository.UpdateUserAsync(user);

        return await repository.FindUserByIdAsync(userId);
    }

    public async Task DeleteUserAsync(decimal userId, decimal authenticatedUserId)
    {
        ValidateUserOwnership(userId, authenticatedUserId);
        await FindUserOrThrowExceptionAsync(userId);
        await repository.DeleteUserAsync(userId);
    }

    private void ValidateUserOwnership(decimal userId, decimal authenticatedUserId)
    {
        if (userId != authenticatedUserId)
        {
            logger.LogInformation("User tentou acessar dados de outro usuario");
            throw new NotFoundException("User não encontrado");
        }
    }

    private async Task<User> FindUserOrThrowExceptionAsync(decimal userId)
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