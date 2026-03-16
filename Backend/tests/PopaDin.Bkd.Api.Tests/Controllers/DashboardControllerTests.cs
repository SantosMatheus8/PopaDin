using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PopaDin.Bkd.Api.Controllers;
using PopaDin.Bkd.Api.Dtos.Dashboard;
using PopaDin.Bkd.Api.Tests.Helpers;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Api.Tests.Controllers;

public class DashboardControllerTests
{
    private readonly IDashboardService _dashboardService = Substitute.For<IDashboardService>();
    private readonly DashboardController _sut;

    private const int AuthUserId = 1;

    public DashboardControllerTests()
    {
        _sut = new DashboardController(_dashboardService);
        ControllerTestHelper.SetupAuthenticatedUser(_sut, AuthUserId);
    }

    [Fact]
    public async Task GetDashboard_WithNoDates_ShouldReturnOk()
    {
        var request = new DashboardRequest();
        _dashboardService.GetDashboardAsync(AuthUserId, null, null)
            .Returns(new DashboardResult
            {
                Summary = new DashboardSummary { TotalDeposits = 5000, TotalOutflows = 2000, Balance = 3000 },
                Budgets = new List<DashboardBudget>(),
                SpendingByTag = new List<DashboardSpendingByTag>(),
                LatestRecords = new List<Record>(),
                TopDeposits = new List<Record>(),
                TopOutflows = new List<Record>()
            });

        var result = await _sut.GetDashboard(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetDashboard_WithCustomDates_ShouldPassDatesToService()
    {
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 31);
        var request = new DashboardRequest { StartDate = start, EndDate = end };

        _dashboardService.GetDashboardAsync(AuthUserId, start, end)
            .Returns(new DashboardResult
            {
                Summary = new DashboardSummary(),
                Budgets = new List<DashboardBudget>(),
                SpendingByTag = new List<DashboardSpendingByTag>(),
                LatestRecords = new List<Record>(),
                TopDeposits = new List<Record>(),
                TopOutflows = new List<Record>()
            });

        var result = await _sut.GetDashboard(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        await _dashboardService.Received(1).GetDashboardAsync(AuthUserId, start, end);
    }
}
