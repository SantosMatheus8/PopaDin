using PopaDin.Bkd.Domain.Exceptions;

namespace PopaDin.Bkd.Domain.Models;

public class Budget
{
    public int? Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Goal { get; set; }
    public User User { get; set; } = new();
    public DateTime? FinishAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public void ValidateGoal()
    {
        if (Goal < 1)
            throw new UnprocessableEntityException("A meta deve ser maior que um.");
    }
}
