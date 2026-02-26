namespace PopaDin.Bkd.Domain.Models.Budget;

public class Budget
{
    public int? Id { get; set; }
    public string Name { get; set; } = "";
    public double Goal { get; set; }
    public User.User User { get; set; } = new();
    public DateTime? FinishAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}