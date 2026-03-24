namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IRecurrenceLogRepository
{
    /// <summary>
    /// Retorna o conjunto de (SourceRecordId, OccurrenceDate) já materializados pelo worker
    /// dentro do período informado.
    /// </summary>
    Task<HashSet<(string SourceRecordId, DateTime OccurrenceDate)>> GetMaterializedOccurrencesAsync(
        DateTime startDate, DateTime endDate);
}
