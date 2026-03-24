namespace PopaDin.Bkd.Api.Dtos.Goal;

public class GoalResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal TargetAmount { get; set; }
    public int UserId { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime? FinishAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
