using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.Budget;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IBudgetService
{
    Task<Budget> CreateBudgetAsync(Budget budget, decimal userId);
    Task<PaginatedResult<Budget>> GetBudgetsAsync(ListBudgets listBudgets, decimal userId);
    Task<Budget> FindBudgetByIdAsync(decimal budgetId, decimal userId);
    Task<Budget> UpdateBudgetAsync(Budget updateBudgetRequest, decimal budgetId, decimal userId);
    Task DeleteBudgetAsync(decimal budgetId, decimal userId);
    Task FinishBudgetAsync(decimal budgetId, decimal userId);
}
