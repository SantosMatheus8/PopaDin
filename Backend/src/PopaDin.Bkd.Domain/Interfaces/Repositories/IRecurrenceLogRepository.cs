namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface IRecurrenceLogRepository
{
    /// <summary>
    /// Retorna o conjunto de (SourceRecordId, OccurrenceDate) já materializados pelo worker
    /// dentro do período informado.
    /// </summary>
    Task<HashSet<(string SourceRecordId, DateTime OccurrenceDate)>> GetMaterializedOccurrencesAsync(
        DateTime startDate, DateTime endDate);

    /// <summary>
    /// Retorna o conjunto de (SourceRecordId, OccurrenceDate) já materializados pelo worker
    /// desde o início até a data informada. Usado para cálculo de saldo cumulativo.
    /// </summary>
    Task<HashSet<(string SourceRecordId, DateTime OccurrenceDate)>> GetMaterializedOccurrencesUpToAsync(
        DateTime endDate);
}
