using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.Tag;
using PopaDin.Bkd.Domain.Models.User;

namespace PopaDin.Bkd.Service;

public class TagService(ITagRepository repository, ILogger<TagService> logger) : ITagService
{
    public async Task<Tag> CreateTagAsync(Tag tag, decimal userId)
    {
        logger.LogInformation("Criando Tag");
        tag.User = new User { Id = (int)userId };
        var tagCreated = await repository.CreateTagAsync(tag);
        return await FindTagOrThrowExceptionAsync(tagCreated.Id!.Value, userId);
    }

    public async Task<PaginatedResult<Tag>> GetTagsAsync(ListTags listTags, decimal userId)
    {
        logger.LogInformation("Listando Tag");
        listTags.UserId = (int)userId;
        return await repository.GetTagsAsync(listTags);
    }

    public async Task<Tag> FindTagByIdAsync(decimal tagId, decimal userId)
    {
        logger.LogInformation("Buscando um Tag");
        return await FindTagOrThrowExceptionAsync(tagId, userId);
    }

    public async Task<Tag> UpdateTagAsync(Tag updateTagRequest, decimal tagId, decimal userId)
    {
        logger.LogInformation("Editando um Tag");
        Tag tag = await FindTagOrThrowExceptionAsync(tagId, userId);

        tag.Name = updateTagRequest.Name;
        tag.TagType = updateTagRequest.TagType;
        tag.Description = updateTagRequest.Description;
        await repository.UpdateTagAsync(tag);

        return await FindTagOrThrowExceptionAsync(tagId, userId);
    }

    public async Task DeleteTagAsync(decimal tagId, decimal userId)
    {
        await FindTagOrThrowExceptionAsync(tagId, userId);
        await repository.DeleteTagAsync(tagId);
    }

    private async Task<Tag> FindTagOrThrowExceptionAsync(decimal tagId, decimal userId)
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
