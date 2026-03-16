using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Service;

namespace PopaDin.Bkd.Service.Tests;

public class BudgetServiceTests
{
    private readonly IBudgetRepository _budgetRepository = Substitute.For<IBudgetRepository>();
    private readonly IDashboardCacheRepository _dashboardCacheRepository = Substitute.For<IDashboardCacheRepository>();
    private readonly ILogger<BudgetService> _logger = Substitute.For<ILogger<BudgetService>>();
    private readonly BudgetService _sut;

    public BudgetServiceTests()
    {
        _sut = new BudgetService(_budgetRepository, _dashboardCacheRepository, _logger);
    }

    #region CreateBudgetAsync

    [Fact]
    public async Task CreateBudgetAsync_WithValidBudget_ShouldCreateAndReturn()
    {
        var budget = new Budget { Name = "Viagem", Goal = 5000 };
        var createdBudget = new Budget { Id = 1, Name = "Viagem", Goal = 5000 };
        var fetchedBudget = new Budget { Id = 1, Name = "Viagem", Goal = 5000, User = new User { Id = 1 } };

        _budgetRepository.CreateBudgetAsync(Arg.Any<Budget>()).Returns(createdBudget);
        _budgetRepository.FindBudgetByIdAsync(1, 1).Returns(fetchedBudget);

        var result = await _sut.CreateBudgetAsync(budget, 1);

        result.Should().Be(fetchedBudget);
        await _dashboardCacheRepository.Received(1).InvalidateAsync(1);
    }

    [Fact]
    public async Task CreateBudgetAsync_WithInvalidGoal_ShouldThrowException()
    {
        var budget = new Budget { Name = "Viagem", Goal = 0 };

        var act = () => _sut.CreateBudgetAsync(budget, 1);

        await act.Should().ThrowAsync<UnprocessableEntityException>();
    }

    #endregion

    #region GetBudgetsAsync

    [Fact]
    public async Task GetBudgetsAsync_ShouldSetUserIdAndReturn()
    {
        var listBudgets = new ListBudgets { Page = 1, ItemsPerPage = 20 };
        var expected = new PaginatedResult<Budget> { Lines = [new Budget { Id = 1 }], Page = 1, TotalItems = 1 };
        _budgetRepository.GetBudgetsAsync(Arg.Any<ListBudgets>()).Returns(expected);

        var result = await _sut.GetBudgetsAsync(listBudgets, 1);

        result.Should().Be(expected);
        listBudgets.UserId.Should().Be(1);
    }

    #endregion

    #region FindBudgetByIdAsync

    [Fact]
    public async Task FindBudgetByIdAsync_WhenExists_ShouldReturn()
    {
        var budget = new Budget { Id = 1, Name = "Test" };
        _budgetRepository.FindBudgetByIdAsync(1, 1).Returns(budget);

        var result = await _sut.FindBudgetByIdAsync(1, 1);

        result.Should().Be(budget);
    }

    [Fact]
    public async Task FindBudgetByIdAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _budgetRepository.FindBudgetByIdAsync(999, 1).Returns((Budget?)null);

        var act = () => _sut.FindBudgetByIdAsync(999, 1);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Budget não encontrado");
    }

    #endregion

    #region UpdateBudgetAsync

    [Fact]
    public async Task UpdateBudgetAsync_WithValidData_ShouldUpdateAndInvalidateCache()
    {
        var existingBudget = new Budget { Id = 1, Name = "Old", Goal = 1000 };
        var updateRequest = new Budget { Name = "New", Goal = 2000 };
        var updatedBudget = new Budget { Id = 1, Name = "New", Goal = 2000 };

        _budgetRepository.FindBudgetByIdAsync(1, 1).Returns(existingBudget, updatedBudget);

        var result = await _sut.UpdateBudgetAsync(updateRequest, 1, 1);

        result.Name.Should().Be("New");
        result.Goal.Should().Be(2000);
        await _dashboardCacheRepository.Received(1).InvalidateAsync(1);
    }

    [Fact]
    public async Task UpdateBudgetAsync_WithInvalidGoal_ShouldThrowException()
    {
        var updateRequest = new Budget { Name = "New", Goal = 0 };

        var act = () => _sut.UpdateBudgetAsync(updateRequest, 1, 1);

        await act.Should().ThrowAsync<UnprocessableEntityException>();
    }

    [Fact]
    public async Task UpdateBudgetAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        var updateRequest = new Budget { Name = "New", Goal = 2000 };
        _budgetRepository.FindBudgetByIdAsync(1, 1).Returns((Budget?)null);

        var act = () => _sut.UpdateBudgetAsync(updateRequest, 1, 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeleteBudgetAsync

    [Fact]
    public async Task DeleteBudgetAsync_WhenExists_ShouldDeleteAndInvalidateCache()
    {
        var budget = new Budget { Id = 1, Name = "Test" };
        _budgetRepository.FindBudgetByIdAsync(1, 1).Returns(budget);

        await _sut.DeleteBudgetAsync(1, 1);

        await _budgetRepository.Received(1).DeleteBudgetAsync(1);
        await _dashboardCacheRepository.Received(1).InvalidateAsync(1);
    }

    [Fact]
    public async Task DeleteBudgetAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _budgetRepository.FindBudgetByIdAsync(999, 1).Returns((Budget?)null);

        var act = () => _sut.DeleteBudgetAsync(999, 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region FinishBudgetAsync

    [Fact]
    public async Task FinishBudgetAsync_WhenExists_ShouldFinishAndInvalidateCache()
    {
        var budget = new Budget { Id = 1, Name = "Test" };
        _budgetRepository.FindBudgetByIdAsync(1, 1).Returns(budget);

        await _sut.FinishBudgetAsync(1, 1);

        await _budgetRepository.Received(1).FinishBudgetAsync(1);
        await _dashboardCacheRepository.Received(1).InvalidateAsync(1);
    }

    [Fact]
    public async Task FinishBudgetAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _budgetRepository.FindBudgetByIdAsync(999, 1).Returns((Budget?)null);

        var act = () => _sut.FinishBudgetAsync(999, 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
