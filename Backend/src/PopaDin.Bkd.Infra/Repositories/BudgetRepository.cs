using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Infra.Queries;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Utils;
using PopaDin.Bkd.Domain.Models.Budget;
using PopaDin.Bkd.Domain.Models.User;

namespace PopaDin.Bkd.Infra.Repositories;

public class BudgetRepository(SqlConnection connection, ILogger<BudgetRepository> logger) : IBudgetRepository
{
    public async Task<Budget> CreateBudgetAsync(Budget budget)
    {
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        try
        {
            logger.LogInformation("Query a ser executada: {Sql}.", BudgetQueries.CreateBudget);
            var budgetCreated = await connection.QueryAsync<Budget>(BudgetQueries.CreateBudget, new
            {
                Name = budget.Name,
                Goal = budget.Goal,
                UserId = budget.User.Id,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }, transaction);
            await transaction.CommitAsync();

            return budgetCreated.FirstOrDefault()!;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError("Erro ao Criar Budget : {Erro}", e);
            throw;
        }
    }

    public async Task<PaginatedResult<Budget>> GetBudgetsAsync(ListBudgets listBudgets)
    {
        var query = AddQueryPagination(listBudgets);
        var countQuery = AddFilters(listBudgets, BudgetQueries.Count);

        logger.LogInformation("Query a ser executada: {Sql}. with parameters: {@Parameters}", query, listBudgets);

        var parameters = new
        {
            Id = listBudgets.Id,
            Name = listBudgets.Name,
            Goal = listBudgets.Goal,
            UserId = listBudgets.UserId,
            Offset = (listBudgets.Page - 1) * listBudgets.ItemsPerPage,
            listBudgets.ItemsPerPage
        };

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

        logger.LogInformation("Resultado: {@Resultado}. ", result);

        return new PaginatedResult<Budget>
        {
            Lines = result.ToList(),
            Page = listBudgets.Page,
            TotalPages = (int)Math.Ceiling(totalLines / (double)listBudgets.ItemsPerPage),
            TotalItens = totalLines,
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

    public async Task<Budget> FindBudgetByIdAsync(decimal budgetId, decimal userId)
    {
        logger.LogInformation("Query executada: {Sql}.", BudgetQueries.FindBudgetById);

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

        var budget = result.FirstOrDefault();

        logger.LogInformation("Resultado: {@Resultado}. ", budget);

        return budget!;
    }

    public async Task UpdateBudgetAsync(Budget budget)
    {
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        try
        {
            logger.LogInformation("Query executada: {Sql}.", BudgetQueries.UpdateBudget);
            var response = await connection.ExecuteAsync(BudgetQueries.UpdateBudget,
                new
                {
                    BudgetId = budget.Id,
                    Name = budget.Name,
                    Goal = budget.Goal,
                    UpdatedAt = DateTime.Now
                }, transaction);
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError("Erro ao editar Budget : {Erro}", e);
            throw;
        }
    }

    public async Task DeleteBudgetAsync(decimal budgetId)
    {
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        try
        {
            logger.LogInformation("Query executada: {Sql}.", BudgetQueries.DeleteBudget);
            var response = await connection.ExecuteAsync(BudgetQueries.DeleteBudget,
                new
                {
                    BudgetId = budgetId
                }, transaction);
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError("Erro ao deletar Budget : {Erro}", e);
            throw;
        }
    }

    public async Task FinishBudgetAsync(decimal budgetId)
    {
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        try
        {
            logger.LogInformation("Query executada: {Sql}.", BudgetQueries.FinishBudget);
            var response = await connection.ExecuteAsync(BudgetQueries.FinishBudget,
                new
                {
                    BudgetId = budgetId,
                    FinishAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }, transaction);
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError("Erro ao finalizar Budget : {Erro}", e);
            throw;
        }
    }
}
