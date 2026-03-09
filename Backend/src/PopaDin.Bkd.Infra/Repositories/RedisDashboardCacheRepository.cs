using System.Text.Json;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Models;
using StackExchange.Redis;

namespace PopaDin.Bkd.Infra.Repositories;

public class RedisDashboardCacheRepository(IConnectionMultiplexer redis, ILogger<RedisDashboardCacheRepository> logger)
    : IDashboardCacheRepository
{
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(50);
    private const string KeyPrefix = "dashboard:";

    public async Task<DashboardResult?> GetAsync(int userId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var db = redis.GetDatabase();
            var cached = await db.StringGetAsync(BuildKey(userId, startDate, endDate));

            if (cached.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<DashboardResult>(cached!);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao buscar dashboard do cache Redis para o usuário {UserId}", userId);
            return null;
        }
    }

    public async Task SetAsync(int userId, DateTime startDate, DateTime endDate, DashboardResult dashboard)
    {
        try
        {
            var db = redis.GetDatabase();
            var serialized = JsonSerializer.Serialize(dashboard);
            await db.StringSetAsync(BuildKey(userId, startDate, endDate), serialized, CacheExpiration);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao salvar dashboard no cache Redis para o usuário {UserId}", userId);
        }
    }

    public async Task InvalidateAsync(int userId)
    {
        try
        {
            var server = redis.GetServers().First();
            var db = redis.GetDatabase();
            var pattern = $"{KeyPrefix}{userId}:*";

            await foreach (var key in server.KeysAsync(pattern: pattern))
            {
                await db.KeyDeleteAsync(key);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao invalidar cache Redis de dashboard para o usuário {UserId}", userId);
        }
    }

    private static string BuildKey(int userId, DateTime startDate, DateTime endDate) =>
        $"{KeyPrefix}{userId}:{startDate:yyyy-MM-dd}:{endDate:yyyy-MM-dd}";
}
