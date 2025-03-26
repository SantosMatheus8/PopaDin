namespace PopaDin.Bkd.Api.Dtos.Budget;

public class CreateBudgetRequest
{
    public string Name { get; set; }
    public double Goal { get; set; }
    public double CurrentAmount { get; set; }
    // public int UserId { get; set; }
    // public DateTime FinishAt { get; set; }
}