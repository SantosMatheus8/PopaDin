using FluentAssertions;
using PopaDin.ExportService.Models;

namespace PopaDin.ExportService.Tests;

public class FrequencyTypeTests
{
    [Theory]
    [InlineData(FrequencyType.Monthly, 1)]
    [InlineData(FrequencyType.Bimonthly, 2)]
    [InlineData(FrequencyType.Quarterly, 3)]
    [InlineData(FrequencyType.Semiannual, 6)]
    [InlineData(FrequencyType.Annual, 12)]
    [InlineData(FrequencyType.OneTime, 0)]
    [InlineData(99, 0)]
    public void GetMonthInterval_ShouldReturnCorrectInterval(int frequency, int expected)
    {
        FrequencyType.GetMonthInterval(frequency).Should().Be(expected);
    }

    [Theory]
    [InlineData(FrequencyType.Monthly, "Mensal")]
    [InlineData(FrequencyType.Bimonthly, "Bimestral")]
    [InlineData(FrequencyType.Quarterly, "Trimestral")]
    [InlineData(FrequencyType.Semiannual, "Semestral")]
    [InlineData(FrequencyType.Annual, "Anual")]
    [InlineData(FrequencyType.OneTime, "-")]
    [InlineData(99, "-")]
    public void GetDisplayName_ShouldReturnCorrectName(int frequency, string expected)
    {
        FrequencyType.GetDisplayName(frequency).Should().Be(expected);
    }
}
