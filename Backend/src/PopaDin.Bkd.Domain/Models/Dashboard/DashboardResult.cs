using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Domain.Models;

public class DashboardResult
{
    public DashboardSummary Summary { get; set; } = new();
    public List<DashboardBudget> Budgets { get; set; } = [];
    public List<DashboardSpendingByTag> SpendingByTag { get; set; } = [];
    public List<Record> LatestRecords { get; set; } = [];
    public List<Record> TopDeposits { get; set; } = [];
    public List<Record> TopOutflows { get; set; } = [];
}

public class DashboardSummary
{
    public decimal TotalDeposits { get; set; }
    public decimal TotalOutflows { get; set; }
    public decimal Balance { get; set; }
    public int RecordCount { get; set; }
}

public class DashboardBudget
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Goal { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal UsedPercentage { get; set; }
    public string Status { get; set; } = "ok";
}

public class DashboardSpendingByTag
{
    public int TagId { get; set; }
    public string TagName { get; set; } = "";
    public decimal TotalSpent { get; set; }
}
