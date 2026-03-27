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
    private readonly IRecurrenceLogRepository _recurrenceLogRepository = Substitute.For<IRecurrenceLogRepository>();
    private readonly ILogger<DashboardService> _logger = Substitute.For<ILogger<DashboardService>>();
    private readonly DashboardService _sut;

    private const int UserId = 1;

    public DashboardServiceTests()
    {
        _recurrenceLogRepository
            .GetMaterializedOccurrencesAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new HashSet<(string, DateTime)>());

        _recurrenceLogRepository
            .GetMaterializedOccurrencesUpToAsync(Arg.Any<DateTime>())
            .Returns(new HashSet<(string, DateTime)>());

        _sut = new DashboardService(
            _dashboardRepository, _cacheRepository, _goalRepository,
            _userRepository, _recordRepository, _recurrenceLogRepository, _logger);
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
        _dashboardRepository.GetPeriodSummaryAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new DashboardSummary { TotalDeposits = 4000, TotalOutflows = 1800 });
        _dashboardRepository.GetMonthlyTrendAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<DashboardMonthlyTrend>
            {
                new() { Year = 2024, Month = 1, TotalDeposits = 4000, TotalOutflows = 1500 },
                new() { Year = 2024, Month = 2, TotalDeposits = 5000, TotalOutflows = 2000 }
            });
        _goalRepository.GetGoalsAsync(Arg.Any<ListGoals>())
            .Returns(new PaginatedResult<Goal> { Lines = new List<Goal>(), Page = 1, TotalItems = 0 });
        _userRepository.FindUserByIdAsync(UserId)
            .Returns(new User { Id = UserId, Balance = 3000 });
        _recordRepository.GetRecurringRecordsAsync(UserId)
            .Returns(new List<Record>());
        _recordRepository.GetCumulativeBalanceUpToAsync(UserId, Arg.Any<DateTime>())
            .Returns(3000m);
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
        _recordRepository.GetCumulativeBalanceUpToAsync(UserId, Arg.Any<DateTime>()).Returns(3000m);

        var result = await _sut.GetDashboardAsync(UserId, null, null);

        result.Goals.Should().HaveCount(1);
        result.Goals[0].Name.Should().Be("Savings");
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldPopulateComparison()
    {
        _cacheRepository.GetAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns((DashboardResult?)null);
        SetupDefaultRepositories();

        var result = await _sut.GetDashboardAsync(UserId, null, null);

        result.Comparison.Should().NotBeNull();
        result.Comparison!.PreviousTotalDeposits.Should().Be(4000);
        result.Comparison.PreviousTotalOutflows.Should().Be(1800);
    }

    [Fact]
    public async Task GetDashboardAsync_ComparisonDepositsChangePercent_ShouldBeCorrect()
    {
        _cacheRepository.GetAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns((DashboardResult?)null);

        var dashboard = CreateDefaultDashboard(); // TotalDeposits = 5000
        _dashboardRepository.GetDashboardDataAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(dashboard);
        _dashboardRepository.GetPeriodSummaryAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new DashboardSummary { TotalDeposits = 4000, TotalOutflows = 2000 }); // previous = 4000
        _dashboardRepository.GetMonthlyTrendAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<DashboardMonthlyTrend>());
        _goalRepository.GetGoalsAsync(Arg.Any<ListGoals>())
            .Returns(new PaginatedResult<Goal> { Lines = new List<Goal>(), Page = 1, TotalItems = 0 });
        _userRepository.FindUserByIdAsync(UserId).Returns(new User { Id = UserId, Balance = 3000 });
        _recordRepository.GetRecurringRecordsAsync(UserId).Returns(new List<Record>());
        _recordRepository.GetCumulativeBalanceUpToAsync(UserId, Arg.Any<DateTime>()).Returns(3000m);

        var result = await _sut.GetDashboardAsync(UserId, null, null);

        // (5000 - 4000) / 4000 * 100 = 25%
        result.Comparison!.DepositsChangePercent.Should().Be(25m);
    }

    [Fact]
    public async Task GetDashboardAsync_ComparisonWithZeroPreviousDeposits_ShouldReturnZeroPercent()
    {
        _cacheRepository.GetAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns((DashboardResult?)null);

        var dashboard = CreateDefaultDashboard();
        _dashboardRepository.GetDashboardDataAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(dashboard);
        _dashboardRepository.GetPeriodSummaryAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new DashboardSummary { TotalDeposits = 0, TotalOutflows = 0 });
        _dashboardRepository.GetMonthlyTrendAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new List<DashboardMonthlyTrend>());
        _goalRepository.GetGoalsAsync(Arg.Any<ListGoals>())
            .Returns(new PaginatedResult<Goal> { Lines = new List<Goal>(), Page = 1, TotalItems = 0 });
        _userRepository.FindUserByIdAsync(UserId).Returns(new User { Id = UserId, Balance = 3000 });
        _recordRepository.GetRecurringRecordsAsync(UserId).Returns(new List<Record>());
        _recordRepository.GetCumulativeBalanceUpToAsync(UserId, Arg.Any<DateTime>()).Returns(3000m);

        var result = await _sut.GetDashboardAsync(UserId, null, null);

        result.Comparison!.DepositsChangePercent.Should().Be(0);
        result.Comparison.OutflowsChangePercent.Should().Be(0);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldPopulateMonthlyTrend()
    {
        _cacheRepository.GetAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns((DashboardResult?)null);
        SetupDefaultRepositories();

        var result = await _sut.GetDashboardAsync(UserId, null, null);

        result.MonthlyTrend.Should().HaveCount(2);
        result.MonthlyTrend[0].Year.Should().Be(2024);
        result.MonthlyTrend[0].Month.Should().Be(1);
        result.MonthlyTrend[0].TotalDeposits.Should().Be(4000);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldCallGetMonthlyTrendWithLast6Months()
    {
        _cacheRepository.GetAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns((DashboardResult?)null);
        SetupDefaultRepositories();

        var periodStart = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = new DateTime(2024, 6, 30, 23, 59, 59, DateTimeKind.Utc);

        await _sut.GetDashboardAsync(UserId, periodStart, periodEnd);

        // Trend should start 5 months before June 2024 = January 2024
        await _dashboardRepository.Received(1).GetMonthlyTrendAsync(
            UserId,
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Arg.Any<DateTime>());
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
        _recordRepository.GetCumulativeBalanceUpToAsync(UserId, Arg.Any<DateTime>()).Returns(3000m);

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
