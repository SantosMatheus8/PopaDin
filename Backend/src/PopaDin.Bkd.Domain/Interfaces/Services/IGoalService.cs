using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IGoalService
{
    Task<Goal> CreateGoalAsync(Goal goal, int userId);
    Task<PaginatedResult<Goal>> GetGoalsAsync(ListGoals listGoals, int userId);
    Task<Goal> FindGoalByIdAsync(int goalId, int userId);
    Task<Goal> UpdateGoalAsync(Goal updateGoalRequest, int goalId, int userId);
    Task DeleteGoalAsync(int goalId, int userId);
    Task FinishGoalAsync(int goalId, int userId);
}
