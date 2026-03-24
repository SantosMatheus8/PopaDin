using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IGoalRepository
{
    Task<Goal> CreateGoalAsync(Goal goal);
    Task<PaginatedResult<Goal>> GetGoalsAsync(ListGoals listGoals);
    Task<Goal> FindGoalByIdAsync(int goalId, int userId);
    Task UpdateGoalAsync(Goal goal);
    Task DeleteGoalAsync(int goalId);
    Task FinishGoalAsync(int goalId);
}
