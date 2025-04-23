using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IBudgetRepository
{
    Task<Budget> CreateBudgetAsync(Budget budget);
    Task<PaginatedResult<Budget>> GetBudgetsAsync(ListBudgets listBudgets);
}