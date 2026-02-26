using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Domain.Models.Tag;

namespace PopaDin.Bkd.Service;

public class TagService(ITagRepository repository, ILogger<TagService> logger) : ITagService
{
    public async Task<Tag> CreateTagAsync(Tag tag)
    {
        logger.LogInformation("Criando Tag");

        return await repository.CreateTagAsync(tag);
    }

    public async Task<PaginatedResult<Tag>> GetTagsAsync(ListTags listTags)
    {
        logger.LogInformation("Listando Tag");
        return await repository.GetTagsAsync(listTags);
    }

    public async Task<Tag> FindTagByIdAsync(decimal tagId)
    {
        logger.LogInformation("Buscando um Tag");
        return await FindTagOrThrowExceptionAsync(tagId);
    }

    public async Task<Tag> UpdateTagAsync(Tag updateTagRequest, decimal tagId)
    {
        logger.LogInformation("Editando um Tag");
        Tag tag = await FindTagOrThrowExceptionAsync(tagId);

        tag.Name = updateTagRequest.Name;
        tag.TagType = updateTagRequest.TagType;
        tag.Description = updateTagRequest.Description;
        await repository.UpdateTagAsync(tag);

        return await repository.FindTagByIdAsync(tagId);
    }

    public async Task DeleteTagAsync(decimal tagId)
    {
        await FindTagOrThrowExceptionAsync(tagId);
        await repository.DeleteTagAsync(tagId);
    }

    private async Task<Tag> FindTagOrThrowExceptionAsync(decimal tagId)
    {
        Tag tag = await repository.FindTagByIdAsync(tagId);

        if (tag == null)
        {
            logger.LogInformation("Tag nao encontrada");
            throw new PopaBaseException("Tag não encontrada", 404);
        }

        return tag;
    }
}