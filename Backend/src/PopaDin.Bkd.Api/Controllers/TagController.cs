using Microsoft.AspNetCore.Mvc;
using PopaDin.Bkd.Api.Dtos.Tag;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace PopaDin.Bkd.Api.Controllers;

[Route("v1/[controller]")]
[ApiController]
[Authorize]
public class TagController(ITagService tagService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(TagResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TagResponse>> CreateTag([FromBody] CreateTagRequest createTagRequest)
    {
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var tag = createTagRequest.Adapt<Tag>();
        Tag tagCreated = await tagService.CreateTagAsync(tag, userId);
        var tagResponse = tagCreated.Adapt<TagResponse>();
        return StatusCode(StatusCodes.Status201Created, tagResponse);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<TagResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResult<TagResponse>>> GetTags([FromQuery] ListTagsRequest listTagsRequest)
    {
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var listTags = listTagsRequest.Adapt<ListTags>();
        PaginatedResult<Tag> tags = await tagService.GetTagsAsync(listTags, userId);
        var tagsResponse = tags.Adapt<PaginatedResult<TagResponse>>();
        return Ok(tagsResponse);
    }

    [HttpGet("{tagId:int}")]
    [ProducesResponseType(typeof(TagResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TagResponse>> FindTagById([FromRoute] int tagId)
    {
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        Tag tag = await tagService.FindTagByIdAsync(tagId, userId);
        var tagResponse = tag.Adapt<TagResponse>();
        return Ok(tagResponse);
    }

    [HttpPut("{tagId:int}")]
    [ProducesResponseType(typeof(TagResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TagResponse>> UpdateTag([FromBody] UpdateTagRequest updateTagRequest,
        [FromRoute] int tagId)
    {
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var tag = updateTagRequest.Adapt<Tag>();
        Tag updatedTag = await tagService.UpdateTagAsync(tag, tagId, userId);
        var tagResponse = updatedTag.Adapt<TagResponse>();
        return Ok(tagResponse);
    }

    [HttpDelete("{tagId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTag([FromRoute] int tagId)
    {
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        await tagService.DeleteTagAsync(tagId, userId);
        return NoContent();
    }
}
