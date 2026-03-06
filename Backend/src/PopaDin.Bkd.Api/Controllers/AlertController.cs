using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopaDin.Bkd.Api.Dtos.Alert;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using Mapster;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var alert = createAlertRequest.Adapt<Alert>();
        Alert alertCreated = await alertService.CreateAlertAsync(alert, userId);
        var alertResponse = alertCreated.Adapt<AlertResponse>();
        return StatusCode(StatusCodes.Status201Created, alertResponse);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<AlertResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AlertResponse>>> GetAlerts()
    {
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        List<Alert> alerts = await alertService.GetAlertsByUserIdAsync(userId);
        var alertsResponse = alerts.Adapt<List<AlertResponse>>();
        return Ok(alertsResponse);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ToggleAlert([FromRoute] string id, [FromBody] ToggleAlertRequest toggleAlertRequest)
    {
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        await alertService.ToggleAlertAsync(id, toggleAlertRequest.Active, userId);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAlert([FromRoute] string id)
    {
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        await alertService.DeleteAlertAsync(id, userId);
        return NoContent();
    }
}
