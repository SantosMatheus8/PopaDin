using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.Tag;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface ITagService
{
    Task<Tag> CreateTagAsync(Tag tag, decimal userId);
    Task<PaginatedResult<Tag>> GetTagsAsync(ListTags listTags, decimal userId);
    Task<Tag> FindTagByIdAsync(decimal tagId, decimal userId);
    Task<Tag> UpdateTagAsync(Tag updateTagRequest, decimal tagId, decimal userId);
    Task DeleteTagAsync(decimal tagId, decimal userId);
}
