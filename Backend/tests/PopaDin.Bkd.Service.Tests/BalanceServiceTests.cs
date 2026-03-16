using FluentAssertions;
using NSubstitute;
using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Service;

namespace PopaDin.Bkd.Service.Tests;

public class BalanceServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly BalanceService _sut;

    public BalanceServiceTests()
    {
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero));
        _sut = new BalanceService(_userRepository, _timeProvider);
    }

    #region UpdateBalanceForNewRecordAsync

    [Fact]
    public async Task UpdateBalanceForNewRecordAsync_WithDeposit_ShouldAddToBalance()
    {
        var record = new Record
        {
            Operation = OperationEnum.Deposit,
            Value = 100,
            Frequency = FrequencyEnum.OneTime,
            ReferenceDate = new DateTime(2024, 6, 1)
        };

        await _sut.UpdateBalanceForNewRecordAsync(1, record);

        await _userRepository.Received(1).UpdateBalanceAsync(1, 100);
    }

    [Fact]
    public async Task UpdateBalanceForNewRecordAsync_WithOutflow_ShouldSubtractFromBalance()
    {
        var record = new Record
        {
            Operation = OperationEnum.Outflow,
            Value = 50,
            Frequency = FrequencyEnum.OneTime,
            ReferenceDate = new DateTime(2024, 6, 1)
        };

        await _sut.UpdateBalanceForNewRecordAsync(1, record);

        await _userRepository.Received(1).UpdateBalanceAsync(1, -50);
    }

    [Fact]
    public async Task UpdateBalanceForNewRecordAsync_WithFutureDate_ShouldNotUpdateBalance()
    {
        var record = new Record
        {
            Operation = OperationEnum.Deposit,
            Value = 100,
            Frequency = FrequencyEnum.OneTime,
            ReferenceDate = new DateTime(2025, 1, 1)
        };

        await _sut.UpdateBalanceForNewRecordAsync(1, record);

        await _userRepository.DidNotReceive().UpdateBalanceAsync(Arg.Any<int>(), Arg.Any<decimal>());
    }

    #endregion

    #region UpdateBalanceForNewRecordsAsync

    [Fact]
    public async Task UpdateBalanceForNewRecordsAsync_ShouldSumImpactOfPastRecords()
    {
        var records = new List<Record>
        {
            new() { Operation = OperationEnum.Deposit, Value = 200, Frequency = FrequencyEnum.OneTime, ReferenceDate = new DateTime(2024, 6, 1) },
            new() { Operation = OperationEnum.Outflow, Value = 50, Frequency = FrequencyEnum.OneTime, ReferenceDate = new DateTime(2024, 6, 10) },
            new() { Operation = OperationEnum.Deposit, Value = 1000, Frequency = FrequencyEnum.OneTime, ReferenceDate = new DateTime(2025, 1, 1) }
        };

        await _sut.UpdateBalanceForNewRecordsAsync(1, records);

        await _userRepository.Received(1).UpdateBalanceAsync(1, 150);
    }

    [Fact]
    public async Task UpdateBalanceForNewRecordsAsync_WithAllFuture_ShouldNotUpdate()
    {
        var records = new List<Record>
        {
            new() { Operation = OperationEnum.Deposit, Value = 100, Frequency = FrequencyEnum.OneTime, ReferenceDate = new DateTime(2025, 1, 1) }
        };

        await _sut.UpdateBalanceForNewRecordsAsync(1, records);

        await _userRepository.DidNotReceive().UpdateBalanceAsync(Arg.Any<int>(), Arg.Any<decimal>());
    }

    #endregion

    #region RevertBalanceForRecordAsync

    [Fact]
    public async Task RevertBalanceForRecordAsync_WithDeposit_ShouldSubtractFromBalance()
    {
        var record = new Record
        {
            Operation = OperationEnum.Deposit,
            Value = 100,
            Frequency = FrequencyEnum.OneTime,
            ReferenceDate = new DateTime(2024, 6, 1)
        };

        await _sut.RevertBalanceForRecordAsync(1, record);

        await _userRepository.Received(1).UpdateBalanceAsync(1, -100);
    }

    [Fact]
    public async Task RevertBalanceForRecordAsync_WithOutflow_ShouldAddBackToBalance()
    {
        var record = new Record
        {
            Operation = OperationEnum.Outflow,
            Value = 50,
            Frequency = FrequencyEnum.OneTime,
            ReferenceDate = new DateTime(2024, 6, 1)
        };

        await _sut.RevertBalanceForRecordAsync(1, record);

        await _userRepository.Received(1).UpdateBalanceAsync(1, 50);
    }

    #endregion

    #region RevertBalanceForRecordsAsync

    [Fact]
    public async Task RevertBalanceForRecordsAsync_ShouldRevertSumOfPastRecords()
    {
        var records = new List<Record>
        {
            new() { Operation = OperationEnum.Deposit, Value = 200, Frequency = FrequencyEnum.OneTime, ReferenceDate = new DateTime(2024, 6, 1) },
            new() { Operation = OperationEnum.Outflow, Value = 50, Frequency = FrequencyEnum.OneTime, ReferenceDate = new DateTime(2024, 6, 10) }
        };

        await _sut.RevertBalanceForRecordsAsync(1, records);

        await _userRepository.Received(1).UpdateBalanceAsync(1, -150);
    }

    #endregion

    #region AdjustBalanceAsync

    [Fact]
    public async Task AdjustBalanceAsync_ShouldCalculateNetDifference()
    {
        await _sut.AdjustBalanceAsync(1, 100, 150);

        await _userRepository.Received(1).UpdateBalanceAsync(1, 50);
    }

    [Fact]
    public async Task AdjustBalanceAsync_WhenNoChange_ShouldNotUpdate()
    {
        await _sut.AdjustBalanceAsync(1, 100, 100);

        await _userRepository.DidNotReceive().UpdateBalanceAsync(Arg.Any<int>(), Arg.Any<decimal>());
    }

    #endregion
}
