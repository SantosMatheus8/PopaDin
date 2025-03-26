using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IBudgetRepository
{
    Task<Budget> CriarBudgetAsync(Budget budget);
}