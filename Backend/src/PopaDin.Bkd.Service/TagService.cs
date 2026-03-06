using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Service;

public class TagService(ITagRepository repository, ILogger<TagService> logger) : ITagService
{
    public async Task<Tag> CreateTagAsync(Tag tag, int userId)
    {
        logger.LogInformation("Criando Tag");
        tag.User = new User { Id = userId };
        var tagCreated = await repository.CreateTagAsync(tag);
        return await FindTagOrThrowAsync(tagCreated.Id!.Value, userId);
    }

    public async Task<PaginatedResult<Tag>> GetTagsAsync(ListTags listTags, int userId)
    {
        logger.LogInformation("Listando Tag");
        listTags.UserId = userId;
        return await repository.GetTagsAsync(listTags);
    }

    public async Task<Tag> FindTagByIdAsync(int tagId, int userId)
    {
        logger.LogInformation("Buscando um Tag");
        return await FindTagOrThrowAsync(tagId, userId);
    }

    public async Task<Tag> UpdateTagAsync(Tag updateTagRequest, int tagId, int userId)
    {
        logger.LogInformation("Editando um Tag");
        Tag tag = await FindTagOrThrowAsync(tagId, userId);

        tag.Name = updateTagRequest.Name;
        tag.TagType = updateTagRequest.TagType;
        tag.Description = updateTagRequest.Description;
        await repository.UpdateTagAsync(tag);

        return await FindTagOrThrowAsync(tagId, userId);
    }

    public async Task DeleteTagAsync(int tagId, int userId)
    {
        await FindTagOrThrowAsync(tagId, userId);
        await repository.DeleteTagAsync(tagId);
    }

    private async Task<Tag> FindTagOrThrowAsync(int tagId, int userId)
    {
        Tag tag = await repository.FindTagByIdAsync(tagId, userId);

        if (tag == null)
        {
            logger.LogInformation("Tag nao encontrada");
            throw new NotFoundException("Tag não encontrada");
        }

        return tag;
    }
}
