using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Domain.Interfaces.Publishers;

public interface IRecordEventPublisher
{
    Task PublishRecordCreatedAsync(int userId, decimal value, OperationEnum operation, decimal newBalance);
}
