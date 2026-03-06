using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Infra.Documents;

namespace PopaDin.Bkd.Infra.Repositories;

public class AlertRepository(IMongoDatabase database, ILogger<AlertRepository> logger) : IAlertRepository
{
    private IMongoCollection<AlertDocument> Collection =>
        database.GetCollection<AlertDocument>("alerts");

    public async Task<Alert> CreateAlertAsync(Alert alert)
    {
        logger.LogInformation("Criando Alert no MongoDB");

        var document = new AlertDocument
        {
            UserId = alert.User.Id,
            Type = alert.Type.ToString(),
            Threshold = alert.Threshold,
            Channel = alert.Channel,
            Active = alert.Active,
            CreatedAt = DateTime.UtcNow
        };

        await Collection.InsertOneAsync(document);

        logger.LogInformation("Alert criado com Id: {Id}", document.Id);

        return MapToAlert(document);
    }

    public async Task<List<Alert>> GetAlertsByUserIdAsync(int userId)
    {
        logger.LogInformation("Listando Alerts do usuario: {UserId}", userId);

        var filter = Builders<AlertDocument>.Filter.Eq(a => a.UserId, userId);
        var documents = await Collection.Find(filter).ToListAsync();

        logger.LogInformation("Encontrados {Count} alerts", documents.Count);

        return documents.Select(MapToAlert).ToList();
    }

    public async Task<Alert?> FindAlertByIdAsync(string alertId, int userId)
    {
        logger.LogInformation("Buscando Alert: {AlertId}", alertId);

        if (!ObjectId.TryParse(alertId, out _))
            return null;

        var filter = Builders<AlertDocument>.Filter.Eq(a => a.Id, alertId)
                     & Builders<AlertDocument>.Filter.Eq(a => a.UserId, userId);

        var document = await Collection.Find(filter).FirstOrDefaultAsync();

        return document == null ? null : MapToAlert(document);
    }

    public async Task ToggleAlertAsync(string alertId, bool active)
    {
        logger.LogInformation("Alterando status do Alert: {AlertId} para Active: {Active}", alertId, active);

        var filter = Builders<AlertDocument>.Filter.Eq(a => a.Id, alertId);
        var update = Builders<AlertDocument>.Update.Set(a => a.Active, active);

        await Collection.UpdateOneAsync(filter, update);
    }

    public async Task DeleteAlertAsync(string alertId)
    {
        logger.LogInformation("Deletando Alert: {AlertId}", alertId);

        var filter = Builders<AlertDocument>.Filter.Eq(a => a.Id, alertId);
        await Collection.DeleteOneAsync(filter);
    }

    private static Alert MapToAlert(AlertDocument document)
    {
        return new Alert
        {
            Id = document.Id,
            User = new User { Id = document.UserId },
            Type = Enum.Parse<AlertType>(document.Type),
            Threshold = (decimal)document.Threshold,
            Channel = document.Channel,
            Active = document.Active,
            CreatedAt = document.CreatedAt
        };
    }
}
