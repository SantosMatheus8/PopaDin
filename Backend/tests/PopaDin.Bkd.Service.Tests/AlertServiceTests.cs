using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Service;

namespace PopaDin.Bkd.Service.Tests;

public class AlertServiceTests
{
    private readonly IAlertRepository _alertRepository = Substitute.For<IAlertRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ILogger<AlertService> _logger = Substitute.For<ILogger<AlertService>>();
    private readonly AlertService _sut;

    public AlertServiceTests()
    {
        _sut = new AlertService(_alertRepository, _userRepository, _logger);
    }

    #region CreateAlertAsync

    [Fact]
    public async Task CreateAlertAsync_WithValidAlert_ShouldCreate()
    {
        var alert = new Alert { Type = AlertType.BALANCE_BELOW, Threshold = 100 };
        var user = new User { Id = 1, Email = "test@test.com" };
        var createdAlert = new Alert { Id = "abc", Type = AlertType.BALANCE_BELOW, Threshold = 100, Active = true, Channel = "test@test.com" };

        _alertRepository.GetAlertsByUserIdAsync(1).Returns(new List<Alert>());
        _userRepository.FindUserByIdAsync(1).Returns(user);
        _alertRepository.CreateAlertAsync(Arg.Any<Alert>()).Returns(createdAlert);

        var result = await _sut.CreateAlertAsync(alert, 1);

        result.Should().Be(createdAlert);
    }

    [Fact]
    public async Task CreateAlertAsync_WithDuplicateTypeAndThreshold_ShouldThrowUnprocessableEntityException()
    {
        var existingAlert = new Alert { Type = AlertType.BALANCE_BELOW, Threshold = 100 };
        var newAlert = new Alert { Type = AlertType.BALANCE_BELOW, Threshold = 100 };

        _alertRepository.GetAlertsByUserIdAsync(1).Returns(new List<Alert> { existingAlert });

        var act = () => _sut.CreateAlertAsync(newAlert, 1);

        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .WithMessage("Já existe um alerta com este tipo e limite.");
    }

    [Fact]
    public async Task CreateAlertAsync_WithInvalidThreshold_ShouldThrowException()
    {
        var alert = new Alert { Type = AlertType.BALANCE_BELOW, Threshold = 0 };

        var act = () => _sut.CreateAlertAsync(alert, 1);

        await act.Should().ThrowAsync<UnprocessableEntityException>();
    }

    [Fact]
    public async Task CreateAlertAsync_ShouldSetChannelFromUserEmail()
    {
        var alert = new Alert { Type = AlertType.BALANCE_ABOVE, Threshold = 500 };
        var user = new User { Id = 1, Email = "user@email.com" };

        _alertRepository.GetAlertsByUserIdAsync(1).Returns(new List<Alert>());
        _userRepository.FindUserByIdAsync(1).Returns(user);
        _alertRepository.CreateAlertAsync(Arg.Any<Alert>()).Returns(callInfo =>
        {
            var a = callInfo.Arg<Alert>();
            a.Id = "123";
            return a;
        });

        var result = await _sut.CreateAlertAsync(alert, 1);

        result.Channel.Should().Be("user@email.com");
        result.Active.Should().BeTrue();
    }

    #endregion

    #region GetAlertsByUserIdAsync

    [Fact]
    public async Task GetAlertsByUserIdAsync_ShouldReturnAlerts()
    {
        var alerts = new List<Alert> { new() { Id = "1" }, new() { Id = "2" } };
        _alertRepository.GetAlertsByUserIdAsync(1).Returns(alerts);

        var result = await _sut.GetAlertsByUserIdAsync(1);

        result.Should().HaveCount(2);
    }

    #endregion

    #region ToggleAlertAsync

    [Fact]
    public async Task ToggleAlertAsync_WhenExists_ShouldToggle()
    {
        var alert = new Alert { Id = "abc", Active = true };
        _alertRepository.FindAlertByIdAsync("abc", 1).Returns(alert);

        await _sut.ToggleAlertAsync("abc", false, 1);

        await _alertRepository.Received(1).ToggleAlertAsync("abc", false);
    }

    [Fact]
    public async Task ToggleAlertAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _alertRepository.FindAlertByIdAsync("invalid", 1).Returns((Alert?)null);

        var act = () => _sut.ToggleAlertAsync("invalid", true, 1);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Alert não encontrado");
    }

    #endregion

    #region DeleteAlertAsync

    [Fact]
    public async Task DeleteAlertAsync_WhenExists_ShouldDelete()
    {
        var alert = new Alert { Id = "abc" };
        _alertRepository.FindAlertByIdAsync("abc", 1).Returns(alert);

        await _sut.DeleteAlertAsync("abc", 1);

        await _alertRepository.Received(1).DeleteAlertAsync("abc");
    }

    [Fact]
    public async Task DeleteAlertAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _alertRepository.FindAlertByIdAsync("invalid", 1).Returns((Alert?)null);

        var act = () => _sut.DeleteAlertAsync("invalid", 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
