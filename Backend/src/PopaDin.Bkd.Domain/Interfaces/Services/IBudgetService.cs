using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IBudgetService
{
    Task<Budget> CreateBudgetAsync(Budget budget);
    // Task<List<Budget>> GetUserBudgets(int userId);
    // Task<Budget> FindBudgetById(int id, int userId);
    // Task<Budget> UpdateBudget(Budget updateBudgetRequest, int id, int userId);
    // Task<Budget> DeleteBudget(int id, int userId);
}