using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using PopaDin.AlertService.Interfaces;
using PopaDin.AlertService.Models;

namespace PopaDin.AlertService.Services;

public partial class NotificationService(
    IConfiguration configuration,
    IEmailSender emailSender,
    INotificationPublisher notificationPublisher,
    ILogger<NotificationService> logger) : INotificationService
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();

    public async Task SendAlertNotificationAsync(AlertRule rule, RecordCreatedEvent recordEvent)
    {
        if (!EmailRegex().IsMatch(rule.Channel))
        {
            logger.LogWarning("Canal '{Channel}' do alerta {AlertId} não é um email válido, ignorando envio",
                rule.Channel, rule.Id);
            return;
        }

        logger.LogInformation("Enviando notificação de alerta para o canal: {Channel}", rule.Channel);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            configuration["SmtpSettings:SenderName"],
            configuration["SmtpSettings:SenderEmail"]
        ));
        message.To.Add(new MailboxAddress("", rule.Channel));
        message.Subject = "PopaDin - Alerta Disparado";

        var body = BuildEmailBody(rule, recordEvent);
        message.Body = new TextPart("html") { Text = body };

        await emailSender.SendAsync(message);

        logger.LogInformation("Notificação enviada com sucesso para: {Channel}", rule.Channel);

        var (notifTitle, notifMessage, metadata) = BuildNotificationPayload(rule, recordEvent);
        await notificationPublisher.PublishAsync(rule.UserId, rule.Type, notifTitle, notifMessage, metadata);
    }

    private static string BuildEmailBody(AlertRule rule, RecordCreatedEvent recordEvent)
    {
        return rule.Type switch
        {
            nameof(AlertRuleType.BALANCE_BELOW) =>
                $"""
                <h2>Alerta: Saldo Abaixo do Limite</h2>
                <p>Uma movimentação foi registrada e seu saldo ficou abaixo do limite configurado.</p>
                <ul>
                    <li><strong>Operação:</strong> {recordEvent.Operation}</li>
                    <li><strong>Valor:</strong> R$ {recordEvent.Value:F2}</li>
                    <li><strong>Saldo Atual:</strong> R$ {recordEvent.NewBalance:F2}</li>
                    <li><strong>Limite Configurado:</strong> R$ {rule.Threshold:F2}</li>
                </ul>
                """,
            nameof(AlertRuleType.GOAL_ABOVE) =>
                $"""
                <h2>Alerta: Gastos Acima da Meta</h2>
                <p>Seus gastos mensais ultrapassaram o limite configurado.</p>
                <ul>
                    <li><strong>Operação:</strong> {recordEvent.Operation}</li>
                    <li><strong>Valor:</strong> R$ {recordEvent.Value:F2}</li>
                    <li><strong>Gastos do Mês:</strong> R$ {recordEvent.MonthlyExpenses:F2}</li>
                    <li><strong>Limite da Meta:</strong> R$ {rule.Threshold:F2}</li>
                </ul>
                """,
            _ =>
                $"""
                <h2>Alerta Disparado</h2>
                <p>Um alerta do tipo <strong>{rule.Type}</strong> foi disparado.</p>
                <ul>
                    <li><strong>Saldo Atual:</strong> R$ {recordEvent.NewBalance:F2}</li>
                </ul>
                """
        };
    }

    private static (string Title, string Message, object Metadata) BuildNotificationPayload(
        AlertRule rule, RecordCreatedEvent recordEvent)
    {
        return rule.Type switch
        {
            nameof(AlertRuleType.BALANCE_BELOW) => (
                "Alerta de Saldo Baixo",
                $"Seu saldo atual é R$ {recordEvent.NewBalance:F2}, abaixo do limite de R$ {rule.Threshold:F2}",
                new { currentBalance = recordEvent.NewBalance, threshold = rule.Threshold }
            ),
            nameof(AlertRuleType.GOAL_ABOVE) => (
                "Alerta de Meta Excedida",
                $"Seus gastos mensais de R$ {recordEvent.MonthlyExpenses:F2} ultrapassaram o limite de R$ {rule.Threshold:F2}",
                new { monthlyExpenses = recordEvent.MonthlyExpenses, threshold = rule.Threshold }
            ),
            _ => (
                "Alerta Disparado",
                $"Um alerta do tipo {rule.Type} foi disparado. Saldo atual: R$ {recordEvent.NewBalance:F2}",
                new { newBalance = recordEvent.NewBalance }
            )
        };
    }
}
