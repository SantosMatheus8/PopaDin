using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IBudgetService
{
    Task<Budget> CreateBudgetAsync(Budget budget);
    Task<PaginatedResult<Budget>> GetBudgetsAsync(ListBudgets listBudgets);
    // Task<Budget> FindBudgetById(int id, int userId);
    // Task<Budget> UpdateBudget(Budget updateBudgetRequest, int id, int userId);
    // Task<Budget> DeleteBudget(int id, int userId);
}