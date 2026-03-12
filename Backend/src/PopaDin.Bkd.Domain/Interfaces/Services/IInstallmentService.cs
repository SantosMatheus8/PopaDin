using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface IInstallmentService
{
    Task<List<Record>> CreateInstallmentRecordsAsync(Record baseRecord, int installmentCount);
}
