using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Infra.Queries;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Utils;

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
                CurrentAmount = budget.CurrentAmount,
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

        var result = await connection.QueryAsync<Budget>(
            query, new
            {
                Id = listBudgets.Id,
                Name = listBudgets.Name,
                Goal = listBudgets.Goal,
                CurrentAmount = listBudgets.CurrentAmount,
                Offset = (listBudgets.Page - 1) * listBudgets.ItemsPerPage,
                listBudgets.ItemsPerPage
            }
        );

        var totalLines = await connection.QuerySingleAsync<int>(countQuery, new
        {
            Id = listBudgets.Id,
            Name = listBudgets.Name,
            Goal = listBudgets.Goal,
            CurrentAmount = listBudgets.CurrentAmount,
            Offset = (listBudgets.Page - 1) * listBudgets.ItemsPerPage,
            listBudgets.ItemsPerPage
        });


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
        if (listBudgets.Id.HasValue)
            query += " AND b.Id = @Id ";
        if (!string.IsNullOrEmpty(listBudgets.Name))
            query += " AND LOWER(b.Name) COLLATE Latin1_General_CI_AI LIKE '%' + @Name + '%' ";
        if (listBudgets.Goal.HasValue)
            query += " AND b.Goal = @Goal ";
        if (listBudgets.CurrentAmount.HasValue)
            query += " AND b.CurrentAmount = @CurrentAmount ";
        return query;
    }

    public async Task<Budget> FindBudgetByIdAsync(decimal budgetId)
    {
        logger.LogInformation("Query executada: {Sql}.", BudgetQueries.FindBudgetById);

        var response = await connection.QueryFirstOrDefaultAsync<Budget>(BudgetQueries.FindBudgetById,
            new { BudgetId = budgetId });

        logger.LogInformation("Resultado: {@Resultado}. ", response);

        return response!;
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
                    CurrentAmount = budget.CurrentAmount,
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
}

