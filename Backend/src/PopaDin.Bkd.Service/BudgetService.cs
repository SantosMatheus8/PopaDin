using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.Budget;
using PopaDin.Bkd.Domain.Models.User;

namespace PopaDin.Bkd.Service;

public class BudgetService(IBudgetRepository repository, ILogger<BudgetService> logger) : IBudgetService
{
    public async Task<Budget> CreateBudgetAsync(Budget budget, decimal userId)
    {
        logger.LogInformation("Criando Budget");

        if (budget.Goal < 1)
        {
            throw new UnprocessableEntityException("A meta deve ser maior que um.");
        }

        budget.User = new User { Id = (int)userId };
        var budgetCreated = await repository.CreateBudgetAsync(budget);

        return await FindBudgetOrThrowExceptionAsync(budgetCreated.Id!.Value, userId);
    }

    public async Task<PaginatedResult<Budget>> GetBudgetsAsync(ListBudgets listBudgets, decimal userId)
    {
        logger.LogInformation("Listando Budget");
        listBudgets.UserId = (int)userId;
        return await repository.GetBudgetsAsync(listBudgets);
    }

    public async Task<Budget> FindBudgetByIdAsync(decimal budgetId, decimal userId)
    {
        logger.LogInformation("Buscando um Budget");
        return await FindBudgetOrThrowExceptionAsync(budgetId, userId);
    }

    public async Task<Budget> UpdateBudgetAsync(Budget updateBudgetRequest, decimal budgetId, decimal userId)
    {
        logger.LogInformation("Editando um Budget");

        if (updateBudgetRequest.Goal < 1)
        {
            throw new UnprocessableEntityException("A meta deve ser maior que um.");
        }

        Budget budget = await FindBudgetOrThrowExceptionAsync(budgetId, userId);

        budget.Name = updateBudgetRequest.Name;
        budget.Goal = updateBudgetRequest.Goal;
        await repository.UpdateBudgetAsync(budget);

        return await FindBudgetOrThrowExceptionAsync(budgetId, userId);
    }

    public async Task DeleteBudgetAsync(decimal budgetId, decimal userId)
    {
        await FindBudgetOrThrowExceptionAsync(budgetId, userId);
        await repository.DeleteBudgetAsync(budgetId);
    }

    private async Task<Budget> FindBudgetOrThrowExceptionAsync(decimal budgetId, decimal userId)
    {
        Budget budget = await repository.FindBudgetByIdAsync(budgetId, userId);

        if (budget == null)
        {
            logger.LogInformation("Budget nao encontrado");
            throw new NotFoundException("Budget não encontrado");
        }

        return budget;
    }
}
