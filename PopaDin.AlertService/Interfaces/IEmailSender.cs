using MimeKit;

namespace PopaDin.AlertService.Interfaces;

public interface IEmailSender
{
    Task SendAsync(MimeMessage message);
}
