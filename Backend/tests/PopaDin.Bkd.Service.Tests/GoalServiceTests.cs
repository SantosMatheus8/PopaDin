using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Service;

namespace PopaDin.Bkd.Service.Tests;

public class GoalServiceTests
{
    private readonly IGoalRepository _goalRepository = Substitute.For<IGoalRepository>();
    private readonly IDashboardCacheRepository _dashboardCacheRepository = Substitute.For<IDashboardCacheRepository>();
    private readonly ILogger<GoalService> _logger = Substitute.For<ILogger<GoalService>>();
    private readonly GoalService _sut;

    public GoalServiceTests()
    {
        _sut = new GoalService(_goalRepository, _dashboardCacheRepository, _logger);
    }

    #region CreateGoalAsync

    [Fact]
    public async Task CreateGoalAsync_WithValidGoal_ShouldCreateAndReturn()
    {
        var goal = new Goal { Name = "Viagem", TargetAmount = 5000 };
        var createdGoal = new Goal { Id = 1, Name = "Viagem", TargetAmount = 5000 };
        var fetchedGoal = new Goal { Id = 1, Name = "Viagem", TargetAmount = 5000, User = new User { Id = 1 } };

        _goalRepository.CreateGoalAsync(Arg.Any<Goal>()).Returns(createdGoal);
        _goalRepository.FindGoalByIdAsync(1, 1).Returns(fetchedGoal);

        var result = await _sut.CreateGoalAsync(goal, 1);

        result.Should().Be(fetchedGoal);
        await _dashboardCacheRepository.Received(1).InvalidateAsync(1);
    }

    [Fact]
    public async Task CreateGoalAsync_WithInvalidTargetAmount_ShouldThrowException()
    {
        var goal = new Goal { Name = "Viagem", TargetAmount = 0 };

        var act = () => _sut.CreateGoalAsync(goal, 1);

        await act.Should().ThrowAsync<UnprocessableEntityException>();
    }

    #endregion

    #region GetGoalsAsync

    [Fact]
    public async Task GetGoalsAsync_ShouldSetUserIdAndReturn()
    {
        var listGoals = new ListGoals { Page = 1, ItemsPerPage = 20 };
        var expected = new PaginatedResult<Goal> { Lines = [new Goal { Id = 1 }], Page = 1, TotalItems = 1 };
        _goalRepository.GetGoalsAsync(Arg.Any<ListGoals>()).Returns(expected);

        var result = await _sut.GetGoalsAsync(listGoals, 1);

        result.Should().Be(expected);
        listGoals.UserId.Should().Be(1);
    }

    #endregion

    #region FindGoalByIdAsync

    [Fact]
    public async Task FindGoalByIdAsync_WhenExists_ShouldReturn()
    {
        var goal = new Goal { Id = 1, Name = "Test" };
        _goalRepository.FindGoalByIdAsync(1, 1).Returns(goal);

        var result = await _sut.FindGoalByIdAsync(1, 1);

        result.Should().Be(goal);
    }

    [Fact]
    public async Task FindGoalByIdAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _goalRepository.FindGoalByIdAsync(999, 1).Returns((Goal?)null);

        var act = () => _sut.FindGoalByIdAsync(999, 1);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Meta não encontrada");
    }

    #endregion

    #region UpdateGoalAsync

    [Fact]
    public async Task UpdateGoalAsync_WithValidData_ShouldUpdateAndInvalidateCache()
    {
        var existingGoal = new Goal { Id = 1, Name = "Old", TargetAmount = 1000 };
        var updateRequest = new Goal { Name = "New", TargetAmount = 2000 };
        var updatedGoal = new Goal { Id = 1, Name = "New", TargetAmount = 2000 };

        _goalRepository.FindGoalByIdAsync(1, 1).Returns(existingGoal, updatedGoal);

        var result = await _sut.UpdateGoalAsync(updateRequest, 1, 1);

        result.Name.Should().Be("New");
        result.TargetAmount.Should().Be(2000);
        await _dashboardCacheRepository.Received(1).InvalidateAsync(1);
    }

    [Fact]
    public async Task UpdateGoalAsync_WithInvalidTargetAmount_ShouldThrowException()
    {
        var updateRequest = new Goal { Name = "New", TargetAmount = 0 };

        var act = () => _sut.UpdateGoalAsync(updateRequest, 1, 1);

        await act.Should().ThrowAsync<UnprocessableEntityException>();
    }

    [Fact]
    public async Task UpdateGoalAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        var updateRequest = new Goal { Name = "New", TargetAmount = 2000 };
        _goalRepository.FindGoalByIdAsync(1, 1).Returns((Goal?)null);

        var act = () => _sut.UpdateGoalAsync(updateRequest, 1, 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeleteGoalAsync

    [Fact]
    public async Task DeleteGoalAsync_WhenExists_ShouldDeleteAndInvalidateCache()
    {
        var goal = new Goal { Id = 1, Name = "Test" };
        _goalRepository.FindGoalByIdAsync(1, 1).Returns(goal);

        await _sut.DeleteGoalAsync(1, 1);

        await _goalRepository.Received(1).DeleteGoalAsync(1);
        await _dashboardCacheRepository.Received(1).InvalidateAsync(1);
    }

    [Fact]
    public async Task DeleteGoalAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _goalRepository.FindGoalByIdAsync(999, 1).Returns((Goal?)null);

        var act = () => _sut.DeleteGoalAsync(999, 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region FinishGoalAsync

    [Fact]
    public async Task FinishGoalAsync_WhenExists_ShouldFinishAndInvalidateCache()
    {
        var goal = new Goal { Id = 1, Name = "Test" };
        _goalRepository.FindGoalByIdAsync(1, 1).Returns(goal);

        await _sut.FinishGoalAsync(1, 1);

        await _goalRepository.Received(1).FinishGoalAsync(1);
        await _dashboardCacheRepository.Received(1).InvalidateAsync(1);
    }

    [Fact]
    public async Task FinishGoalAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _goalRepository.FindGoalByIdAsync(999, 1).Returns((Goal?)null);

        var act = () => _sut.FinishGoalAsync(999, 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
