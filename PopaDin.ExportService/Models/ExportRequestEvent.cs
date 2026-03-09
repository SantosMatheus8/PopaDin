namespace PopaDin.ExportService.Models;

public class ExportRequestEvent
{
    public int UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
