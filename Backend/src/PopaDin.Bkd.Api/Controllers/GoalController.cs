using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopaDin.Bkd.Api.Dtos.Goal;
using PopaDin.Bkd.Api.Extensions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using Mapster;

namespace PopaDin.Bkd.Api.Controllers;

[Route("v1/[controller]")]
[ApiController]
[Authorize]
public class GoalController(IGoalService goalService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(GoalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GoalResponse>> CreateGoal([FromBody] CreateGoalRequest createGoalRequest)
    {
        var userId = User.GetUserId();
        var goal = createGoalRequest.Adapt<Goal>();
        Goal goalCreated = await goalService.CreateGoalAsync(goal, userId);
        var goalResponse = goalCreated.Adapt<GoalResponse>();
        return StatusCode(StatusCodes.Status201Created, goalResponse);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<GoalResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResult<GoalResponse>>> GetGoals([FromQuery] ListGoalsRequest listGoalsRequest)
    {
        var userId = User.GetUserId();
        var listGoals = listGoalsRequest.Adapt<ListGoals>();
        PaginatedResult<Goal> goals = await goalService.GetGoalsAsync(listGoals, userId);
        var goalsResponse = goals.Adapt<PaginatedResult<GoalResponse>>();
        return Ok(goalsResponse);
    }

    [HttpGet("{goalId:int}")]
    [ProducesResponseType(typeof(GoalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GoalResponse>> FindGoalById([FromRoute] int goalId)
    {
        var userId = User.GetUserId();
        Goal goal = await goalService.FindGoalByIdAsync(goalId, userId);
        var goalResponse = goal.Adapt<GoalResponse>();
        return Ok(goalResponse);
    }

    [HttpPut("{goalId:int}")]
    [ProducesResponseType(typeof(GoalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GoalResponse>> UpdateGoal([FromBody] UpdateGoalRequest updateGoalRequest,
        [FromRoute] int goalId)
    {
        var userId = User.GetUserId();
        var goal = updateGoalRequest.Adapt<Goal>();
        Goal updatedGoal = await goalService.UpdateGoalAsync(goal, goalId, userId);
        var goalResponse = updatedGoal.Adapt<GoalResponse>();
        return Ok(goalResponse);
    }

    [HttpDelete("{goalId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteGoal([FromRoute] int goalId)
    {
        var userId = User.GetUserId();
        await goalService.DeleteGoalAsync(goalId, userId);
        return NoContent();
    }

    [HttpPatch("{goalId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> FinishGoal([FromRoute] int goalId)
    {
        var userId = User.GetUserId();
        await goalService.FinishGoalAsync(goalId, userId);
        return NoContent();
    }
}
