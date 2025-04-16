using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Infra.Queries;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Infra.Repositories;

public class BudgetRepository(SqlConnection connection, ILogger<BudgetRepository> logger) : IBudgetRepository
{
    public async Task<Budget> CriarBudgetAsync(Budget budget)
    {
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        try
        {
            var budgetCreated = await connection.QueryAsync<Budget>(BudgetQueries.CreateBudget, new
            {
                // Id = budget.Id,
                Name = budget.Name,
                Goal = budget.Goal,
                CurrentAmount = budget.CurrentAmount,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }, transaction);
            await transaction.CommitAsync();

            return budgetCreated.FirstOrDefault();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine(ex);
            throw;
        }
    }

}

