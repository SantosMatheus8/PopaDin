namespace PopaDin.Bkd.Domain.Models;

public class Budget
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Goal { get; set; }
    public double CurrentAmount { get; set; }
    // public int UserId { get; set; }
    // public UserModel User { get; set; }
    public DateTime FinishAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}