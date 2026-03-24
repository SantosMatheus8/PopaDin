using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Service;

public class GoalService(
    IGoalRepository repository,
    IDashboardCacheRepository dashboardCacheRepository,
    ILogger<GoalService> logger) : IGoalService
{
    public async Task<Goal> CreateGoalAsync(Goal goal, int userId)
    {
        logger.LogInformation("Criando Meta");

        goal.ValidateTargetAmount();

        goal.User = new User { Id = userId };
        var goalCreated = await repository.CreateGoalAsync(goal);

        await dashboardCacheRepository.InvalidateAsync(userId);

        return await FindGoalOrThrowAsync(goalCreated.Id!.Value, userId);
    }

    public async Task<PaginatedResult<Goal>> GetGoalsAsync(ListGoals listGoals, int userId)
    {
        logger.LogInformation("Listando Metas");
        listGoals.UserId = userId;
        return await repository.GetGoalsAsync(listGoals);
    }

    public async Task<Goal> FindGoalByIdAsync(int goalId, int userId)
    {
        logger.LogInformation("Buscando uma Meta");
        return await FindGoalOrThrowAsync(goalId, userId);
    }

    public async Task<Goal> UpdateGoalAsync(Goal updateGoalRequest, int goalId, int userId)
    {
        logger.LogInformation("Editando uma Meta");

        updateGoalRequest.ValidateTargetAmount();

        Goal goal = await FindGoalOrThrowAsync(goalId, userId);

        goal.Name = updateGoalRequest.Name;
        goal.TargetAmount = updateGoalRequest.TargetAmount;
        goal.Deadline = updateGoalRequest.Deadline;
        await repository.UpdateGoalAsync(goal);

        await dashboardCacheRepository.InvalidateAsync(userId);

        return await FindGoalOrThrowAsync(goalId, userId);
    }

    public async Task DeleteGoalAsync(int goalId, int userId)
    {
        logger.LogInformation("Deletando Meta {GoalId} do usuário {UserId}", goalId, userId);
        await FindGoalOrThrowAsync(goalId, userId);
        await repository.DeleteGoalAsync(goalId);

        await dashboardCacheRepository.InvalidateAsync(userId);
    }

    public async Task FinishGoalAsync(int goalId, int userId)
    {
        await FindGoalOrThrowAsync(goalId, userId);
        await repository.FinishGoalAsync(goalId);

        await dashboardCacheRepository.InvalidateAsync(userId);
    }

    private async Task<Goal> FindGoalOrThrowAsync(int goalId, int userId)
    {
        Goal goal = await repository.FindGoalByIdAsync(goalId, userId);

        if (goal == null)
        {
            logger.LogWarning("Meta não encontrada. GoalId: {GoalId}, UserId: {UserId}", goalId, userId);
            throw new NotFoundException("Meta não encontrada");
        }

        return goal;
    }
}
