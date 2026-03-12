using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using PopaDin.AlertService.Interfaces;

namespace PopaDin.AlertService.Services;

public class SmtpEmailSender : IEmailSender, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailSender> _logger;
    private readonly int _port;
    private SmtpClient? _client;
    private bool _disposed;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;

        if (!int.TryParse(configuration["SmtpSettings:Port"], out _port))
            throw new InvalidOperationException("SmtpSettings:Port não está configurado ou é inválido.");
    }

    public async Task SendAsync(MimeMessage message)
    {
        var client = await GetConnectedClientAsync();
        await client.SendAsync(message);
        _logger.LogInformation("Email enviado com sucesso");
    }

    private async Task<SmtpClient> GetConnectedClientAsync()
    {
        if (_client is { IsConnected: true })
            return _client;

        _client?.Dispose();
        _client = new SmtpClient();

        await _client.ConnectAsync(
            _configuration["SmtpSettings:Host"],
            _port,
            SecureSocketOptions.StartTls
        );

        await _client.AuthenticateAsync(
            _configuration["SmtpSettings:Username"],
            _configuration["SmtpSettings:Password"]
        );

        return _client;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_client is { IsConnected: true })
        {
            try { _client.Disconnect(true); } catch { /* best effort */ }
        }
        _client?.Dispose();
    }
}
