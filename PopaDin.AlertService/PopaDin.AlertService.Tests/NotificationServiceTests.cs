using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using NSubstitute;
using PopaDin.AlertService.Interfaces;
using PopaDin.AlertService.Models;
using PopaDin.AlertService.Services;

namespace PopaDin.AlertService.Tests;

public class NotificationServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly INotificationPublisher _notificationPublisher = Substitute.For<INotificationPublisher>();
    private readonly ILogger<NotificationService> _logger = Substitute.For<ILogger<NotificationService>>();

    public NotificationServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "SmtpSettings:SenderName", "PopaDin" },
            { "SmtpSettings:SenderEmail", "noreply@popadin.com" },
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    private NotificationService CreateService()
    {
        return new NotificationService(_configuration, _emailSender, _notificationPublisher, _logger);
    }

    [Fact]
    public async Task SendAlertNotificationAsync_WithValidEmail_ShouldSendEmailAndPublishNotification()
    {
        var rule = new AlertRule
        {
            Id = "abc123",
            UserId = 1,
            Type = nameof(AlertRuleType.BALANCE_BELOW),
            Channel = "user@example.com",
            Threshold = 500
        };
        var recordEvent = new RecordCreatedEvent
        {
            UserId = 1,
            Value = 100,
            Operation = "Outflow",
            NewBalance = 300,
            MonthlyExpenses = 800
        };

        await CreateService().SendAlertNotificationAsync(rule, recordEvent);

        await _emailSender.Received(1).SendAsync(Arg.Any<MimeMessage>());
        await _notificationPublisher.Received(1).PublishAsync(
            1, nameof(AlertRuleType.BALANCE_BELOW),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<object>());
    }

    [Fact]
    public async Task SendAlertNotificationAsync_WithInvalidEmail_ShouldNotSend()
    {
        var rule = new AlertRule
        {
            Id = "abc123",
            UserId = 1,
            Type = nameof(AlertRuleType.BALANCE_BELOW),
            Channel = "not-an-email",
            Threshold = 500
        };
        var recordEvent = new RecordCreatedEvent { UserId = 1, NewBalance = 300 };

        await CreateService().SendAlertNotificationAsync(rule, recordEvent);

        await _emailSender.DidNotReceive().SendAsync(Arg.Any<MimeMessage>());
        await _notificationPublisher.DidNotReceive().PublishAsync(
            Arg.Any<int>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<object?>());
    }

    [Fact]
    public async Task SendAlertNotificationAsync_WithEmptyChannel_ShouldNotSend()
    {
        var rule = new AlertRule
        {
            Id = "abc123",
            UserId = 1,
            Type = nameof(AlertRuleType.BALANCE_BELOW),
            Channel = "",
            Threshold = 500
        };
        var recordEvent = new RecordCreatedEvent { UserId = 1, NewBalance = 300 };

        await CreateService().SendAlertNotificationAsync(rule, recordEvent);

        await _emailSender.DidNotReceive().SendAsync(Arg.Any<MimeMessage>());
    }

    [Fact]
    public async Task SendAlertNotificationAsync_BalanceAboveType_ShouldSendCorrectEmail()
    {
        var rule = new AlertRule
        {
            Id = "def456",
            UserId = 2,
            Type = nameof(AlertRuleType.BALANCE_ABOVE),
            Channel = "user@example.com",
            Threshold = 2000
        };
        var recordEvent = new RecordCreatedEvent
        {
            UserId = 2,
            Value = 500,
            Operation = "Deposit",
            NewBalance = 2500,
            MonthlyExpenses = 0
        };

        await CreateService().SendAlertNotificationAsync(rule, recordEvent);

        await _emailSender.Received(1).SendAsync(Arg.Is<MimeMessage>(m =>
            m.To.Mailboxes.Any(mb => mb.Address == "user@example.com")));
    }

    [Fact]
    public async Task SendAlertNotificationAsync_BalanceBelowType_ShouldPublishWithCorrectTitle()
    {
        var rule = new AlertRule
        {
            Id = "abc",
            UserId = 1,
            Type = nameof(AlertRuleType.BALANCE_BELOW),
            Channel = "user@test.com",
            Threshold = 500
        };
        var recordEvent = new RecordCreatedEvent { UserId = 1, NewBalance = 300 };

        await CreateService().SendAlertNotificationAsync(rule, recordEvent);

        await _notificationPublisher.Received(1).PublishAsync(
            1, nameof(AlertRuleType.BALANCE_BELOW),
            "Alerta de Saldo Baixo",
            Arg.Any<string>(), Arg.Any<object>());
    }

    [Fact]
    public async Task SendAlertNotificationAsync_BalanceAboveType_ShouldPublishWithCorrectTitle()
    {
        var rule = new AlertRule
        {
            Id = "abc",
            UserId = 1,
            Type = nameof(AlertRuleType.BALANCE_ABOVE),
            Channel = "user@test.com",
            Threshold = 1000
        };
        var recordEvent = new RecordCreatedEvent { UserId = 1, NewBalance = 1500 };

        await CreateService().SendAlertNotificationAsync(rule, recordEvent);

        await _notificationPublisher.Received(1).PublishAsync(
            1, nameof(AlertRuleType.BALANCE_ABOVE),
            "Alerta de Saldo Alto",
            Arg.Any<string>(), Arg.Any<object>());
    }

    [Fact]
    public async Task SendAlertNotificationAsync_UnknownType_ShouldPublishGenericTitle()
    {
        var rule = new AlertRule
        {
            Id = "abc",
            UserId = 1,
            Type = "CUSTOM_TYPE",
            Channel = "user@test.com",
            Threshold = 100
        };
        var recordEvent = new RecordCreatedEvent { UserId = 1, NewBalance = 50 };

        await CreateService().SendAlertNotificationAsync(rule, recordEvent);

        await _notificationPublisher.Received(1).PublishAsync(
            1, "CUSTOM_TYPE",
            "Alerta Disparado",
            Arg.Any<string>(), Arg.Any<object>());
    }
}
