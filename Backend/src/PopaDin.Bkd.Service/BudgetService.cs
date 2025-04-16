using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Service;

public class BudgetService(IBudgetRepository repository) : IBudgetService
{
    public async Task<Budget> CreateBudgetAsync(Budget budget)
    {
        return await repository.CriarBudgetAsync(budget);
    }
}