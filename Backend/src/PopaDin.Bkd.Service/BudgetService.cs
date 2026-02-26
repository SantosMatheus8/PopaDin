using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.Budget;

namespace PopaDin.Bkd.Service;

public class BudgetService(IBudgetRepository repository, ILogger<BudgetService> logger) : IBudgetService
{
    public async Task<Budget> CreateBudgetAsync(Budget budget)
    {
        logger.LogInformation("Criando Budget");

        if (budget.CurrentAmount < 0)
        {
            throw new UnprocessableEntityException("O valor atual deve ser maior que zero.");
        }
        if (budget.Goal < 1)
        {
            throw new UnprocessableEntityException("A meta deve ser maior que um.");
        }

        return await repository.CreateBudgetAsync(budget);
    }

    public async Task<PaginatedResult<Budget>> GetBudgetsAsync(ListBudgets listBudgets)
    {
        logger.LogInformation("Listando Budget");
        return await repository.GetBudgetsAsync(listBudgets);
    }

    public async Task<Budget> FindBudgetByIdAsync(decimal budgetId)
    {
        logger.LogInformation("Buscando um Budget");
        return await FindBudgetOrThrowExceptionAsync(budgetId);
    }

    public async Task<Budget> UpdateBudgetAsync(Budget updateBudgetRequest, decimal budgetId)
    {
        logger.LogInformation("Editando um Budget");

        if (updateBudgetRequest.CurrentAmount < 0)
        {
            throw new UnprocessableEntityException("O valor atual deve ser maior que zero.");
        }
        if (updateBudgetRequest.Goal < 1)
        {
            throw new UnprocessableEntityException("A meta deve ser maior que um.");
        }

        Budget budget = await FindBudgetOrThrowExceptionAsync(budgetId);

        budget.Name = updateBudgetRequest.Name;
        budget.Goal = updateBudgetRequest.Goal;
        budget.CurrentAmount = updateBudgetRequest.CurrentAmount;
        await repository.UpdateBudgetAsync(budget);

        return await repository.FindBudgetByIdAsync(budgetId);
    }

    public async Task DeleteBudgetAsync(decimal budgetId)
    {
        await FindBudgetOrThrowExceptionAsync(budgetId);
        await repository.DeleteBudgetAsync(budgetId);
    }

    private async Task<Budget> FindBudgetOrThrowExceptionAsync(decimal budgetId)
    {
        Budget budget = await repository.FindBudgetByIdAsync(budgetId);

        if (budget == null)
        {
            logger.LogInformation("Budget nao encontrado");
            throw new NotFoundException("Budget não encontrado");
        }

        return budget;
    }
}