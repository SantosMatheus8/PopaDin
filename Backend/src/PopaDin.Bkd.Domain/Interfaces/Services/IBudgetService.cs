using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.Budget;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IBudgetService
{
    Task<Budget> CreateBudgetAsync(Budget budget);
    Task<PaginatedResult<Budget>> GetBudgetsAsync(ListBudgets listBudgets);
    Task<Budget> FindBudgetByIdAsync(decimal budgetId);
    Task<Budget> UpdateBudgetAsync(Budget updateBudgetRequest, decimal budgetId);
    Task DeleteBudgetAsync(decimal budgetId);
}