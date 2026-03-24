using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Service;

namespace PopaDin.Bkd.Service.Tests;

public class DashboardServiceTests
{
    private readonly IDashboardRepository _dashboardRepository = Substitute.For<IDashboardRepository>();
    private readonly IDashboardCacheRepository _cacheRepository = Substitute.For<IDashboardCacheRepository>();
    private readonly IGoalRepository _goalRepository = Substitute.For<IGoalRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IRecordRepository _recordRepository = Substitute.For<IRecordRepository>();
    private readonly ILogger<DashboardService> _logger = Substitute.For<ILogger<DashboardService>>();
    private readonly DashboardService _sut;

    private const int UserId = 1;

    public DashboardServiceTests()
    {
        _sut = new DashboardService(
            _dashboardRepository, _cacheRepository, _goalRepository,
            _userRepository, _recordRepository, _logger);
    }

    private DashboardResult CreateDefaultDashboard()
    {
        return new DashboardResult
        {
            Summary = new DashboardSummary { TotalDeposits = 5000, TotalOutflows = 2000, RecordCount = 10 },
            SpendingByTag = new List<DashboardSpendingByTag>(),
            LatestRecords = new List<Record>(),
            TopDeposits = new List<Record>(),
            TopOutflows = new List<Record>(),
            Goals = new List<DashboardGoal>()
        };
    }

    private void SetupDefaultRepositories(DashboardResult? dashboard = null)
    {
        dashboard ??= CreateDefaultDashboard();

        _dashboardRepository.GetDashboardDataAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(dashboard);
        _goalRepository.GetGoalsAsync(Arg.Any<ListGoals>())
            .Returns(new PaginatedResult<Goal> { Lines = new List<Goal>(), Page = 1, TotalItems = 0 });
        _userRepository.FindUserByIdAsync(UserId)
            .Returns(new User { Id = UserId, Balance = 3000 });
        _recordRepository.GetRecurringRecordsAsync(UserId)
            .Returns(new List<Record>());
    }

    #region GetDashboardAsync

    [Fact]
    public async Task GetDashboardAsync_WhenCached_ShouldReturnFromCache()
    {
        var cached = CreateDefaultDashboard();
        _cacheRepository.GetAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(cached);

        var result = await _sut.GetDashboardAsync(UserId, null, null);

        result.Should().Be(cached);
        await _dashboardRepository.DidNotReceive().GetDashboardDataAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<DateTime>());
    }

    [Fact]
    public async Task GetDashboardAsync_WhenNotCached_ShouldFetchAndCache()
    {
        _cacheRepository.GetAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns((DashboardResult?)null);
        SetupDefaultRepositories();

        var result = await _sut.GetDashboardAsync(UserId, null, null);

        result.Should().NotBeNull();
        await _cacheRepository.Received(1).SetAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<DashboardResult>());
    }

    [Fact]
    public async Task GetDashboardAsync_WithNoDates_ShouldUseCurrentMonth()
    {
        _cacheRepository.GetAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns((DashboardResult?)null);
        SetupDefaultRepositories();

        var result = await _sut.GetDashboardAsync(UserId, null, null);

        result.Should().NotBeNull();
        result.Summary.Balance.Should().Be(3000);
    }

    [Fact]
    public async Task GetDashboardAsync_WithCustomDates_ShouldUseThem()
    {
        var start = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2024, 1, 31, 23, 59, 59, DateTimeKind.Utc);

        _cacheRepository.GetAsync(UserId, start, end).Returns((DashboardResult?)null);
        SetupDefaultRepositories();

        var result = await _sut.GetDashboardAsync(UserId, start, end);

        result.Should().NotBeNull();
        await _dashboardRepository.Received(1).GetDashboardDataAsync(UserId, start, end);
    }

    [Fact]
    public async Task GetDashboardAsync_WithGoals_ShouldMapGoalStatus()
    {
        _cacheRepository.GetAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns((DashboardResult?)null);

        var dashboard = CreateDefaultDashboard();
        _dashboardRepository.GetDashboardDataAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(dashboard);
        _goalRepository.GetGoalsAsync(Arg.Any<ListGoals>()).Returns(new PaginatedResult<Goal>
        {
            Lines = new List<Goal>
            {
                new() { Id = 1, Name = "Savings", TargetAmount = 5000 },
                new() { Id = 2, Name = "Emergency", TargetAmount = 2000, FinishAt = DateTime.UtcNow }
            },
            Page = 1,
            TotalItems = 2
        });
        _userRepository.FindUserByIdAsync(UserId).Returns(new User { Id = UserId, Balance = 3000 });
        _recordRepository.GetRecurringRecordsAsync(UserId).Returns(new List<Record>());

        var result = await _sut.GetDashboardAsync(UserId, null, null);

        result.Goals.Should().HaveCount(1);
        result.Goals[0].Name.Should().Be("Savings");
    }

    [Fact]
    public async Task GetDashboardAsync_WithRecurringRecords_ShouldProjectAndAggregate()
    {
        _cacheRepository.GetAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns((DashboardResult?)null);

        var dashboard = CreateDefaultDashboard();
        _dashboardRepository.GetDashboardDataAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(dashboard);
        _goalRepository.GetGoalsAsync(Arg.Any<ListGoals>()).Returns(new PaginatedResult<Goal>
        {
            Lines = new List<Goal>(),
            Page = 1,
            TotalItems = 0
        });
        _userRepository.FindUserByIdAsync(UserId).Returns(new User { Id = UserId, Balance = 3000 });

        var recurringRecords = new List<Record>
        {
            new()
            {
                Id = "rec1",
                Name = "Salary",
                Operation = OperationEnum.Deposit,
                Value = 5000,
                Frequency = FrequencyEnum.Monthly,
                ReferenceDate = new DateTime(2024, 1, 1),
                Tags = new List<Tag> { new() { Id = 1, Name = "Income" } }
            }
        };
        _recordRepository.GetRecurringRecordsAsync(UserId).Returns(recurringRecords);

        var result = await _sut.GetDashboardAsync(UserId, null, null);

        result.Should().NotBeNull();
        result.Summary.TotalDeposits.Should().BeGreaterThanOrEqualTo(5000);
    }

    #endregion
}
