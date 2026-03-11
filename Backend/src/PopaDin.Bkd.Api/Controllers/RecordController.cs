using Microsoft.AspNetCore.Mvc;
using PopaDin.Bkd.Api.Dtos.Record;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Interfaces.Publishers;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace PopaDin.Bkd.Api.Controllers;

[Route("v1/[controller]")]
[ApiController]
[Authorize]
public class RecordController(
    IRecordService recordService,
    IExportEventPublisher exportEventPublisher,
    IExportBlobRepository exportBlobRepository) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(RecordResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecordResponse>> CreateRecord([FromBody] CreateRecordRequest createRecordRequest)
    {
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
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
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
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
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
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
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
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
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        await recordService.DeleteRecordAsync(recordId, userId);
        return NoContent();
    }

    [HttpPost("export")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ExportRecords([FromBody] ExportRecordsRequest exportRecordsRequest)
    {
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        await exportEventPublisher.PublishExportRequestAsync(userId, exportRecordsRequest.StartDate, exportRecordsRequest.EndDate);
        return Accepted();
    }

    [HttpGet("export/files")]
    [ProducesResponseType(typeof(List<ExportFileResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ExportFileResponse>>> ListExportFiles()
    {
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var exports = await exportBlobRepository.ListExportsAsync(userId);
        var response = exports.Adapt<List<ExportFileResponse>>();
        return Ok(response);
    }

    [HttpGet("export/files/{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DownloadExportFile([FromRoute] string fileName)
    {
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var stream = await exportBlobRepository.DownloadExportAsync(userId, fileName);

        if (stream is null)
            return NotFound();

        return File(stream, "application/pdf", fileName);
    }
}
