using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Infra.Queries;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Utils;
using PopaDin.Bkd.Domain.Models.User;

namespace PopaDin.Bkd.Infra.Repositories;

public class UserRepository(SqlConnection connection, ILogger<UserRepository> logger) : IUserRepository
{
    public async Task<User> CreateUserAsync(User user)
    {
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        try
        {
            logger.LogInformation("Query a ser executada: {Sql}.", UserQueries.CreateUser);
            var userCreated = await connection.QueryAsync<User>(UserQueries.CreateUser, new
            {
                Name = user.Name,
                Email = user.Email,
                Balance = user.Balance,
                Password = user.Password,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }, transaction);
            await transaction.CommitAsync();

            return userCreated.FirstOrDefault()!;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError("Erro ao Criar User : {Erro}", e);
            throw;
        }
    }

    public async Task<PaginatedResult<User>> GetUsersAsync(ListUsers listUsers)
    {
        var query = AddQueryPagination(listUsers);
        var countQuery = AddFilters(listUsers, UserQueries.Count);

        logger.LogInformation("Query a ser executada: {Sql}. with parameters: {@Parameters}", query, listUsers);

        var result = await connection.QueryAsync<User>(
            query, new
            {
                Id = listUsers.Id,
                Name = listUsers.Name,
                Email = listUsers.Email,
                Balance = listUsers.Balance,
                Offset = (listUsers.Page - 1) * listUsers.ItemsPerPage,
                listUsers.ItemsPerPage
            }
        );

        var totalLines = await connection.QuerySingleAsync<int>(countQuery, new
        {
            Id = listUsers.Id,
            Name = listUsers.Name,
            Email = listUsers.Email,
            Balance = listUsers.Balance,
            Offset = (listUsers.Page - 1) * listUsers.ItemsPerPage,
            listUsers.ItemsPerPage
        });


        logger.LogInformation("Resultado: {@Resultado}. ", result);

        return new PaginatedResult<User>
        {
            Lines = result.ToList(),
            Page = listUsers.Page,
            TotalPages = (int)Math.Ceiling(totalLines / (double)listUsers.ItemsPerPage),
            TotalItens = totalLines,
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

    public async Task<User> FindUserByIdAsync(decimal userId)
    {
        logger.LogInformation("Query executada: {Sql}.", UserQueries.FindUserById);

        var response = await connection.QueryFirstOrDefaultAsync<User>(UserQueries.FindUserById,
            new { UserId = userId });

        logger.LogInformation("Resultado: {@Resultado}. ", response);

        return response!;
    }

    public async Task UpdateUserAsync(User user)
    {
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        try
        {
            logger.LogInformation("Query executada: {Sql}.", UserQueries.UpdateUser);
            var response = await connection.ExecuteAsync(UserQueries.UpdateUser,
                new
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Password = user.Password,
                    Balance = user.Balance,
                    UpdatedAt = DateTime.Now
                }, transaction);
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError("Erro ao editar User : {Erro}", e);
            throw;
        }
    }

    public async Task DeleteUserAsync(decimal userId)
    {
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        try
        {
            logger.LogInformation("Query executada: {Sql}.", UserQueries.DeleteUser);
            var response = await connection.ExecuteAsync(UserQueries.DeleteUser,
                new
                {
                    UserId = userId
                }, transaction);
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError("Erro ao deletar User : {Erro}", e);
            throw;
        }
    }

    public async Task<User> FindUserByEmailAsync(string userEmail)
    {
        logger.LogInformation("Query executada: {Sql}.", UserQueries.FindUserByEmail);

        var response = await connection.QueryFirstOrDefaultAsync<User>(UserQueries.FindUserByEmail,
            new { UserEmail = userEmail });

        logger.LogInformation("Resultado: {@Resultado}. ", response);

        return response!;
    }
}

