using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Exceptions;

namespace PopaDin.Bkd.Domain.Models;

public class Alert
{
    public string? Id { get; set; }
    public AlertType Type { get; set; }
    public decimal Threshold { get; set; }
    public string Channel { get; set; } = "";
    public bool Active { get; set; } = true;
    public User User { get; set; } = new();
    public DateTime CreatedAt { get; set; }

    public void ValidateThreshold()
    {
        if (Threshold <= 0)
            throw new UnprocessableEntityException("O threshold deve ser maior que zero.");
    }
}
