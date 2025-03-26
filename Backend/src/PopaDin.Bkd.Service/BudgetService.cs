

using PopaDin.Bkd.Api.Dtos.Budget;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Service;

public class BudgetService(IBudgetRepository repository) : IBudgetService
{
    public async Task<Budget> CreateBudgetAsync(CreateBudgetRequest createBudgetRequest)
    {
        var budget = new Budget();
        budget.Name = createBudgetRequest.Name;
        budget.Goal = createBudgetRequest.Goal;
        budget.CurrentAmount = createBudgetRequest.CurrentAmount;
        return await repository.CriarBudgetAsync(budget);

    }

}