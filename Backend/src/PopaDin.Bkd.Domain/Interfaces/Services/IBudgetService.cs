using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IBudgetService
{
    Task<Budget> CreateBudgetAsync(Budget budget, int userId);
    Task<PaginatedResult<Budget>> GetBudgetsAsync(ListBudgets listBudgets, int userId);
    Task<Budget> FindBudgetByIdAsync(int budgetId, int userId);
    Task<Budget> UpdateBudgetAsync(Budget updateBudgetRequest, int budgetId, int userId);
    Task DeleteBudgetAsync(int budgetId, int userId);
    Task FinishBudgetAsync(int budgetId, int userId);
}
