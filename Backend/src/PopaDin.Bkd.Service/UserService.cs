using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Interfaces.Repositories;

namespace PopaDin.Bkd.Service;

public class UserService(
    IUserRepository repository,
    IUserCacheRepository cacheRepository,
    IProfilePictureBlobRepository profilePictureBlobRepository,
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

        var cached = await cacheRepository.GetAsync(userId);
        if (cached != null)
        {
            logger.LogInformation("User encontrado no cache para o usuário {UserId}", userId);
            return cached;
        }

        var user = await FindUserOrThrowAsync(userId);
        await cacheRepository.SetAsync(userId, user);
        return user;
    }

    public async Task<User> UpdateUserAsync(User updateUserRequest, int userId, int authenticatedUserId)
    {
        logger.LogInformation("Editando um User");
        ValidateUserOwnership(userId, authenticatedUserId);
        User user = await FindUserOrThrowAsync(userId);

        user.Name = updateUserRequest.Name;

        if (!string.IsNullOrEmpty(updateUserRequest.Password))
        {
            user.Password = passwordHasher.HashPassword(updateUserRequest.Password);
        }

        await repository.UpdateUserAsync(user);
        await cacheRepository.InvalidateAsync(userId);

        return await repository.FindUserByIdAsync(userId);
    }

    public async Task DeleteUserAsync(int userId, int authenticatedUserId)
    {
        ValidateUserOwnership(userId, authenticatedUserId);
        await FindUserOrThrowAsync(userId);
        await repository.DeleteUserAsync(userId);
        await cacheRepository.InvalidateAsync(userId);
    }

    public async Task<User> AdjustBalanceAsync(int userId, int authenticatedUserId, decimal newBalance)
    {
        logger.LogInformation("Ajustando saldo do User: {UserId}", userId);
        ValidateUserOwnership(userId, authenticatedUserId);
        await FindUserOrThrowAsync(userId);

        await repository.SetBalanceAsync(userId, newBalance);
        await cacheRepository.InvalidateAsync(userId);

        return await repository.FindUserByIdAsync(userId);
    }

    public async Task<string> UploadProfilePictureAsync(int userId, int authenticatedUserId, Stream fileStream, string contentType)
    {
        logger.LogInformation("Fazendo upload da foto de perfil do User: {UserId}", userId);
        ValidateUserOwnership(userId, authenticatedUserId);
        await FindUserOrThrowAsync(userId);

        var url = await profilePictureBlobRepository.UploadAsync(userId, fileStream, contentType);
        await repository.UpdateProfilePictureUrlAsync(userId, url);
        await cacheRepository.InvalidateAsync(userId);

        return url;
    }

    public async Task DeleteProfilePictureAsync(int userId, int authenticatedUserId)
    {
        logger.LogInformation("Deletando foto de perfil do User: {UserId}", userId);
        ValidateUserOwnership(userId, authenticatedUserId);
        await FindUserOrThrowAsync(userId);

        await profilePictureBlobRepository.DeleteAsync(userId);
        await repository.UpdateProfilePictureUrlAsync(userId, null);
        await cacheRepository.InvalidateAsync(userId);
    }

    private void ValidateUserOwnership(int userId, int authenticatedUserId)
    {
        if (userId != authenticatedUserId)
        {
            logger.LogWarning("User {AuthenticatedUserId} tentou acessar dados do usuário {UserId}", authenticatedUserId, userId);
            throw new NotFoundException("User não encontrado");
        }
    }

    private async Task<User> FindUserOrThrowAsync(int userId)
    {
        User user = await repository.FindUserByIdAsync(userId);

        if (user == null)
        {
            logger.LogWarning("User não encontrado. UserId: {UserId}", userId);
            throw new NotFoundException("User não encontrado");
        }

        return user;
    }
}
