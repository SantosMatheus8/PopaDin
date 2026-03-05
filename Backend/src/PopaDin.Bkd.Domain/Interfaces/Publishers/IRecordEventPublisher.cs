using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Domain.Interfaces.Publishers;

public interface IRecordEventPublisher
{
    Task PublishRecordCreatedAsync(int userId, double value, OperationEnum operation, double newBalance);
}
