using Microsoft.AspNetCore.Mvc;
using PopaDin.Bkd.Api.Dtos.Record;
using PopaDin.Bkd.Api.Extensions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;

namespace PopaDin.Bkd.Api.Controllers;

[Route("v1/[controller]")]
[ApiController]
[Authorize]
public class RecordController(
    IRecordService recordService,
    IExportService exportService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(RecordResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecordResponse>> CreateRecord([FromBody] CreateRecordRequest createRecordRequest)
    {
        var userId = User.GetUserId();
        var record = createRecordRequest.Adapt<Record>();
        Record recordCreated = await recordService.CreateRecordAsync(record, createRecordRequest.TagIds, userId, createRecordRequest.Installments);
        var recordResponse = recordCreated.Adapt<RecordResponse>();
        return StatusCode(StatusCodes.Status201Created, recordResponse);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<RecordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResult<RecordResponse>>> GetRecords([FromQuery] ListRecordsRequest listRecordsRequest)
    {
        var userId = User.GetUserId();
        var listRecords = listRecordsRequest.Adapt<ListRecords>();
        PaginatedResult<Record> records = await recordService.GetRecordsAsync(listRecords, userId);
        var recordsResponse = records.Adapt<PaginatedResult<RecordResponse>>();
        return Ok(recordsResponse);
    }

    [HttpGet("{recordId}")]
    [ProducesResponseType(typeof(RecordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecordResponse>> FindRecordById([FromRoute] string recordId)
    {
        var userId = User.GetUserId();
        Record record = await recordService.FindRecordByIdAsync(recordId, userId);
        var recordResponse = record.Adapt<RecordResponse>();
        return Ok(recordResponse);
    }

    [HttpPut("{recordId}")]
    [ProducesResponseType(typeof(RecordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecordResponse>> UpdateRecord([FromBody] UpdateRecordRequest updateRecordRequest,
        [FromRoute] string recordId)
    {
        var userId = User.GetUserId();
        var record = updateRecordRequest.Adapt<Record>();
        Record updatedRecord = await recordService.UpdateRecordAsync(record, updateRecordRequest.TagIds, recordId, userId, updateRecordRequest.Installments);
        var recordResponse = updatedRecord.Adapt<RecordResponse>();
        return Ok(recordResponse);
    }

    [HttpDelete("{recordId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteRecord([FromRoute] string recordId)
    {
        var userId = User.GetUserId();
        await recordService.DeleteRecordAsync(recordId, userId);
        return NoContent();
    }

    [HttpPost("export")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> ExportRecords([FromBody] ExportRecordsRequest exportRecordsRequest)
    {
        var userId = User.GetUserId();
        await exportService.RequestExportAsync(userId, exportRecordsRequest.StartDate, exportRecordsRequest.EndDate);
        return Accepted();
    }

    [HttpGet("export/files")]
    [ProducesResponseType(typeof(List<ExportFileResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ExportFileResponse>>> ListExportFiles()
    {
        var userId = User.GetUserId();
        var exports = await exportService.ListExportsAsync(userId);
        var response = exports.Adapt<List<ExportFileResponse>>();
        return Ok(response);
    }

    [HttpGet("export/files/{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DownloadExportFile([FromRoute] string fileName)
    {
        var userId = User.GetUserId();
        var stream = await exportService.DownloadExportAsync(userId, fileName);

        if (stream is null)
            return NotFound();

        return File(stream, "application/pdf", fileName);
    }
}
