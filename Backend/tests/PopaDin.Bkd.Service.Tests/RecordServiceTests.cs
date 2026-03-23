using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Publishers;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Service;

namespace PopaDin.Bkd.Service.Tests;

public class RecordServiceTests
{
    private readonly IRecordRepository _recordRepository = Substitute.For<IRecordRepository>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly ITagCacheRepository _tagCacheRepository = Substitute.For<ITagCacheRepository>();
    private readonly IDashboardCacheRepository _dashboardCacheRepository = Substitute.For<IDashboardCacheRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IBalanceService _balanceService = Substitute.For<IBalanceService>();
    private readonly IInstallmentService _installmentService = Substitute.For<IInstallmentService>();
    private readonly IRecordEventPublisher _recordEventPublisher = Substitute.For<IRecordEventPublisher>();
    private readonly INotificationEventPublisher _notificationEventPublisher = Substitute.For<INotificationEventPublisher>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly ILogger<RecordService> _logger = Substitute.For<ILogger<RecordService>>();
    private readonly RecordService _sut;

    private const int UserId = 1;

    public RecordServiceTests()
    {
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
        _sut = new RecordService(
            _recordRepository, _tagRepository, _tagCacheRepository, _dashboardCacheRepository,
            _userRepository, _balanceService, _installmentService,
            _recordEventPublisher, _notificationEventPublisher, _timeProvider, _logger);
    }

    private void SetupDefaultTagCache(int userId = UserId)
    {
        _tagCacheRepository.GetUserTagsAsync(userId).Returns(new List<Tag>
        {
            new() { Id = 1, Name = "Tag1" },
            new() { Id = 2, Name = "Tag2" }
        });
    }

    private void SetupDefaultUser(int userId = UserId)
    {
        _userRepository.FindUserByIdAsync(userId).Returns(new User { Id = userId, Balance = 1000, Email = "test@test.com" });
    }

    #region CreateRecordAsync

    [Fact]
    public async Task CreateRecordAsync_WithValidRecord_ShouldCreateAndUpdateBalance()
    {
        SetupDefaultTagCache();
        SetupDefaultUser();
        var record = new Record { Name = "Salary", Operation = OperationEnum.Deposit, Value = 5000, Frequency = FrequencyEnum.OneTime };
        var createdRecord = new Record { Id = "abc", Name = "Salary", Operation = OperationEnum.Deposit, Value = 5000 };

        _recordRepository.CreateRecordAsync(Arg.Any<Record>()).Returns(createdRecord);
        _recordRepository.GetNonRecurringByPeriodAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(new List<Record>());
        _recordRepository.GetRecurringRecordsAsync(UserId).Returns(new List<Record>());

        var result = await _sut.CreateRecordAsync(record, [1], UserId);

        result.Should().Be(createdRecord);
        await _balanceService.Received(1).UpdateBalanceForNewRecordAsync(UserId, createdRecord);
        await _dashboardCacheRepository.Received(1).InvalidateAsync(UserId);
    }

    [Fact]
    public async Task CreateRecordAsync_WithInstallments_ShouldUseInstallmentService()
    {
        SetupDefaultTagCache();
        SetupDefaultUser();
        var record = new Record { Name = "Laptop", Operation = OperationEnum.Outflow, Value = 3000, Frequency = FrequencyEnum.Monthly };
        var installmentRecords = new List<Record>
        {
            new() { Id = "1", Value = 1000, Operation = OperationEnum.Outflow },
            new() { Id = "2", Value = 1000, Operation = OperationEnum.Outflow },
            new() { Id = "3", Value = 1000, Operation = OperationEnum.Outflow }
        };

        _installmentService.CreateInstallmentRecordsAsync(Arg.Any<Record>(), 3).Returns(installmentRecords);
        _recordRepository.GetNonRecurringByPeriodAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(new List<Record>());
        _recordRepository.GetRecurringRecordsAsync(UserId).Returns(new List<Record>());

        var result = await _sut.CreateRecordAsync(record, [1], UserId, 3);

        result.Id.Should().Be("1");
        await _balanceService.Received(1).UpdateBalanceForNewRecordsAsync(UserId, installmentRecords);
    }

    [Fact]
    public async Task CreateRecordAsync_WithInvalidValue_ShouldThrowException()
    {
        var record = new Record { Name = "Test", Operation = OperationEnum.Deposit, Value = 0 };

        var act = () => _sut.CreateRecordAsync(record, [], UserId);

        await act.Should().ThrowAsync<UnprocessableEntityException>();
    }

