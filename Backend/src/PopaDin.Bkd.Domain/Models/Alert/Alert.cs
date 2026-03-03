using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Domain.Models.Alert;

public class Alert
{
    public string? Id { get; set; }
    public AlertType Type { get; set; }
    public double Threshold { get; set; }
    public string Channel { get; set; } = "";
    public bool Active { get; set; } = true;
    public User.User User { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
