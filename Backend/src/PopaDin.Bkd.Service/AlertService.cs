using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Service;

public class AlertService(IAlertRepository repository, IUserRepository userRepository, ILogger<AlertService> logger) : IAlertService
{
    public async Task<Alert> CreateAlertAsync(Alert alert, int userId)
    {
        logger.LogInformation("Criando Alert");

        alert.ValidateThreshold();

        var existingAlerts = await repository.GetAlertsByUserIdAsync(userId);
        if (existingAlerts.Any(a => a.Type == alert.Type && a.Threshold == alert.Threshold))
            throw new UnprocessableEntityException("Já existe um alerta com este tipo e limite.");

        var user = await userRepository.FindUserByIdAsync(userId);
        alert.User = new User { Id = userId };
        alert.Channel = user.Email;
        alert.Active = true;

        return await repository.CreateAlertAsync(alert);
    }

    public async Task<List<Alert>> GetAlertsByUserIdAsync(int userId)
    {
        logger.LogInformation("Listando Alerts");
        return await repository.GetAlertsByUserIdAsync(userId);
    }

    public async Task ToggleAlertAsync(string alertId, bool active, int userId)
    {
        logger.LogInformation("Alterando status do Alert");
        await FindAlertOrThrowAsync(alertId, userId);
        await repository.ToggleAlertAsync(alertId, active);
    }

    public async Task DeleteAlertAsync(string alertId, int userId)
    {
        logger.LogInformation("Deletando Alert");
        await FindAlertOrThrowAsync(alertId, userId);
        await repository.DeleteAlertAsync(alertId);
    }

    private async Task<Alert> FindAlertOrThrowAsync(string alertId, int userId)
    {
        var alert = await repository.FindAlertByIdAsync(alertId, userId);

        if (alert == null)
        {
            logger.LogInformation("Alert nao encontrado");
            throw new NotFoundException("Alert não encontrado");
        }

        return alert;
    }
}
