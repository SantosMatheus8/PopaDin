using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopaDin.Bkd.Api.Dtos.Budget;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using Mapster;
using PopaDin.Bkd.Domain.Models.Budget;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PopaDin.Bkd.Api.Controllers;

[Route("v1/[controller]")]
[ApiController]
[Authorize]
public class BudgetController(IBudgetService budgetService) : ControllerBase
{
    /// <summary>
    ///     Atraves dessa rota voce sera capaz de criar um budget
    /// </summary>
    /// <param name="createBudgetRequest">O objeto de requisicao para criar um budget</param>
    /// <returns>O budget criado</returns>
    /// <response code="201">Sucesso, e retorna um budget</response>
    [HttpPost]
    [ProducesResponseType(typeof(BudgetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetResponse>> CreateBudget([FromBody] CreateBudgetRequest createBudgetRequest)
    {        // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var userId = decimal.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var budget = createBudgetRequest.Adapt<Budget>();
        Budget budgetCreated = await budgetService.CreateBudgetAsync(budget, userId);
        var budgetResponse = budgetCreated.Adapt<BudgetResponse>();
        return Ok(budgetResponse);
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de buscar uma lista paginada de budgets
    /// </summary>
    /// <param name="listBudgetsRequest">O objeto de requisicao para buscar a lista paginada de budgets</param>
    /// <returns>Uma lista paginada de budgets</returns>
    /// <response code="200">Sucesso, e retorna uma lista paginada de budgets</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<BudgetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<BudgetResponse>>> GetBudgets([FromQuery] ListBudgetsRequest listBudgetsRequest)
    {
        var userId = decimal.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var listBudgets = listBudgetsRequest.Adapt<ListBudgets>();
        PaginatedResult<Budget> budgets = await budgetService.GetBudgetsAsync(listBudgets, userId);
        var budgetsResponse = budgets.Adapt<PaginatedResult<BudgetResponse>>();
        return Ok(budgetsResponse);
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de buscar um Budget
    /// </summary>
    /// <param name="budgetId">O codigo Budget</param>
    /// <returns>O Budget consultado</returns>
    /// <response code="200">Sucesso, e retorna um Budget</response>
    [HttpGet("{budgetId:decimal}")]
    [ProducesResponseType(typeof(BudgetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetResponse>> FindBudgetById([FromRoute] decimal budgetId)
    {
        var userId = decimal.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        Budget budget = await budgetService.FindBudgetByIdAsync(budgetId, userId);
        var budgetResponse = budget.Adapt<BudgetResponse>();
        return Ok(budgetResponse);
    }

    [HttpPut("{budgetId:decimal}")]
    [ProducesResponseType(typeof(BudgetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetResponse>> UpdateBudget([FromBody] UpdateBudgetRequest updateBudgetRequest,
        [FromRoute] decimal budgetId)
    {
        var userId = decimal.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var budget = updateBudgetRequest.Adapt<Budget>();
        Budget updatedBudget = await budgetService.UpdateBudgetAsync(budget, budgetId, userId);
        var budgetResponse = updatedBudget.Adapt<BudgetResponse>();
        return Ok(budgetResponse);
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de deletar um budget
    /// </summary>
    /// <param name="budgetId">O codigo do budget</param>
    /// <returns>Confirmação de deleção</returns>
    /// <response code="204">Sucesso, e retorna confirmação de deleção</response>
    [HttpDelete("{budgetId:decimal}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteBudget([FromRoute] decimal budgetId)
    {
        var userId = decimal.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        await budgetService.DeleteBudgetAsync(budgetId, userId);
        return NoContent();
    }
}
