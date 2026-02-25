using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.Tag;
using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Domain.Interfaces.Repositories;

public interface ITagRepository
{
    Task<Tag> CreateTagAsync(Tag tag);
    Task<PaginatedResult<Tag>> GetTagsAsync(ListTags listTags);
    Task<Tag> FindTagByIdAsync(decimal tagId);
    Task<List<Tag>> FindTagsByIdsAsync(List<int> ids);
    Task UpdateTagAsync(Tag tag);
    Task DeleteTagAsync(decimal tagId);
}