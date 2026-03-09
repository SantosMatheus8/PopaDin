using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopaDin.Bkd.Api.Dtos.Dashboard;
using PopaDin.Bkd.Domain.Interfaces.Services;
using Mapster;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PopaDin.Bkd.Api.Controllers;

[Route("v1/[controller]")]
[ApiController]
[Authorize]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DashboardResponse>> GetDashboard([FromQuery] DashboardRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var dashboard = await dashboardService.GetDashboardAsync(userId, request.StartDate, request.EndDate);
        var response = dashboard.Adapt<DashboardResponse>();
        return Ok(response);
    }
}
