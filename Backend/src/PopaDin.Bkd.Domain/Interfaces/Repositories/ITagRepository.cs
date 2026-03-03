using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.Tag;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface ITagRepository
{
    Task<Tag> CreateTagAsync(Tag tag);
    Task<PaginatedResult<Tag>> GetTagsAsync(ListTags listTags);
    Task<Tag> FindTagByIdAsync(decimal tagId, decimal userId);
    Task<List<Tag>> FindTagsByIdsAsync(List<int> ids, decimal userId);
    Task UpdateTagAsync(Tag tag);
    Task DeleteTagAsync(decimal tagId);
}
