namespace PopaDin.Bkd.Api.Dtos.Alert;

public class AlertResponse
{
    public string Id { get; set; } = "";
    public int UserId { get; set; }
    public string Type { get; set; } = "";
    public decimal Threshold { get; set; }
    public string Channel { get; set; } = "";
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
}
