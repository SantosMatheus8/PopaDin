using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Domain.Models;

public class DashboardResult
{
    public DashboardSummary Summary { get; set; } = new();
    public List<DashboardGoal> Goals { get; set; } = [];
    public List<DashboardSpendingByTag> SpendingByTag { get; set; } = [];
    public List<Record> LatestRecords { get; set; } = [];
    public List<Record> TopDeposits { get; set; } = [];
    public List<Record> TopOutflows { get; set; } = [];
    public DashboardComparison? Comparison { get; set; }
    public List<DashboardMonthlyTrend> MonthlyTrend { get; set; } = [];
}

public class DashboardSummary
{
    public decimal TotalDeposits { get; set; }
    public decimal TotalOutflows { get; set; }
    public decimal Balance { get; set; }
    public int RecordCount { get; set; }
}

public class DashboardGoal
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal TargetAmount { get; set; }
    public decimal TotalSaved { get; set; }
    public decimal SavedPercentage { get; set; }
    public string Status { get; set; } = "ok";
}

public class DashboardSpendingByTag
{
    public int TagId { get; set; }
    public string TagName { get; set; } = "";
    public decimal TotalSpent { get; set; }
}

public class DashboardComparison
{
    public decimal PreviousTotalDeposits { get; set; }
    public decimal PreviousTotalOutflows { get; set; }
    public decimal DepositsChangePercent { get; set; }
    public decimal OutflowsChangePercent { get; set; }
}

public class DashboardMonthlyTrend
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalDeposits { get; set; }
    public decimal TotalOutflows { get; set; }
}
