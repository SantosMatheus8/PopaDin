using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Api.Dtos.Alert;

public class CreateAlertRequest
{
    public AlertType Type { get; set; }
    public double Threshold { get; set; }
    public string Channel { get; set; } = "";
}
