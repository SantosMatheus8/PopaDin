using System.Text.RegularExpressions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using PopaDin.AlertService.Interfaces;
using PopaDin.AlertService.Models;

namespace PopaDin.AlertService.Services;

public partial class NotificationService(IConfiguration configuration, ILogger<NotificationService> logger) : INotificationService
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

        using var client = new SmtpClient();

        await client.ConnectAsync(
            configuration["SmtpSettings:Host"],
            int.Parse(configuration["SmtpSettings:Port"]!),
            SecureSocketOptions.StartTls
        );

        await client.AuthenticateAsync(
            configuration["SmtpSettings:Username"],
            configuration["SmtpSettings:Password"]
        );

        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        logger.LogInformation("Notificação enviada com sucesso para: {Channel}", rule.Channel);
    }

    private static string BuildEmailBody(AlertRule rule, RecordCreatedEvent recordEvent)
    {
        return rule.Type switch
        {
            "BALANCE_BELOW" =>
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
            "BUDGET_ABOVE" =>
                $"""
                <h2>Alerta: Saldo Acima do Limite</h2>
                <p>Uma movimentação foi registrada e seu saldo ultrapassou o limite configurado.</p>
                <ul>
                    <li><strong>Operação:</strong> {recordEvent.Operation}</li>
                    <li><strong>Valor:</strong> R$ {recordEvent.Value:F2}</li>
                    <li><strong>Saldo Atual:</strong> R$ {recordEvent.NewBalance:F2}</li>
                    <li><strong>Limite Configurado:</strong> R$ {rule.Threshold:F2}</li>
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
}
