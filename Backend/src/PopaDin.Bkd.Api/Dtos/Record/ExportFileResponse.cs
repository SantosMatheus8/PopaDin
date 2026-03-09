namespace PopaDin.Bkd.Api.Dtos.Record;

public class ExportFileResponse
{
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
    public long Size { get; set; }
    public DateTime? CreatedAt { get; set; }
}
