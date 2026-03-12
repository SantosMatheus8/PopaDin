namespace PopaDin.ExportService.Models;

public static class FrequencyType
{
    public const int Monthly = 0;
    public const int Bimonthly = 1;
    public const int Quarterly = 2;
    public const int Semiannual = 3;
    public const int Annual = 4;
    public const int OneTime = 5;

    public static int GetMonthInterval(int frequency) => frequency switch
    {
        Monthly => 1,
        Bimonthly => 2,
        Quarterly => 3,
        Semiannual => 6,
        Annual => 12,
        _ => 0
    };

    public static string GetDisplayName(int frequency) => frequency switch
    {
        Monthly => "Mensal",
        Bimonthly => "Bimestral",
        Quarterly => "Trimestral",
        Semiannual => "Semestral",
        Annual => "Anual",
        _ => "-"
    };
}
