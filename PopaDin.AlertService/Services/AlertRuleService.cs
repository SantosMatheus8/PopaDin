using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PopaDin.AlertService.Interfaces;
using PopaDin.AlertService.Models;

namespace PopaDin.AlertService.Services;

public class AlertRuleService(IMongoDatabase database, ILogger<AlertRuleService> logger) : IAlertRuleService
{
    private IMongoCollection<AlertRule> Collection =>
        database.GetCollection<AlertRule>("alerts");

    public async Task<List<AlertRule>> GetActiveAlertsByUserIdAsync(int userId)
    {
        logger.LogInformation("Buscando alertas ativos do usuário: {UserId}", userId);

        var filter = Builders<AlertRule>.Filter.Eq(a => a.UserId, userId)
                     & Builders<AlertRule>.Filter.Eq(a => a.Active, true);

        var rules = await Collection.Find(filter).ToListAsync();

        logger.LogInformation("Encontrados {Count} alertas ativos para o usuário {UserId}", rules.Count, userId);

        return rules;
    }

    public bool IsRuleTriggered(AlertRule rule, RecordCreatedEvent recordEvent)
    {
        return rule.Type switch
        {
            nameof(AlertRuleType.BALANCE_BELOW) => recordEvent.NewBalance < rule.Threshold,
            nameof(AlertRuleType.BUDGET_ABOVE) => recordEvent.MonthlyExpenses > rule.Threshold,
            _ => false
        };
    }
}
