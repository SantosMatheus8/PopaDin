using PopaDin.AlertService.Models;

namespace PopaDin.AlertService.Interfaces;

public interface IAlertRuleService
{
    Task<List<AlertRule>> GetActiveAlertsByUserIdAsync(int userId);
    bool IsRuleTriggered(AlertRule rule, RecordCreatedEvent recordEvent);
}
