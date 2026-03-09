using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface ITagCacheRepository
{
    Task<List<Tag>?> GetUserTagsAsync(int userId);
    Task SetUserTagsAsync(int userId, List<Tag> tags);
    Task InvalidateUserTagsAsync(int userId);
}
