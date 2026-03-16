using FluentAssertions;
using NSubstitute;
using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Service;

namespace PopaDin.Bkd.Service.Tests;

public class InstallmentServiceTests
{
    private readonly IRecordRepository _recordRepository = Substitute.For<IRecordRepository>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly InstallmentService _sut;

    public InstallmentServiceTests()
    {
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
        _sut = new InstallmentService(_recordRepository, _timeProvider);
    }

    [Fact]
    public async Task CreateInstallmentRecordsAsync_ShouldCreateCorrectNumberOfRecords()
    {
        var baseRecord = new Record
        {
            Name = "Laptop",
            Operation = OperationEnum.Outflow,
            Value = 3000,
            Frequency = FrequencyEnum.Monthly,
            ReferenceDate = new DateTime(2024, 7, 1),
            Tags = new List<Tag> { new() { Id = 1, Name = "Tech" } },
            User = new User { Id = 1 }
        };

        _recordRepository.CreateManyRecordsAsync(Arg.Any<List<Record>>())
            .Returns(callInfo => callInfo.Arg<List<Record>>());

        var result = await _sut.CreateInstallmentRecordsAsync(baseRecord, 3);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task CreateInstallmentRecordsAsync_ShouldDivideValueEvenly()
    {
        var baseRecord = new Record
        {
            Name = "TV",
            Operation = OperationEnum.Outflow,
            Value = 3000,
            Frequency = FrequencyEnum.Monthly,
            ReferenceDate = new DateTime(2024, 7, 1),
            Tags = new List<Tag>(),
            User = new User { Id = 1 }
        };

        _recordRepository.CreateManyRecordsAsync(Arg.Any<List<Record>>())
            .Returns(callInfo => callInfo.Arg<List<Record>>());

        var result = await _sut.CreateInstallmentRecordsAsync(baseRecord, 3);

        result.Should().AllSatisfy(r => r.Value.Should().Be(1000));
    }

    [Fact]
    public async Task CreateInstallmentRecordsAsync_ShouldSetIncrementalReferenceDates()
    {
        var baseDate = new DateTime(2024, 7, 1);
        var baseRecord = new Record
        {
            Name = "Phone",
            Operation = OperationEnum.Outflow,
            Value = 2400,
            Frequency = FrequencyEnum.Monthly,
            ReferenceDate = baseDate,
            Tags = new List<Tag>(),
            User = new User { Id = 1 }
        };

        _recordRepository.CreateManyRecordsAsync(Arg.Any<List<Record>>())
            .Returns(callInfo => callInfo.Arg<List<Record>>());

        var result = await _sut.CreateInstallmentRecordsAsync(baseRecord, 3);

        result[0].ReferenceDate.Should().Be(baseDate);
        result[1].ReferenceDate.Should().Be(baseDate.AddMonths(1));
        result[2].ReferenceDate.Should().Be(baseDate.AddMonths(2));
    }

    [Fact]
    public async Task CreateInstallmentRecordsAsync_ShouldSetInstallmentMetadata()
    {
        var baseRecord = new Record
        {
            Name = "Sofa",
            Operation = OperationEnum.Outflow,
            Value = 6000,
            Frequency = FrequencyEnum.Monthly,
            ReferenceDate = new DateTime(2024, 7, 1),
            Tags = new List<Tag>(),
            User = new User { Id = 1 }
        };

        _recordRepository.CreateManyRecordsAsync(Arg.Any<List<Record>>())
            .Returns(callInfo => callInfo.Arg<List<Record>>());

        var result = await _sut.CreateInstallmentRecordsAsync(baseRecord, 4);

        result.Should().AllSatisfy(r =>
        {
            r.InstallmentGroupId.Should().NotBeNullOrEmpty();
            r.InstallmentTotal.Should().Be(4);
        });

        result[0].InstallmentIndex.Should().Be(1);
        result[1].InstallmentIndex.Should().Be(2);
        result[2].InstallmentIndex.Should().Be(3);
        result[3].InstallmentIndex.Should().Be(4);

        result.Select(r => r.InstallmentGroupId).Distinct().Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateInstallmentRecordsAsync_WithoutReferenceDate_ShouldUseCurrentTime()
    {
        var baseRecord = new Record
        {
            Name = "Item",
            Operation = OperationEnum.Outflow,
            Value = 1200,
            Frequency = FrequencyEnum.Monthly,
            ReferenceDate = null,
            Tags = new List<Tag>(),
            User = new User { Id = 1 }
        };

        _recordRepository.CreateManyRecordsAsync(Arg.Any<List<Record>>())
            .Returns(callInfo => callInfo.Arg<List<Record>>());

        var result = await _sut.CreateInstallmentRecordsAsync(baseRecord, 2);

        result[0].ReferenceDate.Should().Be(new DateTime(2024, 6, 15, 12, 0, 0));
        result[1].ReferenceDate.Should().Be(new DateTime(2024, 7, 15, 12, 0, 0));
    }

    [Fact]
    public async Task CreateInstallmentRecordsAsync_ShouldRoundValues()
    {
        var baseRecord = new Record
        {
            Name = "Item",
            Operation = OperationEnum.Outflow,
            Value = 100,
            Frequency = FrequencyEnum.Monthly,
            ReferenceDate = new DateTime(2024, 7, 1),
            Tags = new List<Tag>(),
            User = new User { Id = 1 }
        };

        _recordRepository.CreateManyRecordsAsync(Arg.Any<List<Record>>())
            .Returns(callInfo => callInfo.Arg<List<Record>>());

        var result = await _sut.CreateInstallmentRecordsAsync(baseRecord, 3);

        result.Should().AllSatisfy(r => r.Value.Should().Be(33.33m));
    }
}
