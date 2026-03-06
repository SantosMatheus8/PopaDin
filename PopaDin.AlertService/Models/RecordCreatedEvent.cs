namespace PopaDin.AlertService.Models;

public class RecordCreatedEvent
{
    public int UserId { get; set; }
    public double Value { get; set; }
    public string Operation { get; set; } = "";
    public double NewBalance { get; set; }
}
