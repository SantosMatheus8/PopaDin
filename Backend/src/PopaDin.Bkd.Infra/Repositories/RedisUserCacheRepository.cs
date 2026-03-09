using System.Text.Json;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Models;
using StackExchange.Redis;

namespace PopaDin.Bkd.Infra.Repositories;

public class RedisUserCacheRepository(IConnectionMultiplexer redis, ILogger<RedisUserCacheRepository> logger)
    : IUserCacheRepository
{
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(60);
    private const string KeyPrefix = "user:";

    public async Task<User?> GetAsync(int userId)
    {
        try
        {
            var db = redis.GetDatabase();
            var cached = await db.StringGetAsync(BuildKey(userId));

            if (cached.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<User>(cached!);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao buscar usuário do cache Redis para o usuário {UserId}", userId);
            return null;
        }
    }

    public async Task SetAsync(int userId, User user)
    {
        try
        {
            var db = redis.GetDatabase();
            var serialized = JsonSerializer.Serialize(user);
            await db.StringSetAsync(BuildKey(userId), serialized, CacheExpiration);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao salvar usuário no cache Redis para o usuário {UserId}", userId);
        }
    }

    public async Task InvalidateAsync(int userId)
    {
        try
        {
            var db = redis.GetDatabase();
            await db.KeyDeleteAsync(BuildKey(userId));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao invalidar cache Redis de usuário para o usuário {UserId}", userId);
        }
    }

    private static string BuildKey(int userId) => $"{KeyPrefix}{userId}";
}
