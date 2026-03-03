using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models.Alert;
using PopaDin.Bkd.Domain.Models.User;

namespace PopaDin.Bkd.Service;

public class AlertService(IAlertRepository repository, ILogger<AlertService> logger) : IAlertService
{
    private static readonly HashSet<string> ValidTypes = new(
        Enum.GetNames<AlertType>()
    );

    public async Task<Alert> CreateAlertAsync(Alert alert, decimal userId)
    {
        logger.LogInformation("Criando Alert");

        if (!ValidTypes.Contains(alert.Type.ToString()))
        {
            throw new UnprocessableEntityException("Tipo de alerta inválido.");
        }

        if (alert.Threshold <= 0)
        {
            throw new UnprocessableEntityException("O threshold deve ser maior que zero.");
        }

        alert.User = new User { Id = (int)userId };
        alert.Active = true;

        return await repository.CreateAlertAsync(alert);
    }

    public async Task<List<Alert>> GetAlertsByUserIdAsync(decimal userId)
    {
        logger.LogInformation("Listando Alerts");
        return await repository.GetAlertsByUserIdAsync((int)userId);
    }

    public async Task ToggleAlertAsync(string alertId, bool active, decimal userId)
    {
        logger.LogInformation("Alterando status do Alert");
        await FindAlertOrThrowExceptionAsync(alertId, userId);
        await repository.ToggleAlertAsync(alertId, active);
    }

    public async Task DeleteAlertAsync(string alertId, decimal userId)
    {
        logger.LogInformation("Deletando Alert");
        await FindAlertOrThrowExceptionAsync(alertId, userId);
        await repository.DeleteAlertAsync(alertId);
    }

    private async Task<Alert> FindAlertOrThrowExceptionAsync(string alertId, decimal userId)
    {
        var alert = await repository.FindAlertByIdAsync(alertId, (int)userId);

        if (alert == null)
        {
            logger.LogInformation("Alert nao encontrado");
            throw new NotFoundException("Alert não encontrado");
        }

        return alert;
    }
}
