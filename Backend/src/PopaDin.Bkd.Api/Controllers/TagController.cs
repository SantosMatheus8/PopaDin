using Microsoft.AspNetCore.Mvc;
using PopaDin.Bkd.Api.Dtos.Tag;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using Mapster;
using PopaDin.Bkd.Domain.Models.Tag;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace PopaDin.Bkd.Api.Controllers;

[Route("v1/[controller]")]
[ApiController]
[Authorize]
public class TagController(ITagService tagService) : ControllerBase
{
    /// <summary>
    ///     Atraves dessa rota voce sera capaz de criar um tag
    /// </summary>
    /// <param name="createTagRequest">O objeto de requisicao para criar um tag</param>
    /// <returns>O tag criado</returns>
    /// <response code="201">Sucesso, e retorna um tag</response>
    [HttpPost]
    [ProducesResponseType(typeof(TagResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TagResponse>> CreateTag([FromBody] CreateTagRequest createTagRequest)
    {
        var userId = decimal.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var tag = createTagRequest.Adapt<Tag>();
        Tag tagCreated = await tagService.CreateTagAsync(tag, userId);
        var tagResponse = tagCreated.Adapt<TagResponse>();
        return Ok(tagResponse);
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de buscar uma lista paginada de tags
    /// </summary>
    /// <param name="listTagsRequest">O objeto de requisicao para buscar a lista paginada de tags</param>
    /// <returns>Uma lista paginada de tags</returns>
    /// <response code="200">Sucesso, e retorna uma lista paginada de tags</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<TagResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<TagResponse>>> GetTags([FromQuery] ListTagsRequest listTagsRequest)
    {
        var userId = decimal.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var listTags = listTagsRequest.Adapt<ListTags>();
        PaginatedResult<Tag> tags = await tagService.GetTagsAsync(listTags, userId);
        var tagsResponse = tags.Adapt<PaginatedResult<TagResponse>>();
        return Ok(tagsResponse);
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de buscar um Tag
    /// </summary>
    /// <param name="tagId">O codigo Tag</param>
    /// <returns>O Tag consultado</returns>
    /// <response code="200">Sucesso, e retorna um Tag</response>
    [HttpGet("{tagId:decimal}")]
    [ProducesResponseType(typeof(TagResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TagResponse>> FindTagById([FromRoute] decimal tagId)
    {
        var userId = decimal.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        Tag tag = await tagService.FindTagByIdAsync(tagId, userId);
        var tagResponse = tag.Adapt<TagResponse>();
        return Ok(tagResponse);
    }

    [HttpPut("{tagId:decimal}")]
    [ProducesResponseType(typeof(TagResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TagResponse>> UpdateTag([FromBody] UpdateTagRequest updateTagRequest,
        [FromRoute] decimal tagId)
    {
        var userId = decimal.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var tag = updateTagRequest.Adapt<Tag>();
        Tag updatedTag = await tagService.UpdateTagAsync(tag, tagId, userId);
        var tagResponse = updatedTag.Adapt<TagResponse>();
        return Ok(tagResponse);
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de deletar um tag
    /// </summary>
    /// <param name="tagId">O codigo do tag</param>
    /// <returns>Confirmação de deleção</returns>
    /// <response code="204">Sucesso, e retorna confirmação de deleção</response>
    [HttpDelete("{tagId:decimal}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTag([FromRoute] decimal tagId)
    {
        var userId = decimal.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        await tagService.DeleteTagAsync(tagId, userId);
        return NoContent();
    }
}
