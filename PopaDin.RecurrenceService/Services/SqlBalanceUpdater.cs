using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using PopaDin.RecurrenceService.Interfaces;

namespace PopaDin.RecurrenceService.Services;

public class SqlBalanceUpdater(
    IConfiguration configuration,
    ILogger<SqlBalanceUpdater> logger) : IBalanceUpdater
{
    private const string UpdateBalanceSql = @"
        UPDATE [User]
        SET Balance = Balance + @Amount,
            UpdatedAt = @UpdatedAt
        WHERE Id = @UserId";

    public async Task UpdateBalanceAsync(int userId, decimal amount)
    {
        var connectionString = configuration.GetConnectionString("Database");

        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            await connection.ExecuteAsync(
                UpdateBalanceSql,
                new { UserId = userId, Amount = amount, UpdatedAt = DateTime.UtcNow },
                transaction);

            transaction.Commit();
            logger.LogInformation("Saldo do usuário {UserId} atualizado em {Amount}", userId, amount);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            logger.LogError(ex, "Erro ao atualizar saldo do usuário {UserId}", userId);
            throw;
        }
    }
}
