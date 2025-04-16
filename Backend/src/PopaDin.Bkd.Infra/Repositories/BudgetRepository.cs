using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Infra.Queries;
using PopaDin.Bkd.Domain.Models;

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

}

