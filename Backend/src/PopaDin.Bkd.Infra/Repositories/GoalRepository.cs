using Dapper;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Infra.Queries;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Utils;

namespace PopaDin.Bkd.Infra.Repositories;

public class GoalRepository(IDbConnectionFactory connectionFactory, ILogger<GoalRepository> logger) : IGoalRepository
{
    public async Task<Goal> CreateGoalAsync(Goal goal)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Criando Meta no banco de dados");
            var goalCreated = await connection.QueryAsync<Goal>(GoalQueries.CreateGoal, new
            {
                Name = goal.Name,
                TargetAmount = goal.TargetAmount,
                Deadline = goal.Deadline,
                UserId = goal.User.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }, transaction);
            transaction.Commit();

            return goalCreated.FirstOrDefault()!;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao Criar Meta: {Message}", e.Message);
            throw;
        }
    }

    public async Task<PaginatedResult<Goal>> GetGoalsAsync(ListGoals listGoals)
    {
        var query = AddQueryPagination(listGoals);
        var countQuery = AddFilters(listGoals, GoalQueries.Count);

        logger.LogInformation("Listando Metas com paginação");

        var parameters = new
        {
            Id = listGoals.Id,
            Name = listGoals.Name,
            TargetAmount = listGoals.TargetAmount,
            UserId = listGoals.UserId,
            Offset = (listGoals.Page - 1) * listGoals.ItemsPerPage,
            listGoals.ItemsPerPage
        };

        using var connection = connectionFactory.CreateConnection();

        var result = await connection.QueryAsync<Goal, User, Goal>(
            query,
            (goal, user) =>
            {
                goal.User = user;
                return goal;
            },
            parameters,
            splitOn: "UserId"
        );

        var totalLines = await connection.QuerySingleAsync<int>(countQuery, parameters);

        return new PaginatedResult<Goal>
        {
            Lines = result.ToList(),
            Page = listGoals.Page,
            TotalPages = (int)Math.Ceiling(totalLines / (double)listGoals.ItemsPerPage),
            TotalItems = totalLines,
            PageSize = listGoals.ItemsPerPage
        };
    }

    private static string AddQueryPagination(ListGoals listGoals)
    {
        var query = AddFilters(listGoals, GoalQueries.ListGoals);
        query +=
            @$"
                ORDER BY
                {listGoals.OrderBy.GetEnumDescription()}
                {listGoals.OrderDirection.GetEnumDescription()}
                OFFSET @Offset
                ROWS FETCH NEXT @ItemsPerPage ROWS ONLY
                ";
        return query;
    }

    private static string AddFilters(ListGoals listGoals, string query)
    {
        query += " AND g.UserId = @UserId ";
        if (listGoals.Id.HasValue)
            query += " AND g.Id = @Id ";
        if (!string.IsNullOrEmpty(listGoals.Name))
            query += " AND LOWER(g.Name) COLLATE Latin1_General_CI_AI LIKE '%' + @Name + '%' ";
        if (listGoals.TargetAmount.HasValue)
            query += " AND g.TargetAmount = @TargetAmount ";
        return query;
    }

    public async Task<Goal> FindGoalByIdAsync(int goalId, int userId)
    {
        logger.LogInformation("Buscando Meta: {GoalId}", goalId);

        using var connection = connectionFactory.CreateConnection();

        var result = await connection.QueryAsync<Goal, User, Goal>(
            GoalQueries.FindGoalById,
            (goal, user) =>
            {
                goal.User = user;
                return goal;
            },
            new { GoalId = goalId, UserId = userId },
            splitOn: "UserId"
        );

        return result.FirstOrDefault()!;
    }

    public async Task UpdateGoalAsync(Goal goal)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Atualizando Meta: {GoalId}", goal.Id);
            await connection.ExecuteAsync(GoalQueries.UpdateGoal,
                new
                {
                    GoalId = goal.Id,
                    Name = goal.Name,
                    TargetAmount = goal.TargetAmount,
                    Deadline = goal.Deadline,
                    UpdatedAt = DateTime.UtcNow
                }, transaction);
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao editar Meta: {Message}", e.Message);
            throw;
        }
    }

    public async Task DeleteGoalAsync(int goalId)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Deletando Meta: {GoalId}", goalId);
            await connection.ExecuteAsync(GoalQueries.DeleteGoal,
                new { GoalId = goalId }, transaction);
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao deletar Meta: {Message}", e.Message);
            throw;
        }
    }

    public async Task FinishGoalAsync(int goalId)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Finalizando Meta: {GoalId}", goalId);
            await connection.ExecuteAsync(GoalQueries.FinishGoal,
                new
                {
                    GoalId = goalId,
                    FinishAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }, transaction);
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            logger.LogError("Erro ao finalizar Meta: {Message}", e.Message);
            throw;
        }
    }
}
