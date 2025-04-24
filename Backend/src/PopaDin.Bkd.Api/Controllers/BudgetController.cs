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
    public async Task<ActionResult<BudgetResponse>> CreateBudget(
        [FromBody] CreateBudgetRequest createBudgetRequest
    )
    {
        try
        {
            var budget = createBudgetRequest.Adapt<Budget>();
            Budget budgetCreated = await _budgetService.CreateBudgetAsync(budget);
            var budgetResponse = budgetCreated.Adapt<BudgetResponse>();

            return Ok(budgetResponse);
        }
        catch (PopaBaseException ex)
        {
            return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
        }
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de buscar uma lista paginada de budgets
    /// </summary>
    /// <param name="listBudgetsRequest">O objeto de requisicao para buscar a lista paginada de budgets</param>
    /// <returns>Uma lista paginada de budgets</returns>
    /// <response code="200">Sucesso, e retorna uma lista paginada de budgets</response>
    [HttpGet]
    [ProducesResponseType(typeof(Budget), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<Budget>>> GetBudgets([FromQuery] ListBudgetsRequest listBudgetsRequest)
    {
        try
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var listBudgets = listBudgetsRequest.Adapt<ListBudgets>();
            PaginatedResult<Budget> budgets = await _budgetService.GetBudgetsAsync(listBudgets);
            return Ok(budgets);
        }
        catch (PopaBaseException ex)
        {
            return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
        }
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de buscar um Budget
    /// </summary>
    /// <param name="budgetId">O codigo Budget</param>
    /// <returns>O Budget consultado</returns>
    /// <response code="200">Sucesso, e retorna um Budget</response>
    [HttpGet("{budgetId:decimal}")]
    [ProducesResponseType(typeof(Budget), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Budget>> FindBudgetById([FromRoute] decimal budgetId)
    {
        try
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Budget budget = await _budgetService.FindBudgetByIdAsync(budgetId);
            return Ok(budget);
        }
        catch (PopaBaseException ex)
        {
            return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
        }
    }

    [HttpPut("{budgetId:decimal}")]
    [ProducesResponseType(typeof(Budget), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Budget>> UpdateBudget([FromBody] UpdateBudgetRequest updateBudgetRequest,
        [FromRoute] decimal budgetId)
    {
        try
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var budget = updateBudgetRequest.Adapt<Budget>();
            Budget updatedBudget = await _budgetService.UpdateBudgetAsync(budget, budgetId);
            return Ok(updatedBudget);
        }
        catch (PopaBaseException ex)
        {
            return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
        }
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de deletar um budget
    /// </summary>
    /// <param name="budgetId">O codigo do budget</param>
    /// <returns>Confirmação de deleção</returns>
    /// <response code="204">Sucesso, e retorna confirmação de deleção</response>
    [HttpDelete("{budgetId:decimal}")]
    [ProducesResponseType(typeof(Budget), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Budget>> DeleteBudget([FromRoute] decimal budgetId)
    {
        try
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _budgetService.DeleteBudgetAsync(budgetId);
            return NoContent();
        }
        catch (PopaBaseException ex)
        {
            return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
        }
    }
}
