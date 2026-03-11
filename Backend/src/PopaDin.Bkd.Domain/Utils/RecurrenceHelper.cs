using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Utils;

public static class RecurrenceHelper
{
    public static int GetMonthInterval(FrequencyEnum frequency)
    {
        return frequency switch
        {
            FrequencyEnum.Monthly => 1,
            FrequencyEnum.Bimonthly => 2,
            FrequencyEnum.Quarterly => 3,
            FrequencyEnum.Semiannual => 6,
            FrequencyEnum.Annual => 12,
            _ => 0
        };
    }

    public static List<DateTime> ProjectOccurrences(Record record, DateTime periodStart, DateTime periodEnd)
    {
        if (!record.IsRecurring)
            return [];

        var interval = GetMonthInterval(record.Frequency);
        if (interval == 0) return [];

        var baseDate = record.ReferenceDate ?? record.CreatedAt ?? DateTime.UtcNow;
        var endLimit = record.RecurrenceEndDate.HasValue && record.RecurrenceEndDate.Value < periodEnd
            ? record.RecurrenceEndDate.Value
            : periodEnd;

        var occurrences = new List<DateTime>();
        var current = baseDate;

        if (current < periodStart)
        {
            var monthsAhead = MonthsDifference(current, periodStart);
            var steps = monthsAhead / interval;
            current = current.AddMonths(steps * interval);
            if (current < periodStart)
                current = current.AddMonths(interval);
        }

        while (current <= endLimit)
        {
            if (current >= periodStart && current <= periodEnd)
                occurrences.Add(current);

            current = current.AddMonths(interval);
        }

        return occurrences;
    }

    public static int CountOccurrencesUpTo(Record record, DateTime cutoffDate)
    {
        if (!record.IsRecurring)
            return 0;

        var interval = GetMonthInterval(record.Frequency);
        if (interval == 0) return 0;

        var baseDate = record.ReferenceDate ?? record.CreatedAt ?? DateTime.UtcNow;

        if (baseDate > cutoffDate)
            return 0;

        var endLimit = record.RecurrenceEndDate.HasValue && record.RecurrenceEndDate.Value < cutoffDate
            ? record.RecurrenceEndDate.Value
            : cutoffDate;

        var count = 0;
        var current = baseDate;

        while (current <= endLimit)
        {
            count++;
            current = current.AddMonths(interval);
        }

        return count;
    }

    public static List<Record> ProjectRecordsForPeriod(Record record, DateTime periodStart, DateTime periodEnd)
    {
        var dates = ProjectOccurrences(record, periodStart, periodEnd);

        return dates.Select(date => new Record
        {
            Id = record.Id,
            Name = record.Name,
            Operation = record.Operation,
            Value = record.Value,
            Frequency = record.Frequency,
            ReferenceDate = date,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt,
            Tags = record.Tags,
            User = record.User,
            InstallmentGroupId = record.InstallmentGroupId,
            InstallmentIndex = record.InstallmentIndex,
            InstallmentTotal = record.InstallmentTotal,
            RecurrenceEndDate = record.RecurrenceEndDate
        }).ToList();
    }

    private static int MonthsDifference(DateTime from, DateTime to)
    {
        return (to.Year - from.Year) * 12 + (to.Month - from.Month);
    }
}
