using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PopaDin.Bkd.Api.Dtos.Budget;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using Mapster;

namespace PopaDin.Bkd.Api.Controllers;

// [Authorize]
[Route("v1/[controller]")]
[ApiController]
public class BudgetController : ControllerBase
{
    private readonly IBudgetService _budgetService;

    public BudgetController(IBudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(BudgetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetResponse>> CreateBudget(
        [FromBody] CreateBudgetRequest createBudgetRequest
    )
    {
        try
        {
            Budget budgetCreated = await _budgetService.CreateBudgetAsync(createBudgetRequest);

            var budgetResponse = budgetCreated.Adapt<List<BudgetResponse>>();

            // BudgetResponse budgetResponse = new()
            // {
            //     Name = budgetCreated.Name,
            //     Goal = budgetCreated.Goal,
            //     CurrentAmount = budgetCreated.CurrentAmount,
            //     // UserId = budgetCreated.UserId,
            //     // User = budgetCreated.User.Detailed(),
            //     // FinishAt = budgetCreated.FinishAt
            // };
            Console.WriteLine("================");
            return Ok(budgetResponse);
        }
        catch (PopaBaseException ex)
        {
            return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
        }
    }

    // [HttpGet]
    // [ProducesResponseType(typeof(Budget), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<List<Budget>>> GetUserBudgets()
    // {
    //     try
    //     {
    //         var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //         List<Budget> userBudgets = await _budgetService.GetUserBudgets(int.Parse(userId));
    //         return Ok(userBudgets);
    //     }
    //     catch (PopaBaseException ex)
    //     {
    //         return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
    //     }
    // }
    //
    // [HttpGet("{id}")]
    // [ProducesResponseType(typeof(Budget), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<Budget>> FindBudgetById(int id)
    // {
    //     try
    //     {
    //         var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //         Budget budget = await _budgetService.FindBudgetById(id, int.Parse(userId));
    //         return Ok(budget);
    //     }
    //     catch (PopaBaseException ex)
    //     {
    //         return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
    //     }
    // }
    // [HttpPut("{id}")]
    // [ProducesResponseType(typeof(Budget), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<Budget>> UpdateBudget([FromBody] Budget updateBudgetRequest,
    //     int id)
    // {
    //     try
    //     {
    //         var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //         Budget updatedBudget = await _budgetService.UpdateBudget(updateBudgetRequest, id, int.Parse(userId));
    //         return Ok(updatedBudget);
    //     }
    //     catch (PopaBaseException ex)
    //     {
    //         return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
    //     }
    // }
    //
    // [HttpDelete("{id}")]
    // [ProducesResponseType(typeof(Budget), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<Budget>> DeleteBudget(int id)
    // {
    //     try
    //     {
    //         var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //         return Ok(await _budgetService.DeleteBudget(id, int.Parse(userId)));
    //     }
    //     catch (PopaBaseException ex)
    //     {
    //         return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
    //     }
    // }
}
