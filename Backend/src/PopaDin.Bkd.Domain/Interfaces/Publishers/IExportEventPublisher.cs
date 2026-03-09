namespace PopaDin.Bkd.Domain.Interfaces.Publishers;

public interface IExportEventPublisher
{
    Task PublishExportRequestAsync(int userId, DateTime startDate, DateTime endDate);
}
