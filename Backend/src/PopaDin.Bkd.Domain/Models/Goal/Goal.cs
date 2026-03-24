using PopaDin.Bkd.Domain.Exceptions;

namespace PopaDin.Bkd.Domain.Models;

public class Goal
{
    public int? Id { get; set; }
    public string Name { get; set; } = "";
    public decimal TargetAmount { get; set; }
    public User User { get; set; } = new();
    public DateTime? Deadline { get; set; }
    public DateTime? FinishAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public void ValidateTargetAmount()
    {
        if (TargetAmount < 1)
            throw new UnprocessableEntityException("O valor da meta deve ser maior que um.");
    }
}
