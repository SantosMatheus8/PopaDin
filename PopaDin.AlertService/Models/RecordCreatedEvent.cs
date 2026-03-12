namespace PopaDin.AlertService.Models;

public class RecordCreatedEvent
{
    public int UserId { get; set; }
    public decimal Value { get; set; }
    public string Operation { get; set; } = "";
    public decimal NewBalance { get; set; }
    public decimal MonthlyExpenses { get; set; }
}
