using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopaDin.Bkd.Api.Dtos.Alert;
using PopaDin.Bkd.Api.Extensions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using Mapster;

namespace PopaDin.Bkd.Api.Controllers;

[Route("v1/[controller]")]
[ApiController]
[Authorize]
public class AlertController(IAlertService alertService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(AlertResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AlertResponse>> CreateAlert([FromBody] CreateAlertRequest createAlertRequest)
    {
        var userId = User.GetUserId();
        var alert = createAlertRequest.Adapt<Alert>();
        Alert alertCreated = await alertService.CreateAlertAsync(alert, userId);
        var alertResponse = alertCreated.Adapt<AlertResponse>();
        return StatusCode(StatusCodes.Status201Created, alertResponse);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<AlertResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AlertResponse>>> GetAlerts()
    {
        var userId = User.GetUserId();
        List<Alert> alerts = await alertService.GetAlertsByUserIdAsync(userId);
        var alertsResponse = alerts.Adapt<List<AlertResponse>>();
        return Ok(alertsResponse);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ToggleAlert([FromRoute] string id, [FromBody] ToggleAlertRequest toggleAlertRequest)
    {
        var userId = User.GetUserId();
        await alertService.ToggleAlertAsync(id, toggleAlertRequest.Active, userId);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAlert([FromRoute] string id)
    {
        var userId = User.GetUserId();
        await alertService.DeleteAlertAsync(id, userId);
        return NoContent();
    }
}
