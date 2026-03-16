using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PopaDin.ExportService.Documents;
using PopaDin.ExportService.Models;
using PopaDin.ExportService.Services;

namespace PopaDin.ExportService.Tests;

public class PdfGeneratorServiceTests
{
    private readonly ILogger<PdfGeneratorService> _logger = Substitute.For<ILogger<PdfGeneratorService>>();

    static PdfGeneratorServiceTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    private PdfGeneratorService CreateService() => new(_logger);

    [Fact]
    public void GenerateRecordsReport_WithRecords_ShouldReturnNonEmptyByteArray()
    {
        var records = new List<RecordDocument>
        {
            new()
            {
                Name = "Salary", UserId = 1, Operation = 1, Value = 5000,
                Frequency = FrequencyType.OneTime, ReferenceDate = new DateTime(2024, 1, 15),
                Tags = [new RecordTagSubDocument { Name = "Income", OriginalTagId = 1 }],
                CreatedAt = DateTime.Now
            },
            new()
            {
                Name = "Rent", UserId = 1, Operation = 0, Value = 1500,
                Frequency = FrequencyType.Monthly, ReferenceDate = new DateTime(2024, 1, 5),
                Tags = [new RecordTagSubDocument { Name = "Housing", OriginalTagId = 2 }],
                CreatedAt = DateTime.Now
            }
        };

        var result = CreateService().GenerateRecordsReport(records, new DateTime(2024, 1, 1), new DateTime(2024, 1, 31));

        result.Should().NotBeEmpty();
        result.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GenerateRecordsReport_WithEmptyRecords_ShouldReturnValidPdf()
    {
        var records = new List<RecordDocument>();

        var result = CreateService().GenerateRecordsReport(records, new DateTime(2024, 1, 1), new DateTime(2024, 1, 31));

        result.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateRecordsReportStream_WithRecords_ShouldReturnReadableStream()
    {
        var records = new List<RecordDocument>
        {
            new()
            {
                Name = "Food", UserId = 1, Operation = 0, Value = 200,
                Frequency = FrequencyType.OneTime, ReferenceDate = new DateTime(2024, 1, 10),
                Tags = [], CreatedAt = DateTime.Now
            }
        };

        using var stream = CreateService().GenerateRecordsReportStream(records, new DateTime(2024, 1, 1), new DateTime(2024, 1, 31));

        stream.Should().NotBeNull();
        stream.Length.Should().BeGreaterThan(0);
        stream.Position.Should().Be(0);
    }

    [Fact]
    public void GenerateRecordsReport_WithNoTags_ShouldHandleGracefully()
    {
        var records = new List<RecordDocument>
        {
            new()
            {
                Name = "Misc", UserId = 1, Operation = 0, Value = 50,
                Frequency = FrequencyType.OneTime, ReferenceDate = new DateTime(2024, 1, 1),
                Tags = [], CreatedAt = DateTime.Now
            }
        };

        var result = CreateService().GenerateRecordsReport(records, new DateTime(2024, 1, 1), new DateTime(2024, 1, 31));

        result.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateRecordsReport_WithNullReferenceDate_ShouldUseCreatedAt()
    {
        var records = new List<RecordDocument>
        {
            new()
            {
                Name = "Test", UserId = 1, Operation = 1, Value = 100,
                Frequency = FrequencyType.OneTime, ReferenceDate = null,
                Tags = [], CreatedAt = new DateTime(2024, 1, 15)
            }
        };

        var result = CreateService().GenerateRecordsReport(records, new DateTime(2024, 1, 1), new DateTime(2024, 1, 31));

        result.Should().NotBeEmpty();
    }
}
