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

    public async Task<PaginatedResult<User>> GetUsersAsync(ListUsers listUsers)
    {
        logger.LogInformation("Listando User");
        return await repository.GetUsersAsync(listUsers);
    }

    public async Task<User> FindUserByIdAsync(decimal userId)
    {
        logger.LogInformation("Buscando um User");
        return await FindUserOrThrowExceptionAsync(userId);
    }

    public async Task<User> UpdateUserAsync(User updateUserRequest, decimal userId)
    {
        logger.LogInformation("Editando um User");
        User user = await FindUserOrThrowExceptionAsync(userId);

        user.Name = updateUserRequest.Name;
        user.Balance = updateUserRequest.Balance;
        user.Password = updateUserRequest.Password;
        await repository.UpdateUserAsync(user);

        return await repository.FindUserByIdAsync(userId);
    }

    public async Task DeleteUserAsync(decimal userId)
    {
        await FindUserOrThrowExceptionAsync(userId);
        await repository.DeleteUserAsync(userId);
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