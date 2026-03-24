using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PopaDin.Bkd.Api.Controllers;
using PopaDin.Bkd.Api.Dtos.Goal;
using PopaDin.Bkd.Api.Tests.Helpers;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Api.Tests.Controllers;

public class GoalControllerTests
{
    private readonly IGoalService _goalService = Substitute.For<IGoalService>();
    private readonly GoalController _sut;

    private const int AuthUserId = 1;

    public GoalControllerTests()
    {
        _sut = new GoalController(_goalService);
        ControllerTestHelper.SetupAuthenticatedUser(_sut, AuthUserId);
    }

    #region CreateGoal

    [Fact]
    public async Task CreateGoal_WithValidRequest_ShouldReturn201Created()
    {
        var request = new CreateGoalRequest { Name = "Trip", TargetAmount = 5000 };
        _goalService.CreateGoalAsync(Arg.Any<Goal>(), AuthUserId)
            .Returns(new Goal { Id = 1, Name = "Trip", TargetAmount = 5000 });

        var result = await _sut.CreateGoal(request);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task CreateGoal_WithInvalidTargetAmount_ShouldThrowException()
    {
        var request = new CreateGoalRequest { Name = "Trip", TargetAmount = 0 };
        _goalService.CreateGoalAsync(Arg.Any<Goal>(), AuthUserId)
            .Throws(new UnprocessableEntityException("Valor da meta inválido"));

        var act = () => _sut.CreateGoal(request);

        await act.Should().ThrowAsync<UnprocessableEntityException>();
    }

    #endregion

    #region GetGoals

    [Fact]
    public async Task GetGoals_ShouldReturnOkWithPaginatedResult()
    {
        var request = new ListGoalsRequest();
        _goalService.GetGoalsAsync(Arg.Any<ListGoals>(), AuthUserId)
            .Returns(new PaginatedResult<Goal> { Lines = [new Goal { Id = 1 }], Page = 1, TotalItems = 1 });

        var result = await _sut.GetGoals(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    #endregion

    #region FindGoalById

    [Fact]
    public async Task FindGoalById_WhenExists_ShouldReturnOk()
    {
        _goalService.FindGoalByIdAsync(1, AuthUserId)
            .Returns(new Goal { Id = 1, Name = "Test" });

        var result = await _sut.FindGoalById(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task FindGoalById_WhenNotFound_ShouldThrowNotFoundException()
    {
        _goalService.FindGoalByIdAsync(999, AuthUserId)
            .Throws(new NotFoundException("Meta não encontrada"));

        var act = () => _sut.FindGoalById(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region UpdateGoal

    [Fact]
    public async Task UpdateGoal_WithValidRequest_ShouldReturnOk()
    {
        var request = new UpdateGoalRequest { Name = "Updated", TargetAmount = 10000 };
        _goalService.UpdateGoalAsync(Arg.Any<Goal>(), 1, AuthUserId)
            .Returns(new Goal { Id = 1, Name = "Updated", TargetAmount = 10000 });

        var result = await _sut.UpdateGoal(request, 1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateGoal_WhenNotFound_ShouldThrowNotFoundException()
    {
        var request = new UpdateGoalRequest { Name = "Updated", TargetAmount = 10000 };
        _goalService.UpdateGoalAsync(Arg.Any<Goal>(), 999, AuthUserId)
            .Throws(new NotFoundException("Meta não encontrada"));

        var act = () => _sut.UpdateGoal(request, 999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeleteGoal

    [Fact]
    public async Task DeleteGoal_WhenExists_ShouldReturn204NoContent()
    {
        var result = await _sut.DeleteGoal(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteGoal_WhenNotFound_ShouldThrowNotFoundException()
    {
        _goalService.DeleteGoalAsync(999, AuthUserId)
            .Throws(new NotFoundException("Meta não encontrada"));

        var act = () => _sut.DeleteGoal(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region FinishGoal

    [Fact]
    public async Task FinishGoal_WhenExists_ShouldReturn204NoContent()
    {
        var result = await _sut.FinishGoal(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task FinishGoal_WhenNotFound_ShouldThrowNotFoundException()
    {
        _goalService.FinishGoalAsync(999, AuthUserId)
            .Throws(new NotFoundException("Meta não encontrada"));

        var act = () => _sut.FinishGoal(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
