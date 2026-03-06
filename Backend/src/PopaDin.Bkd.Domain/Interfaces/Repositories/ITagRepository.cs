using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface ITagRepository
{
    Task<Tag> CreateTagAsync(Tag tag);
    Task<PaginatedResult<Tag>> GetTagsAsync(ListTags listTags);
    Task<Tag> FindTagByIdAsync(int tagId, int userId);
    Task<List<Tag>> FindTagsByIdsAsync(List<int> ids, int userId);
    Task UpdateTagAsync(Tag tag);
    Task DeleteTagAsync(int tagId);
}
