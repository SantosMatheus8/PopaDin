using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PopaDin.Bkd.Api.Controllers;
using PopaDin.Bkd.Api.Dtos.Record;
using PopaDin.Bkd.Api.Tests.Helpers;
using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Api.Tests.Controllers;

public class RecordControllerTests
{
    private readonly IRecordService _recordService = Substitute.For<IRecordService>();
    private readonly IExportService _exportService = Substitute.For<IExportService>();
    private readonly RecordController _sut;

    private const int AuthUserId = 1;

    public RecordControllerTests()
    {
        _sut = new RecordController(_recordService, _exportService);
        ControllerTestHelper.SetupAuthenticatedUser(_sut, AuthUserId);
    }

    #region CreateRecord

    [Fact]
    public async Task CreateRecord_WithValidRequest_ShouldReturn201Created()
    {
        var request = new CreateRecordRequest
        {
            Name = "Salary",
            Operation = OperationEnum.Deposit,
            Value = 5000,
            Frequency = FrequencyEnum.Monthly,
            TagIds = [1]
        };
        _recordService.CreateRecordAsync(Arg.Any<Record>(), Arg.Any<List<int>>(), AuthUserId, Arg.Any<int?>())
            .Returns(new Record { Id = "abc", Name = "Salary", Value = 5000 });

        var result = await _sut.CreateRecord(request);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task CreateRecord_WithInvalidValue_ShouldThrowException()
    {
        var request = new CreateRecordRequest { Name = "Test", Value = 0 };
        _recordService.CreateRecordAsync(Arg.Any<Record>(), Arg.Any<List<int>>(), AuthUserId, Arg.Any<int?>())
            .Throws(new UnprocessableEntityException("Value inválido"));

        var act = () => _sut.CreateRecord(request);

        await act.Should().ThrowAsync<UnprocessableEntityException>();
    }

    #endregion

    #region GetRecords

    [Fact]
    public async Task GetRecords_ShouldReturnOkWithPaginatedResult()
    {
        var request = new ListRecordsRequest();
        _recordService.GetRecordsAsync(Arg.Any<ListRecords>(), AuthUserId)
            .Returns(new PaginatedResult<Record> { Lines = [new Record { Id = "1" }], Page = 1, TotalItems = 1 });

        var result = await _sut.GetRecords(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    #endregion

    #region FindRecordById

    [Fact]
    public async Task FindRecordById_WhenExists_ShouldReturnOk()
    {
        _recordService.FindRecordByIdAsync("abc", AuthUserId)
            .Returns(new Record { Id = "abc", Name = "Test" });

        var result = await _sut.FindRecordById("abc");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task FindRecordById_WhenNotFound_ShouldThrowNotFoundException()
    {
        _recordService.FindRecordByIdAsync("invalid", AuthUserId)
            .Throws(new NotFoundException("Record não encontrado"));

        var act = () => _sut.FindRecordById("invalid");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region UpdateRecord

    [Fact]
    public async Task UpdateRecord_WithValidRequest_ShouldReturnOk()
    {
        var request = new UpdateRecordRequest { Name = "Updated", Value = 100, TagIds = [1] };
        _recordService.UpdateRecordAsync(Arg.Any<Record>(), Arg.Any<List<int>>(), "abc", AuthUserId, Arg.Any<int?>())
            .Returns(new Record { Id = "abc", Name = "Updated" });

        var result = await _sut.UpdateRecord(request, "abc");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateRecord_WhenNotFound_ShouldThrowNotFoundException()
    {
        var request = new UpdateRecordRequest { Name = "Updated", TagIds = [] };
        _recordService.UpdateRecordAsync(Arg.Any<Record>(), Arg.Any<List<int>>(), "invalid", AuthUserId, Arg.Any<int?>())
            .Throws(new NotFoundException("Record não encontrado"));

        var act = () => _sut.UpdateRecord(request, "invalid");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeleteRecord

    [Fact]
    public async Task DeleteRecord_WhenExists_ShouldReturn204NoContent()
    {
        var result = await _sut.DeleteRecord("abc");

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteRecord_WhenNotFound_ShouldThrowNotFoundException()
    {
        _recordService.DeleteRecordAsync("invalid", AuthUserId)
            .Throws(new NotFoundException("Record não encontrado"));

        var act = () => _sut.DeleteRecord("invalid");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region ExportRecords

    [Fact]
    public async Task ExportRecords_WithValidDates_ShouldReturn202Accepted()
    {
        var request = new ExportRecordsRequest { StartDate = new DateTime(2024, 1, 1), EndDate = new DateTime(2024, 6, 1) };

        var result = await _sut.ExportRecords(request);

        result.Should().BeOfType<AcceptedResult>();
    }

    [Fact]
    public async Task ExportRecords_WithInvalidDates_ShouldThrowUnprocessableEntityException()
    {
        var request = new ExportRecordsRequest { StartDate = new DateTime(2024, 6, 1), EndDate = new DateTime(2024, 1, 1) };
        _exportService.RequestExportAsync(AuthUserId, request.StartDate, request.EndDate)
            .Throws(new UnprocessableEntityException("A data inicial deve ser anterior à data final."));

        var act = () => _sut.ExportRecords(request);

        await act.Should().ThrowAsync<UnprocessableEntityException>();
    }

    #endregion

    #region ListExportFiles

    [Fact]
    public async Task ListExportFiles_ShouldReturnOk()
    {
        _exportService.ListExportsAsync(AuthUserId)
            .Returns(new List<ExportFile> { new() { Name = "export.csv" } });

        var result = await _sut.ListExportFiles();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    #endregion

    #region DownloadExportFile

    [Fact]
    public async Task DownloadExportFile_WhenExists_ShouldReturnFile()
    {
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        _exportService.DownloadExportAsync(AuthUserId, "file.pdf").Returns(stream);

        var result = await _sut.DownloadExportFile("file.pdf");

        result.Should().BeOfType<FileStreamResult>();
    }

    [Fact]
    public async Task DownloadExportFile_WhenNotFound_ShouldReturn404()
    {
        _exportService.DownloadExportAsync(AuthUserId, "missing.pdf").Returns((Stream?)null);

        var result = await _sut.DownloadExportFile("missing.pdf");

        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion
}
