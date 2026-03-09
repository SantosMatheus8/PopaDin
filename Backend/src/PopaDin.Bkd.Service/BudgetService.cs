using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Service;

public class BudgetService(
    IBudgetRepository repository,
    IDashboardCacheRepository dashboardCacheRepository,
    ILogger<BudgetService> logger) : IBudgetService
{
    public async Task<Budget> CreateBudgetAsync(Budget budget, int userId)
    {
        logger.LogInformation("Criando Budget");

        budget.ValidateGoal();

        budget.User = new User { Id = userId };
        var budgetCreated = await repository.CreateBudgetAsync(budget);

        await dashboardCacheRepository.InvalidateAsync(userId);

        return await FindBudgetOrThrowAsync(budgetCreated.Id!.Value, userId);
    }

    public async Task<PaginatedResult<Budget>> GetBudgetsAsync(ListBudgets listBudgets, int userId)
    {
        logger.LogInformation("Listando Budget");
        listBudgets.UserId = userId;
        return await repository.GetBudgetsAsync(listBudgets);
    }

    public async Task<Budget> FindBudgetByIdAsync(int budgetId, int userId)
    {
        logger.LogInformation("Buscando um Budget");
        return await FindBudgetOrThrowAsync(budgetId, userId);
    }

    public async Task<Budget> UpdateBudgetAsync(Budget updateBudgetRequest, int budgetId, int userId)
    {
        logger.LogInformation("Editando um Budget");

        updateBudgetRequest.ValidateGoal();

        Budget budget = await FindBudgetOrThrowAsync(budgetId, userId);

        budget.Name = updateBudgetRequest.Name;
        budget.Goal = updateBudgetRequest.Goal;
        await repository.UpdateBudgetAsync(budget);

        await dashboardCacheRepository.InvalidateAsync(userId);

        return await FindBudgetOrThrowAsync(budgetId, userId);
    }

    public async Task DeleteBudgetAsync(int budgetId, int userId)
    {
        await FindBudgetOrThrowAsync(budgetId, userId);
        await repository.DeleteBudgetAsync(budgetId);

        await dashboardCacheRepository.InvalidateAsync(userId);
    }

    public async Task FinishBudgetAsync(int budgetId, int userId)
    {
        await FindBudgetOrThrowAsync(budgetId, userId);
        await repository.FinishBudgetAsync(budgetId);

        await dashboardCacheRepository.InvalidateAsync(userId);
    }

    private async Task<Budget> FindBudgetOrThrowAsync(int budgetId, int userId)
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
