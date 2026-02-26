using Microsoft.AspNetCore.Mvc;
using PopaDin.Bkd.Api.Dtos.Budget;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using Mapster;
using PopaDin.Bkd.Domain.Models.Budget;

namespace PopaDin.Bkd.Api.Controllers;

[Route("v1/[controller]")]
[ApiController]
// [Authorize]
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
    {
        var budget = createBudgetRequest.Adapt<Budget>();
        Budget budgetCreated = await budgetService.CreateBudgetAsync(budget);
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
        // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var listBudgets = listBudgetsRequest.Adapt<ListBudgets>();
        PaginatedResult<Budget> budgets = await budgetService.GetBudgetsAsync(listBudgets);
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
        // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Budget budget = await budgetService.FindBudgetByIdAsync(budgetId);
        var budgetResponse = budget.Adapt<BudgetResponse>();
        return Ok(budgetResponse);
    }

    [HttpPut("{budgetId:decimal}")]
    [ProducesResponseType(typeof(BudgetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetResponse>> UpdateBudget([FromBody] UpdateBudgetRequest updateBudgetRequest,
        [FromRoute] decimal budgetId)
    {
        // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var budget = updateBudgetRequest.Adapt<Budget>();
        Budget updatedBudget = await budgetService.UpdateBudgetAsync(budget, budgetId);
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
        // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await budgetService.DeleteBudgetAsync(budgetId);
        return NoContent();
    }
}
