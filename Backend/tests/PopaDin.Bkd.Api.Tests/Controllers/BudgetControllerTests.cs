using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PopaDin.Bkd.Api.Controllers;
using PopaDin.Bkd.Api.Dtos.Budget;
using PopaDin.Bkd.Api.Tests.Helpers;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Api.Tests.Controllers;

public class BudgetControllerTests
{
    private readonly IBudgetService _budgetService = Substitute.For<IBudgetService>();
    private readonly BudgetController _sut;

    private const int AuthUserId = 1;

    public BudgetControllerTests()
    {
        _sut = new BudgetController(_budgetService);
        ControllerTestHelper.SetupAuthenticatedUser(_sut, AuthUserId);
    }

    #region CreateBudget

    [Fact]
    public async Task CreateBudget_WithValidRequest_ShouldReturn201Created()
    {
        var request = new CreateBudgetRequest { Name = "Trip", Goal = 5000 };
        _budgetService.CreateBudgetAsync(Arg.Any<Budget>(), AuthUserId)
            .Returns(new Budget { Id = 1, Name = "Trip", Goal = 5000 });

        var result = await _sut.CreateBudget(request);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task CreateBudget_WithInvalidGoal_ShouldThrowException()
    {
        var request = new CreateBudgetRequest { Name = "Trip", Goal = 0 };
        _budgetService.CreateBudgetAsync(Arg.Any<Budget>(), AuthUserId)
            .Throws(new UnprocessableEntityException("Goal inválido"));

        var act = () => _sut.CreateBudget(request);

        await act.Should().ThrowAsync<UnprocessableEntityException>();
    }

    #endregion

    #region GetBudgets

    [Fact]
    public async Task GetBudgets_ShouldReturnOkWithPaginatedResult()
    {
        var request = new ListBudgetsRequest();
        _budgetService.GetBudgetsAsync(Arg.Any<ListBudgets>(), AuthUserId)
            .Returns(new PaginatedResult<Budget> { Lines = [new Budget { Id = 1 }], Page = 1, TotalItems = 1 });

        var result = await _sut.GetBudgets(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    #endregion

    #region FindBudgetById

    [Fact]
    public async Task FindBudgetById_WhenExists_ShouldReturnOk()
    {
        _budgetService.FindBudgetByIdAsync(1, AuthUserId)
            .Returns(new Budget { Id = 1, Name = "Test" });

        var result = await _sut.FindBudgetById(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task FindBudgetById_WhenNotFound_ShouldThrowNotFoundException()
    {
        _budgetService.FindBudgetByIdAsync(999, AuthUserId)
            .Throws(new NotFoundException("Budget não encontrado"));

        var act = () => _sut.FindBudgetById(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region UpdateBudget

    [Fact]
    public async Task UpdateBudget_WithValidRequest_ShouldReturnOk()
    {
        var request = new UpdateBudgetRequest { Name = "Updated", Goal = 10000 };
        _budgetService.UpdateBudgetAsync(Arg.Any<Budget>(), 1, AuthUserId)
            .Returns(new Budget { Id = 1, Name = "Updated", Goal = 10000 });

        var result = await _sut.UpdateBudget(request, 1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateBudget_WhenNotFound_ShouldThrowNotFoundException()
    {
        var request = new UpdateBudgetRequest { Name = "Updated", Goal = 10000 };
        _budgetService.UpdateBudgetAsync(Arg.Any<Budget>(), 999, AuthUserId)
            .Throws(new NotFoundException("Budget não encontrado"));

        var act = () => _sut.UpdateBudget(request, 999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeleteBudget

    [Fact]
    public async Task DeleteBudget_WhenExists_ShouldReturn204NoContent()
    {
        var result = await _sut.DeleteBudget(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteBudget_WhenNotFound_ShouldThrowNotFoundException()
    {
        _budgetService.DeleteBudgetAsync(999, AuthUserId)
            .Throws(new NotFoundException("Budget não encontrado"));

        var act = () => _sut.DeleteBudget(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region FinishBudget

    [Fact]
    public async Task FinishBudget_WhenExists_ShouldReturn204NoContent()
    {
        var result = await _sut.FinishBudget(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task FinishBudget_WhenNotFound_ShouldThrowNotFoundException()
    {
        _budgetService.FinishBudgetAsync(999, AuthUserId)
            .Throws(new NotFoundException("Budget não encontrado"));

        var act = () => _sut.FinishBudget(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
