using Microsoft.AspNetCore.Mvc;
using PopaDin.Bkd.Api.Dtos.Record;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using Mapster;
using PopaDin.Bkd.Domain.Models.Record;

namespace PopaDin.Bkd.Api.Controllers;

[Route("v1/[controller]")]
[ApiController]
// [Authorize]
public class RecordController(IRecordService recordService) : ControllerBase
{
    /// <summary>
    ///     Atraves dessa rota voce sera capaz de criar um record
    /// </summary>
    /// <param name="createRecordRequest">O objeto de requisicao para criar um record</param>
    /// <returns>O record criado</returns>
    /// <response code="201">Sucesso, e retorna um record</response>
    [HttpPost]
    [ProducesResponseType(typeof(RecordResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecordResponse>> CreateRecord(
        [FromBody] CreateRecordRequest createRecordRequest
    )
    {
        try
        {
            var record = createRecordRequest.Adapt<Record>();
            Record recordCreated = await recordService.CreateRecordAsync(record, createRecordRequest.TagIds);
            var recordResponse = recordCreated.Adapt<RecordResponse>();

            return Ok(recordResponse);
        }
        catch (PopaBaseException ex)
        {
            return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
        }
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de buscar uma lista paginada de records
    /// </summary>
    /// <param name="listRecordsRequest">O objeto de requisicao para buscar a lista paginada de records</param>
    /// <returns>Uma lista paginada de records</returns>
    /// <response code="200">Sucesso, e retorna uma lista paginada de records</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<RecordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<RecordResponse>>> GetRecords([FromQuery] ListRecordsRequest listRecordsRequest)
    {
        try
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var listRecords = listRecordsRequest.Adapt<ListRecords>();
            PaginatedResult<Record> records = await recordService.GetRecordsAsync(listRecords);
            var recordsResponse = records.Adapt<PaginatedResult<RecordResponse>>();
            return Ok(recordsResponse);
        }
        catch (PopaBaseException ex)
        {
            return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
        }
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de buscar um Record
    /// </summary>
    /// <param name="recordId">O codigo Record</param>
    /// <returns>O Record consultado</returns>
    /// <response code="200">Sucesso, e retorna um Record</response>
    [HttpGet("{recordId:decimal}")]
    [ProducesResponseType(typeof(RecordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecordResponse>> FindRecordById([FromRoute] decimal recordId)
    {
        try
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Record record = await recordService.FindRecordByIdAsync(recordId);
            var recordResponse = record.Adapt<RecordResponse>();
            return Ok(recordResponse);
        }
        catch (PopaBaseException ex)
        {
            return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
        }
    }

    [HttpPut("{recordId:decimal}")]
    [ProducesResponseType(typeof(RecordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecordResponse>> UpdateRecord([FromBody] UpdateRecordRequest updateRecordRequest,
        [FromRoute] decimal recordId)
    {
        try
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var record = updateRecordRequest.Adapt<Record>();
            Record updatedRecord = await recordService.UpdateRecordAsync(record, updateRecordRequest.TagIds, recordId);
            return Ok(updatedRecord.Adapt<RecordResponse>());
        }
        catch (PopaBaseException ex)
        {
            return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
        }
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de deletar um record
    /// </summary>
    /// <param name="recordId">O codigo do record</param>
    /// <returns>Confirmação de deleção</returns>
    /// <response code="204">Sucesso, e retorna confirmação de deleção</response>
    [HttpDelete("{recordId:decimal}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Record>> DeleteRecord([FromRoute] decimal recordId)
    {
        try
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await recordService.DeleteRecordAsync(recordId);
            return NoContent();
        }
        catch (PopaBaseException ex)
        {
            return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
        }
    }
}