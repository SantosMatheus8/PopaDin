using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PopaDin.Bkd.Api.Controllers;
using PopaDin.Bkd.Api.Dtos.Alert;
using PopaDin.Bkd.Api.Tests.Helpers;
using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Api.Tests.Controllers;

public class AlertControllerTests
{
    private readonly IAlertService _alertService = Substitute.For<IAlertService>();
    private readonly AlertController _sut;

    private const int AuthUserId = 1;

    public AlertControllerTests()
    {
        _sut = new AlertController(_alertService);
        ControllerTestHelper.SetupAuthenticatedUser(_sut, AuthUserId);
    }

    #region CreateAlert

    [Fact]
    public async Task CreateAlert_WithValidRequest_ShouldReturn201Created()
    {
        var request = new CreateAlertRequest { Type = AlertType.BALANCE_BELOW, Threshold = 100 };
        _alertService.CreateAlertAsync(Arg.Any<Alert>(), AuthUserId)
            .Returns(new Alert { Id = "abc", Type = AlertType.BALANCE_BELOW, Threshold = 100, Active = true });

        var result = await _sut.CreateAlert(request);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task CreateAlert_WithDuplicate_ShouldThrowUnprocessableEntityException()
    {
        var request = new CreateAlertRequest { Type = AlertType.BALANCE_BELOW, Threshold = 100 };
        _alertService.CreateAlertAsync(Arg.Any<Alert>(), AuthUserId)
            .Throws(new UnprocessableEntityException("Já existe um alerta com este tipo e limite."));

        var act = () => _sut.CreateAlert(request);

        await act.Should().ThrowAsync<UnprocessableEntityException>();
    }

    #endregion

    #region GetAlerts

    [Fact]
    public async Task GetAlerts_ShouldReturnOkWithAlertsList()
    {
        _alertService.GetAlertsByUserIdAsync(AuthUserId)
            .Returns(new List<Alert> { new() { Id = "1" }, new() { Id = "2" } });

        var result = await _sut.GetAlerts();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    #endregion

    #region ToggleAlert

    [Fact]
    public async Task ToggleAlert_WhenExists_ShouldReturn204NoContent()
    {
        var request = new ToggleAlertRequest { Active = false };

        var result = await _sut.ToggleAlert("abc", request);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ToggleAlert_WhenNotFound_ShouldThrowNotFoundException()
    {
        var request = new ToggleAlertRequest { Active = true };
        _alertService.ToggleAlertAsync("invalid", true, AuthUserId)
            .Throws(new NotFoundException("Alert não encontrado"));

        var act = () => _sut.ToggleAlert("invalid", request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeleteAlert

    [Fact]
    public async Task DeleteAlert_WhenExists_ShouldReturn204NoContent()
    {
        var result = await _sut.DeleteAlert("abc");

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteAlert_WhenNotFound_ShouldThrowNotFoundException()
    {
        _alertService.DeleteAlertAsync("invalid", AuthUserId)
            .Throws(new NotFoundException("Alert não encontrado"));

        var act = () => _sut.DeleteAlert("invalid");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
