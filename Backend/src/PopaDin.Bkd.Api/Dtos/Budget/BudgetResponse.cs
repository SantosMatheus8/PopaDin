namespace PopaDin.Bkd.Api.Dtos.Budget;

public class BudgetResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Goal { get; set; }
    public int UserId { get; set; }
    public DateTime? FinishAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
