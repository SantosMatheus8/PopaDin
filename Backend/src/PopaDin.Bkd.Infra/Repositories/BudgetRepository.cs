using Dapper;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Infra.Queries;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Utils;

namespace PopaDin.Bkd.Infra.Repositories;

public class BudgetRepository(IDbConnectionFactory connectionFactory, ILogger<BudgetRepository> logger) : IBudgetRepository
{
    public async Task<Budget> CreateBudgetAsync(Budget budget)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Criando Budget no banco de dados");
            var budgetCreated = await connection.QueryAsync<Budget>(BudgetQueries.CreateBudget, new
            {
                Name = budget.Name,
                Goal = budget.Goal,
                UserId = budget.User.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }, transaction);
            transaction.Commit();

            return budgetCreated.FirstOrDefault()!;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao Criar Budget: {Message}", e.Message);
            throw;
        }
    }

    public async Task<PaginatedResult<Budget>> GetBudgetsAsync(ListBudgets listBudgets)
    {
        var query = AddQueryPagination(listBudgets);
        var countQuery = AddFilters(listBudgets, BudgetQueries.Count);

        logger.LogInformation("Listando Budgets com paginação");

        var parameters = new
        {
            Id = listBudgets.Id,
            Name = listBudgets.Name,
            Goal = listBudgets.Goal,
            UserId = listBudgets.UserId,
            Offset = (listBudgets.Page - 1) * listBudgets.ItemsPerPage,
            listBudgets.ItemsPerPage
        };

        using var connection = connectionFactory.CreateConnection();

        var result = await connection.QueryAsync<Budget, User, Budget>(
            query,
            (budget, user) =>
            {
                budget.User = user;
                return budget;
            },
            parameters,
            splitOn: "UserId"
        );

        var totalLines = await connection.QuerySingleAsync<int>(countQuery, parameters);

        return new PaginatedResult<Budget>
        {
            Lines = result.ToList(),
            Page = listBudgets.Page,
            TotalPages = (int)Math.Ceiling(totalLines / (double)listBudgets.ItemsPerPage),
            TotalItems = totalLines,
            PageSize = listBudgets.ItemsPerPage
        };
    }

    private static string AddQueryPagination(ListBudgets listBudgets)
    {
        var query = AddFilters(listBudgets, BudgetQueries.ListBudgets);
        query +=
            @$"
                ORDER BY
                {listBudgets.OrderBy.GetEnumDescription()}
                {listBudgets.OrderDirection.GetEnumDescription()}
                OFFSET @Offset
                ROWS FETCH NEXT @ItemsPerPage ROWS ONLY
                ";
        return query;
    }

    private static string AddFilters(ListBudgets listBudgets, string query)
    {
        query += " AND b.UserId = @UserId ";
        if (listBudgets.Id.HasValue)
            query += " AND b.Id = @Id ";
        if (!string.IsNullOrEmpty(listBudgets.Name))
            query += " AND LOWER(b.Name) COLLATE Latin1_General_CI_AI LIKE '%' + @Name + '%' ";
        if (listBudgets.Goal.HasValue)
            query += " AND b.Goal = @Goal ";
        return query;
    }

    public async Task<Budget> FindBudgetByIdAsync(int budgetId, int userId)
    {
        logger.LogInformation("Buscando Budget: {BudgetId}", budgetId);

        using var connection = connectionFactory.CreateConnection();

        var result = await connection.QueryAsync<Budget, User, Budget>(
            BudgetQueries.FindBudgetById,
            (budget, user) =>
            {
                budget.User = user;
                return budget;
            },
            new { BudgetId = budgetId, UserId = userId },
            splitOn: "UserId"
        );

        return result.FirstOrDefault()!;
    }

    public async Task UpdateBudgetAsync(Budget budget)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Atualizando Budget: {BudgetId}", budget.Id);
            await connection.ExecuteAsync(BudgetQueries.UpdateBudget,
                new
                {
                    BudgetId = budget.Id,
                    Name = budget.Name,
                    Goal = budget.Goal,
                    UpdatedAt = DateTime.UtcNow
                }, transaction);
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao editar Budget: {Message}", e.Message);
            throw;
        }
    }

    public async Task DeleteBudgetAsync(int budgetId)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Deletando Budget: {BudgetId}", budgetId);
            await connection.ExecuteAsync(BudgetQueries.DeleteBudget,
                new { BudgetId = budgetId }, transaction);
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao deletar Budget: {Message}", e.Message);
            throw;
        }
    }

    public async Task FinishBudgetAsync(int budgetId)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Finalizando Budget: {BudgetId}", budgetId);
            await connection.ExecuteAsync(BudgetQueries.FinishBudget,
                new
                {
                    BudgetId = budgetId,
                    FinishAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }, transaction);
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao finalizar Budget: {Message}", e.Message);
            throw;
        }
    }
}
