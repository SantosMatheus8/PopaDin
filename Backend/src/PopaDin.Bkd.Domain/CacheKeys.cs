namespace PopaDin.Bkd.Domain;

public static class CacheKeys
{
    public static string UserTags(int userId) => $"tags:user:{userId}";
    public static string Dashboard(int userId, DateTime startDate, DateTime endDate) =>
        $"dashboard:{userId}:{startDate:yyyy-MM-dd}:{endDate:yyyy-MM-dd}";
    public static string DashboardPattern(int userId) => $"dashboard:{userId}:*";
    public static string UserData(int userId) => $"user:{userId}";
}
