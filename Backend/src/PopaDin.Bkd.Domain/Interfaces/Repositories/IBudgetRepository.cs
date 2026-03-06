using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IBudgetRepository
{
    Task<Budget> CreateBudgetAsync(Budget budget);
    Task<PaginatedResult<Budget>> GetBudgetsAsync(ListBudgets listBudgets);
    Task<Budget> FindBudgetByIdAsync(int budgetId, int userId);
    Task UpdateBudgetAsync(Budget budget);
    Task DeleteBudgetAsync(int budgetId);
    Task FinishBudgetAsync(int budgetId);
}
