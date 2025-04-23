using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Service;

public class BudgetService(IBudgetRepository repository, ILogger<BudgetService> logger) : IBudgetService
{
    public async Task<Budget> CreateBudgetAsync(Budget budget)
    {
        logger.LogInformation("Criando Budget");

        if (budget.CurrentAmount < 0)
        {
            throw new PopaBaseException("O valor atual deve ser maior que zero.", 422);
        }
        if (budget.Goal < 1)
        {
            throw new PopaBaseException("A meta deve ser maior que um.", 422);
        }

        return await repository.CreateBudgetAsync(budget);
    }

    public async Task<PaginatedResult<Budget>> GetBudgetsAsync(ListBudgets listBudgets)
    {
        logger.LogInformation("Listando Budget");
        return await repository.GetBudgetsAsync(listBudgets);
    }
}