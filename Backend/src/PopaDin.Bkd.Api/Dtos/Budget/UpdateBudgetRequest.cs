namespace PopaDin.Bkd.Api.Dtos.Budget;

public class UpdateBudgetRequest
{
    public string Name { get; set; } = "";
    public double Goal { get; set; }
    public double CurrentAmount { get; set; }
    // public DateTime FinishAt { get; set; }
}