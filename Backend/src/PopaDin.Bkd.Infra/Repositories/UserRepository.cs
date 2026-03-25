using Dapper;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Infra.Queries;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Utils;

namespace PopaDin.Bkd.Infra.Repositories;

public class UserRepository(IDbConnectionFactory connectionFactory, ILogger<UserRepository> logger) : IUserRepository
{
    public async Task<User> CreateUserAsync(User user)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Criando User no banco de dados");
            var userCreated = await connection.QueryAsync<User>(UserQueries.CreateUser, new
            {
                Name = user.Name,
                Email = user.Email,
                Balance = user.Balance,
                Password = user.Password,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }, transaction);
            transaction.Commit();

            return userCreated.FirstOrDefault()!;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao Criar User: {Message}", e.Message);
            throw;
        }
    }

    public async Task<PaginatedResult<User>> GetUsersAsync(ListUsers listUsers)
    {
        var query = AddQueryPagination(listUsers);
        var countQuery = AddFilters(listUsers, UserQueries.Count);

        logger.LogInformation("Listando Users com paginação");

        var parameters = new
        {
            Id = listUsers.Id,
            Name = listUsers.Name,
            Email = listUsers.Email,
            Balance = listUsers.Balance,
            Offset = (listUsers.Page - 1) * listUsers.ItemsPerPage,
            listUsers.ItemsPerPage
        };

        using var connection = connectionFactory.CreateConnection();

        var result = await connection.QueryAsync<User>(query, parameters);
        var totalLines = await connection.QuerySingleAsync<int>(countQuery, parameters);

        return new PaginatedResult<User>
        {
            Lines = result.ToList(),
            Page = listUsers.Page,
            TotalPages = (int)Math.Ceiling(totalLines / (double)listUsers.ItemsPerPage),
            TotalItems = totalLines,
            PageSize = listUsers.ItemsPerPage
        };
    }

    private static string AddQueryPagination(ListUsers listUsers)
    {
        var query = AddFilters(listUsers, UserQueries.ListUsers);
        query +=
            @$"
                ORDER BY
                {listUsers.OrderBy.GetEnumDescription()}
                {listUsers.OrderDirection.GetEnumDescription()}
                OFFSET @Offset
                ROWS FETCH NEXT @ItemsPerPage ROWS ONLY
                ";
        return query;
    }

    private static string AddFilters(ListUsers listUsers, string query)
    {
        if (listUsers.Id.HasValue)
            query += " AND u.Id = @Id ";
        if (!string.IsNullOrEmpty(listUsers.Name))
            query += " AND LOWER(u.Name) COLLATE Latin1_General_CI_AI LIKE '%' + @Name + '%' ";
        if (!string.IsNullOrEmpty(listUsers.Email))
            query += " AND LOWER(u.Email) COLLATE Latin1_General_CI_AI LIKE '%' + @Email + '%' ";
        if (listUsers.Balance.HasValue)
            query += " AND u.Balance = @Balance ";
        return query;
    }

    public async Task<User> FindUserByIdAsync(int userId)
    {
        logger.LogInformation("Buscando User por Id: {UserId}", userId);

        using var connection = connectionFactory.CreateConnection();

        var response = await connection.QueryFirstOrDefaultAsync<User>(UserQueries.FindUserById,
            new { UserId = userId });

        return response!;
    }

    public async Task UpdateUserAsync(User user)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Atualizando User: {UserId}", user.Id);
            await connection.ExecuteAsync(UserQueries.UpdateUser,
                new
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Password = user.Password,
                    Balance = user.Balance,
                    UpdatedAt = DateTime.UtcNow
                }, transaction);
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao editar User: {Message}", e.Message);
            throw;
        }
    }

    public async Task DeleteUserAsync(int userId)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Deletando User: {UserId}", userId);
            await connection.ExecuteAsync(UserQueries.DeleteUser,
                new { UserId = userId }, transaction);
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao deletar User: {Message}", e.Message);
            throw;
        }
    }

    public async Task<User> FindUserByEmailAsync(string userEmail)
    {
        logger.LogInformation("Buscando User por Email");

        using var connection = connectionFactory.CreateConnection();

        var response = await connection.QueryFirstOrDefaultAsync<User>(UserQueries.FindUserByEmail,
            new { UserEmail = userEmail });

        return response!;
    }

    public async Task UpdateProfilePictureUrlAsync(int userId, string? url)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Atualizando ProfilePictureUrl do User: {UserId}", userId);
            await connection.ExecuteAsync(UserQueries.UpdateProfilePictureUrl,
                new { UserId = userId, ProfilePictureUrl = url, UpdatedAt = DateTime.UtcNow },
                transaction);
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao atualizar ProfilePictureUrl do User: {Message}", e.Message);
            throw;
        }
    }

    public async Task UpdateBalanceAsync(int userId, decimal amount)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Atualizando Balance do User: {UserId}", userId);
            await connection.ExecuteAsync(UserQueries.UpdateBalance,
                new { UserId = userId, Amount = amount, UpdatedAt = DateTime.UtcNow },
                transaction);
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao atualizar Balance do User: {Message}", e.Message);
            throw;
        }
    }

    public async Task SetBalanceAsync(int userId, decimal balance)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Definindo Balance do User: {UserId} para {Balance}", userId, balance);
            await connection.ExecuteAsync(UserQueries.SetBalance,
                new { UserId = userId, Balance = balance, UpdatedAt = DateTime.UtcNow },
                transaction);
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao definir Balance do User: {Message}", e.Message);
            throw;
        }
    }
}
