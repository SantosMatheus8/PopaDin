using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopaDin.Bkd.Api.Dtos.Alert;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models.Alert;
using Mapster;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PopaDin.Bkd.Api.Controllers;

[Route("v1/[controller]")]
[ApiController]
[Authorize]
public class AlertController(IAlertService alertService) : ControllerBase
{
    /// <summary>
    ///     Atraves dessa rota voce sera capaz de criar um alerta
    /// </summary>
    /// <param name="createAlertRequest">O objeto de requisicao para criar um alerta</param>
    /// <returns>O alerta criado</returns>
    /// <response code="201">Sucesso, e retorna um alerta</response>
    [HttpPost]
    [ProducesResponseType(typeof(AlertResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AlertResponse>> CreateAlert([FromBody] CreateAlertRequest createAlertRequest)
    {
        var userId = decimal.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var alert = createAlertRequest.Adapt<Alert>();
        Alert alertCreated = await alertService.CreateAlertAsync(alert, userId);
        var alertResponse = alertCreated.Adapt<AlertResponse>();
        return Ok(alertResponse);
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de listar os alertas do usuario logado
    /// </summary>
    /// <returns>Uma lista de alertas do usuario</returns>
    /// <response code="200">Sucesso, e retorna a lista de alertas</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<AlertResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AlertResponse>>> GetAlerts()
    {
        var userId = decimal.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        List<Alert> alerts = await alertService.GetAlertsByUserIdAsync(userId);
        var alertsResponse = alerts.Adapt<List<AlertResponse>>();
        return Ok(alertsResponse);
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de ativar/desativar um alerta
    /// </summary>
    /// <param name="id">O codigo do alerta</param>
    /// <param name="toggleAlertRequest">O objeto com o status desejado</param>
    /// <returns>Confirmacao de alteracao</returns>
    /// <response code="204">Sucesso, alerta atualizado</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ToggleAlert([FromRoute] string id, [FromBody] ToggleAlertRequest toggleAlertRequest)
    {
        var userId = decimal.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        await alertService.ToggleAlertAsync(id, toggleAlertRequest.Active, userId);
        return NoContent();
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de deletar um alerta
    /// </summary>
    /// <param name="id">O codigo do alerta</param>
    /// <returns>Confirmacao de delecao</returns>
    /// <response code="204">Sucesso, e retorna confirmacao de delecao</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAlert([FromRoute] string id)
    {
        var userId = decimal.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        await alertService.DeleteAlertAsync(id, userId);
        return NoContent();
    }
}
