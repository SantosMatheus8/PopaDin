using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Publishers;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Service;

namespace PopaDin.Bkd.Service.Tests;

public class ExportServiceTests
{
    private readonly IExportEventPublisher _exportEventPublisher = Substitute.For<IExportEventPublisher>();
    private readonly IExportBlobRepository _exportBlobRepository = Substitute.For<IExportBlobRepository>();
    private readonly ILogger<ExportService> _logger = Substitute.For<ILogger<ExportService>>();
    private readonly ExportService _sut;

    public ExportServiceTests()
    {
        _sut = new ExportService(_exportEventPublisher, _exportBlobRepository, _logger);
    }

    #region RequestExportAsync

    [Fact]
    public async Task RequestExportAsync_WithValidDates_ShouldPublishEvent()
    {
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 6, 1);

        await _sut.RequestExportAsync(1, start, end);

        await _exportEventPublisher.Received(1).PublishExportRequestAsync(1, start, end);
    }

    [Fact]
    public async Task RequestExportAsync_WhenStartDateAfterEndDate_ShouldThrowUnprocessableEntityException()
    {
        var start = new DateTime(2024, 6, 1);
        var end = new DateTime(2024, 1, 1);

        var act = () => _sut.RequestExportAsync(1, start, end);

        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .WithMessage("A data inicial deve ser anterior à data final.");
    }

    [Fact]
    public async Task RequestExportAsync_WhenStartDateEqualsEndDate_ShouldThrowUnprocessableEntityException()
    {
        var date = new DateTime(2024, 1, 1);

        var act = () => _sut.RequestExportAsync(1, date, date);

        await act.Should().ThrowAsync<UnprocessableEntityException>();
    }

    [Fact]
    public async Task RequestExportAsync_WhenPeriodExceeds365Days_ShouldThrowUnprocessableEntityException()
    {
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2025, 2, 1);

        var act = () => _sut.RequestExportAsync(1, start, end);

        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .WithMessage("O período máximo de exportação é de 365 dias.");
    }

    [Fact]
    public async Task RequestExportAsync_WithExactly365Days_ShouldSucceed()
    {
        var start = new DateTime(2024, 1, 1);
        var end = start.AddDays(365);

        await _sut.RequestExportAsync(1, start, end);

        await _exportEventPublisher.Received(1).PublishExportRequestAsync(1, start, end);
    }

    #endregion

    #region ListExportsAsync

    [Fact]
    public async Task ListExportsAsync_ShouldReturnExports()
    {
        var exports = new List<ExportFile>
        {
            new() { Name = "export1.csv", Size = 1024 },
            new() { Name = "export2.csv", Size = 2048 }
        };
        _exportBlobRepository.ListExportsAsync(1).Returns(exports);

        var result = await _sut.ListExportsAsync(1);

        result.Should().HaveCount(2);
    }

    #endregion

    #region DownloadExportAsync

    [Fact]
    public async Task DownloadExportAsync_ShouldReturnStream()
    {
        var stream = new MemoryStream();
        _exportBlobRepository.DownloadExportAsync(1, "file.csv").Returns(stream);

        var result = await _sut.DownloadExportAsync(1, "file.csv");

        result.Should().BeSameAs(stream);
    }

    [Fact]
    public async Task DownloadExportAsync_WhenNotFound_ShouldReturnNull()
    {
        _exportBlobRepository.DownloadExportAsync(1, "missing.csv").Returns((Stream?)null);

        var result = await _sut.DownloadExportAsync(1, "missing.csv");

        result.Should().BeNull();
    }

    #endregion
}
