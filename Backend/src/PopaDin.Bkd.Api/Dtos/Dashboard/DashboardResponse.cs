using PopaDin.Bkd.Api.Dtos.Record;

namespace PopaDin.Bkd.Api.Dtos.Dashboard;

public class DashboardResponse
{
    public DashboardSummaryResponse Summary { get; set; } = new();
    public List<DashboardGoalResponse> Goals { get; set; } = [];
    public List<DashboardSpendingByTagResponse> SpendingByTag { get; set; } = [];
    public List<RecordResponse> LatestRecords { get; set; } = [];
    public List<RecordResponse> TopDeposits { get; set; } = [];
    public List<RecordResponse> TopOutflows { get; set; } = [];
}

public class DashboardSummaryResponse
{
    public decimal TotalDeposits { get; set; }
    public decimal TotalOutflows { get; set; }
    public decimal Balance { get; set; }
    public int RecordCount { get; set; }
}

public class DashboardGoalResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal TargetAmount { get; set; }
    public decimal TotalSaved { get; set; }
    public decimal SavedPercentage { get; set; }
    public string Status { get; set; } = "";
}

public class DashboardSpendingByTagResponse
{
    public int TagId { get; set; }
    public string TagName { get; set; } = "";
    public decimal TotalSpent { get; set; }
}
