using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.Budget;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IBudgetRepository
{
    Task<Budget> CreateBudgetAsync(Budget budget);
    Task<PaginatedResult<Budget>> GetBudgetsAsync(ListBudgets listBudgets);
    Task<Budget> FindBudgetByIdAsync(decimal budgetId, decimal userId);
    Task UpdateBudgetAsync(Budget budget);
    Task DeleteBudgetAsync(decimal budgetId);
}