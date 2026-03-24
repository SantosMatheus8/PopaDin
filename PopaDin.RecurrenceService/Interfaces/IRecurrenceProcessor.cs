namespace PopaDin.RecurrenceService.Interfaces;

public interface IRecurrenceProcessor
{
    Task ProcessPendingRecurrencesAsync(CancellationToken cancellationToken);
}
