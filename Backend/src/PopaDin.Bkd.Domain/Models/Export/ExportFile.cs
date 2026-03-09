namespace PopaDin.Bkd.Domain.Models;

public class ExportFile
{
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
    public long Size { get; set; }
    public DateTime? CreatedAt { get; set; }
}
