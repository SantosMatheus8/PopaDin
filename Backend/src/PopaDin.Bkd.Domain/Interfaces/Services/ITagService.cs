using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Domain.Interfaces.Services;

public interface ITagService
{
    Task<Tag> CreateTagAsync(Tag tag, int userId);
    Task<PaginatedResult<Tag>> GetTagsAsync(ListTags listTags, int userId);
    Task<Tag> FindTagByIdAsync(int tagId, int userId);
    Task<Tag> UpdateTagAsync(Tag updateTagRequest, int tagId, int userId);
    Task DeleteTagAsync(int tagId, int userId);
}
