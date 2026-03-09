using System.Text.Json;
using Microsoft.Extensions.Logging;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Models;
using StackExchange.Redis;

namespace PopaDin.Bkd.Infra.Repositories;

public class RedisTagCacheRepository(IConnectionMultiplexer redis, ILogger<RedisTagCacheRepository> logger)
    : ITagCacheRepository
{
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);
    private const string KeyPrefix = "tags:user:";

    public async Task<List<Tag>?> GetUserTagsAsync(int userId)
    {
        try
        {
            var db = redis.GetDatabase();
            var cached = await db.StringGetAsync(BuildKey(userId));

            if (cached.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<List<Tag>>(cached!);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao buscar tags do cache Redis para o usuário {UserId}", userId);
            return null;
        }
    }

    public async Task SetUserTagsAsync(int userId, List<Tag> tags)
    {
        try
        {
            var db = redis.GetDatabase();
            var serialized = JsonSerializer.Serialize(tags);
            await db.StringSetAsync(BuildKey(userId), serialized, CacheExpiration);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao salvar tags no cache Redis para o usuário {UserId}", userId);
        }
    }

    public async Task InvalidateUserTagsAsync(int userId)
    {
        try
        {
            var db = redis.GetDatabase();
            await db.KeyDeleteAsync(BuildKey(userId));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao invalidar cache Redis de tags para o usuário {UserId}", userId);
        }
    }

    private static string BuildKey(int userId) => $"{KeyPrefix}{userId}";
}
