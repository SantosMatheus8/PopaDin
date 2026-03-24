namespace PopaDin.Bkd.Api.Dtos.Goal;

public class UpdateGoalRequest
{
    public string Name { get; set; } = "";
    public decimal TargetAmount { get; set; }
    public DateTime? Deadline { get; set; }
}