    [Fact]
    public async Task CreateRecordAsync_WithInvalidTagIds_ShouldThrowNotFoundException()
    {
        _tagCacheRepository.GetUserTagsAsync(UserId).Returns(new List<Tag>
        {
            new() { Id = 1, Name = "Tag1" }
        });

        var record = new Record { Name = "Test", Operation = OperationEnum.Deposit, Value = 100, Frequency = FrequencyEnum.OneTime };

        var act = () => _sut.CreateRecordAsync(record, [1, 999], UserId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateRecordAsync_WithEmptyTags_ShouldSucceed()
    {
        SetupDefaultUser();
        var record = new Record { Name = "Test", Operation = OperationEnum.Deposit, Value = 100, Frequency = FrequencyEnum.OneTime };
        var createdRecord = new Record { Id = "abc", Name = "Test", Value = 100 };

        _recordRepository.CreateRecordAsync(Arg.Any<Record>()).Returns(createdRecord);
        _recordRepository.GetNonRecurringByPeriodAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(new List<Record>());
        _recordRepository.GetRecurringRecordsAsync(UserId).Returns(new List<Record>());

        var result = await _sut.CreateRecordAsync(record, [], UserId);

        result.Should().Be(createdRecord);
    }

    [Fact]
    public async Task CreateRecordAsync_WhenBalanceUpdateFails_ShouldRollbackRecord()
    {
        SetupDefaultUser();
        var record = new Record { Name = "Test", Operation = OperationEnum.Deposit, Value = 100, Frequency = FrequencyEnum.OneTime };
        var createdRecord = new Record { Id = "abc", Name = "Test", Value = 100 };

        _recordRepository.CreateRecordAsync(Arg.Any<Record>()).Returns(createdRecord);
        _balanceService.UpdateBalanceForNewRecordAsync(UserId, createdRecord).Throws(new Exception("Balance error"));

        var act = () => _sut.CreateRecordAsync(record, [], UserId);

        await act.Should().ThrowAsync<Exception>().WithMessage("Balance error");
        await _recordRepository.Received(1).DeleteRecordAsync("abc");
    }

    [Fact]
    public async Task CreateRecordAsync_ShouldPublishEvents()
    {
        SetupDefaultTagCache();
        SetupDefaultUser();
        var record = new Record { Name = "Salary", Operation = OperationEnum.Deposit, Value = 5000, Frequency = FrequencyEnum.OneTime };
        var createdRecord = new Record { Id = "abc", Name = "Salary", Operation = OperationEnum.Deposit, Value = 5000 };

        _recordRepository.CreateRecordAsync(Arg.Any<Record>()).Returns(createdRecord);
        _recordRepository.GetNonRecurringByPeriodAsync(UserId, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(new List<Record>());
        _recordRepository.GetRecurringRecordsAsync(UserId).Returns(new List<Record>());

        await _sut.CreateRecordAsync(record, [1], UserId);

        await _recordEventPublisher.Received(1).PublishRecordCreatedAsync(
            UserId, Arg.Any<decimal>(), Arg.Any<OperationEnum>(), Arg.Any<decimal>(), Arg.Any<decimal>());
        await _notificationEventPublisher.Received(1).PublishAsync(
            UserId, "RECORD_CREATED", Arg.Any<string>(), Arg.Any<string>(), Arg.Any<object>());
    }

    #endregion

    #region GetRecordsAsync

    [Fact]
    public async Task GetRecordsAsync_ShouldSetUserIdAndReturn()
    {
        var listRecords = new ListRecords { Page = 1, ItemsPerPage = 20 };
        var expected = new PaginatedResult<Record> { Lines = [new Record { Id = "1" }], Page = 1, TotalItems = 1 };
        _recordRepository.GetRecordsAsync(Arg.Any<ListRecords>()).Returns(expected);

        var result = await _sut.GetRecordsAsync(listRecords, UserId);

        result.Should().Be(expected);
        listRecords.UserId.Should().Be(UserId);
    }

    #endregion

    #region FindRecordByIdAsync

    [Fact]
    public async Task FindRecordByIdAsync_WhenExists_ShouldReturn()
    {
        var record = new Record { Id = "abc", Name = "Test" };
        _recordRepository.FindRecordByIdAsync("abc", UserId).Returns(record);

        var result = await _sut.FindRecordByIdAsync("abc", UserId);

        result.Should().Be(record);
    }

    [Fact]
    public async Task FindRecordByIdAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _recordRepository.FindRecordByIdAsync("invalid", UserId).Returns((Record?)null);

        var act = () => _sut.FindRecordByIdAsync("invalid", UserId);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Record não encontrado");
    }

    #endregion

    #region DeleteRecordAsync

    [Fact]
    public async Task DeleteRecordAsync_WithSingleRecord_ShouldDeleteAndRevertBalance()
    {
        var record = new Record { Id = "abc", Operation = OperationEnum.Deposit, Value = 100 };
        _recordRepository.FindRecordByIdAsync("abc", UserId).Returns(record);

        await _sut.DeleteRecordAsync("abc", UserId);

        await _recordRepository.Received(1).DeleteRecordAsync("abc");
        await _balanceService.Received(1).RevertBalanceForRecordAsync(UserId, record);
        await _dashboardCacheRepository.Received(1).InvalidateAsync(UserId);
    }

    [Fact]
    public async Task DeleteRecordAsync_WithInstallmentGroup_ShouldDeleteGroupAndRevertBalance()
    {
        var record = new Record { Id = "abc", InstallmentGroupId = "group1" };
        var groupRecords = new List<Record>
        {
            new() { Id = "1", Value = 100 },
            new() { Id = "2", Value = 100 },
            new() { Id = "3", Value = 100 }
        };

        _recordRepository.FindRecordByIdAsync("abc", UserId).Returns(record);
        _recordRepository.FindByInstallmentGroupAsync("group1", UserId).Returns(groupRecords);

        await _sut.DeleteRecordAsync("abc", UserId);

        await _recordRepository.Received(1).DeleteManyByInstallmentGroupAsync("group1", UserId);
        await _balanceService.Received(1).RevertBalanceForRecordsAsync(UserId, groupRecords);
        await _dashboardCacheRepository.Received(1).InvalidateAsync(UserId);
    }

    [Fact]
    public async Task DeleteRecordAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _recordRepository.FindRecordByIdAsync("invalid", UserId).Returns((Record?)null);

        var act = () => _sut.DeleteRecordAsync("invalid", UserId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region UpdateRecordAsync

    [Fact]
    public async Task UpdateRecordAsync_SimpleRecord_ShouldUpdateAndRebalance()
    {
        SetupDefaultTagCache();
        var existingRecord = new Record { Id = "abc", Name = "Old", Operation = OperationEnum.Deposit, Value = 100, Frequency = FrequencyEnum.OneTime };
        var updateRequest = new Record { Name = "New", Operation = OperationEnum.Outflow, Value = 200, Frequency = FrequencyEnum.Monthly };
        var updatedRecord = new Record { Id = "abc", Name = "New", Operation = OperationEnum.Outflow, Value = 200 };

        _recordRepository.FindRecordByIdAsync("abc", UserId).Returns(existingRecord);

        var result = await _sut.UpdateRecordAsync(updateRequest, [1], "abc", UserId);

        await _balanceService.Received(1).RevertBalanceForRecordAsync(UserId, existingRecord);
        await _balanceService.Received(1).UpdateBalanceForNewRecordAsync(UserId, existingRecord);
        await _dashboardCacheRepository.Received(1).InvalidateAsync(UserId);
    }

    [Fact]
    public async Task UpdateRecordAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _recordRepository.FindRecordByIdAsync("invalid", UserId).Returns((Record?)null);

        var act = () => _sut.UpdateRecordAsync(new Record(), [], "invalid", UserId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateRecordAsync_InstallmentToInstallment_ShouldDeleteOldGroupAndCreateNew()
    {
        SetupDefaultTagCache();
        var existingRecord = new Record { Id = "abc", InstallmentGroupId = "group1", Value = 100 };
        var oldGroupRecords = new List<Record> { new() { Id = "1", Value = 100 }, new() { Id = "2", Value = 100 } };
        var newRecords = new List<Record> { new() { Id = "n1", Value = 150 }, new() { Id = "n2", Value = 150 }, new() { Id = "n3", Value = 150 } };

        _recordRepository.FindRecordByIdAsync("abc", UserId).Returns(existingRecord);
        _recordRepository.FindByInstallmentGroupAsync("group1", UserId).Returns(oldGroupRecords);
        _installmentService.CreateInstallmentRecordsAsync(Arg.Any<Record>(), 3).Returns(newRecords);

        var result = await _sut.UpdateRecordAsync(new Record { Name = "New", Value = 450 }, [1], "abc", UserId, 3);

        await _recordRepository.Received(1).DeleteManyByInstallmentGroupAsync("group1", UserId);
        await _balanceService.Received(1).RevertBalanceForRecordsAsync(UserId, oldGroupRecords);
        await _balanceService.Received(1).UpdateBalanceForNewRecordsAsync(UserId, newRecords);
        result.Id.Should().Be("n1");
    }

    [Fact]
    public async Task UpdateRecordAsync_SimpleToInstallment_ShouldDeleteOldAndCreateInstallments()
    {
        SetupDefaultTagCache();
        var existingRecord = new Record { Id = "abc", Value = 300, Operation = OperationEnum.Outflow };
        var newRecords = new List<Record> { new() { Id = "n1", Value = 100 }, new() { Id = "n2", Value = 100 }, new() { Id = "n3", Value = 100 } };

        _recordRepository.FindRecordByIdAsync("abc", UserId).Returns(existingRecord);
        _installmentService.CreateInstallmentRecordsAsync(Arg.Any<Record>(), 3).Returns(newRecords);

        var result = await _sut.UpdateRecordAsync(new Record { Name = "New", Value = 300 }, [1], "abc", UserId, 3);

        await _balanceService.Received(1).RevertBalanceForRecordAsync(UserId, existingRecord);
        await _recordRepository.Received(1).DeleteRecordAsync("abc");
        await _balanceService.Received(1).UpdateBalanceForNewRecordsAsync(UserId, newRecords);
        result.Id.Should().Be("n1");
    }

    #endregion
}
