using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Service;

public class BudgetService(IBudgetRepository repository, ILogger<BudgetService> logger) : IBudgetService
{
    public async Task<Budget> CreateBudgetAsync(Budget budget)
    {
        logger.LogInformation("Criando Budget");
        return await repository.CreateBudgetAsync(budget);
    }
}