namespace PopaDin.Bkd.Api.Dtos.Goal;

public class CreateGoalRequest
{
    public string Name { get; set; } = "";
    public decimal TargetAmount { get; set; }
    public DateTime? Deadline { get; set; }
}
