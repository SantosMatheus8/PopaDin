using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.Tag;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface ITagService
{
    Task<Tag> CreateTagAsync(Tag tag);
    Task<PaginatedResult<Tag>> GetTagsAsync(ListTags listTags);
    Task<Tag> FindTagByIdAsync(decimal tagId);
    Task<Tag> UpdateTagAsync(Tag updateTagRequest, decimal tagId);
    Task DeleteTagAsync(decimal tagId);
}

