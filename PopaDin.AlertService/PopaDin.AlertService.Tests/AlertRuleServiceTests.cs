using FluentAssertions;
using PopaDin.AlertService.Models;
using PopaDin.AlertService.Services;

namespace PopaDin.AlertService.Tests;

public class AlertRuleServiceTests
{
    #region IsRuleTriggered

    [Fact]
    public void IsRuleTriggered_BalanceBelow_WhenBalanceBelowThreshold_ShouldReturnTrue()
    {
        var rule = new AlertRule { Type = nameof(AlertRuleType.BALANCE_BELOW), Threshold = 500 };
        var recordEvent = new RecordCreatedEvent { NewBalance = 300 };

        var result = CreateService().IsRuleTriggered(rule, recordEvent);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsRuleTriggered_BalanceBelow_WhenBalanceAboveThreshold_ShouldReturnFalse()
    {
        var rule = new AlertRule { Type = nameof(AlertRuleType.BALANCE_BELOW), Threshold = 500 };
        var recordEvent = new RecordCreatedEvent { NewBalance = 700 };

        var result = CreateService().IsRuleTriggered(rule, recordEvent);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsRuleTriggered_BalanceBelow_WhenBalanceEqualsThreshold_ShouldReturnFalse()
    {
        var rule = new AlertRule { Type = nameof(AlertRuleType.BALANCE_BELOW), Threshold = 500 };
        var recordEvent = new RecordCreatedEvent { NewBalance = 500 };

        var result = CreateService().IsRuleTriggered(rule, recordEvent);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsRuleTriggered_GoalAbove_WhenExpensesAboveThreshold_ShouldReturnTrue()
    {
        var rule = new AlertRule { Type = nameof(AlertRuleType.GOAL_ABOVE), Threshold = 1000 };
        var recordEvent = new RecordCreatedEvent { MonthlyExpenses = 1500 };

        var result = CreateService().IsRuleTriggered(rule, recordEvent);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsRuleTriggered_GoalAbove_WhenExpensesBelowThreshold_ShouldReturnFalse()
    {
        var rule = new AlertRule { Type = nameof(AlertRuleType.GOAL_ABOVE), Threshold = 1000 };
        var recordEvent = new RecordCreatedEvent { MonthlyExpenses = 800 };

        var result = CreateService().IsRuleTriggered(rule, recordEvent);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsRuleTriggered_GoalAbove_WhenExpensesEqualThreshold_ShouldReturnFalse()
    {
        var rule = new AlertRule { Type = nameof(AlertRuleType.GOAL_ABOVE), Threshold = 1000 };
        var recordEvent = new RecordCreatedEvent { MonthlyExpenses = 1000 };

        var result = CreateService().IsRuleTriggered(rule, recordEvent);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsRuleTriggered_UnknownType_ShouldReturnFalse()
    {
        var rule = new AlertRule { Type = "UNKNOWN_TYPE", Threshold = 500 };
        var recordEvent = new RecordCreatedEvent { NewBalance = 100, MonthlyExpenses = 2000 };

        var result = CreateService().IsRuleTriggered(rule, recordEvent);

        result.Should().BeFalse();
    }

    #endregion

    private static AlertRuleService CreateService()
    {
        var database = NSubstitute.Substitute.For<MongoDB.Driver.IMongoDatabase>();
        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<AlertRuleService>>();
        return new AlertRuleService(database, logger);
    }
}
