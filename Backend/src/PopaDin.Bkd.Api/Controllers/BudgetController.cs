using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopaDin.Bkd.Api.Dtos.Budget;
using PopaDin.Bkd.Api.Extensions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using Mapster;

namespace PopaDin.Bkd.Api.Controllers;

[Route("v1/[controller]")]
[ApiController]
[Authorize]
public class BudgetController(IBudgetService budgetService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(BudgetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BudgetResponse>> CreateBudget([FromBody] CreateBudgetRequest createBudgetRequest)
    {
        var userId = User.GetUserId();
        var budget = createBudgetRequest.Adapt<Budget>();
        Budget budgetCreated = await budgetService.CreateBudgetAsync(budget, userId);
        var budgetResponse = budgetCreated.Adapt<BudgetResponse>();
        return StatusCode(StatusCodes.Status201Created, budgetResponse);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<BudgetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResult<BudgetResponse>>> GetBudgets([FromQuery] ListBudgetsRequest listBudgetsRequest)
    {
        var userId = User.GetUserId();
        var listBudgets = listBudgetsRequest.Adapt<ListBudgets>();
        PaginatedResult<Budget> budgets = await budgetService.GetBudgetsAsync(listBudgets, userId);
        var budgetsResponse = budgets.Adapt<PaginatedResult<BudgetResponse>>();
        return Ok(budgetsResponse);
    }

    [HttpGet("{budgetId:int}")]
    [ProducesResponseType(typeof(BudgetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetResponse>> FindBudgetById([FromRoute] int budgetId)
    {
        var userId = User.GetUserId();
        Budget budget = await budgetService.FindBudgetByIdAsync(budgetId, userId);
        var budgetResponse = budget.Adapt<BudgetResponse>();
        return Ok(budgetResponse);
    }

    [HttpPut("{budgetId:int}")]
    [ProducesResponseType(typeof(BudgetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetResponse>> UpdateBudget([FromBody] UpdateBudgetRequest updateBudgetRequest,
        [FromRoute] int budgetId)
    {
        var userId = User.GetUserId();
        var budget = updateBudgetRequest.Adapt<Budget>();
        Budget updatedBudget = await budgetService.UpdateBudgetAsync(budget, budgetId, userId);
        var budgetResponse = updatedBudget.Adapt<BudgetResponse>();
        return Ok(budgetResponse);
    }

    [HttpDelete("{budgetId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteBudget([FromRoute] int budgetId)
    {
        var userId = User.GetUserId();
        await budgetService.DeleteBudgetAsync(budgetId, userId);
        return NoContent();
    }

    [HttpPatch("{budgetId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> FinishBudget([FromRoute] int budgetId)
    {
        var userId = User.GetUserId();
        await budgetService.FinishBudgetAsync(budgetId, userId);
        return NoContent();
    }
}
