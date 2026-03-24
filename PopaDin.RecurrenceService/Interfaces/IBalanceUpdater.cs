namespace PopaDin.RecurrenceService.Interfaces;

public interface IBalanceUpdater
{
    Task UpdateBalanceAsync(int userId, decimal amount);
}
