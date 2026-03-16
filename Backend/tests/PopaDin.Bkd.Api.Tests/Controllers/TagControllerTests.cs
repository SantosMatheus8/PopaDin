using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PopaDin.Bkd.Api.Controllers;
using PopaDin.Bkd.Api.Dtos.Tag;
using PopaDin.Bkd.Api.Tests.Helpers;
using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;

namespace PopaDin.Bkd.Api.Tests.Controllers;

public class TagControllerTests
{
    private readonly ITagService _tagService = Substitute.For<ITagService>();
    private readonly TagController _sut;

    private const int AuthUserId = 1;

    public TagControllerTests()
    {
        _sut = new TagController(_tagService);
        ControllerTestHelper.SetupAuthenticatedUser(_sut, AuthUserId);
    }

    #region CreateTag

    [Fact]
    public async Task CreateTag_WithValidRequest_ShouldReturn201Created()
    {
        var request = new CreateTagRequest { Name = "Food", TagType = OperationEnum.Outflow, Color = "#FF0000" };
        _tagService.CreateTagAsync(Arg.Any<Tag>(), AuthUserId)
            .Returns(new Tag { Id = 1, Name = "Food", TagType = OperationEnum.Outflow });

        var result = await _sut.CreateTag(request);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);
    }

    #endregion

    #region GetTags

    [Fact]
    public async Task GetTags_ShouldReturnOkWithPaginatedResult()
    {
        var request = new ListTagsRequest();
        _tagService.GetTagsAsync(Arg.Any<ListTags>(), AuthUserId)
            .Returns(new PaginatedResult<Tag> { Lines = [new Tag { Id = 1 }], Page = 1, TotalItems = 1 });

        var result = await _sut.GetTags(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    #endregion

    #region FindTagById

    [Fact]
    public async Task FindTagById_WhenExists_ShouldReturnOk()
    {
        _tagService.FindTagByIdAsync(1, AuthUserId)
            .Returns(new Tag { Id = 1, Name = "Food" });

        var result = await _sut.FindTagById(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task FindTagById_WhenNotFound_ShouldThrowNotFoundException()
    {
        _tagService.FindTagByIdAsync(999, AuthUserId)
            .Throws(new NotFoundException("Tag não encontrada"));

        var act = () => _sut.FindTagById(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region UpdateTag

    [Fact]
    public async Task UpdateTag_WithValidRequest_ShouldReturnOk()
    {
        var request = new UpdateTagRequest { Name = "Updated", TagType = OperationEnum.Deposit };
        _tagService.UpdateTagAsync(Arg.Any<Tag>(), 1, AuthUserId)
            .Returns(new Tag { Id = 1, Name = "Updated" });

        var result = await _sut.UpdateTag(request, 1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateTag_WhenNotFound_ShouldThrowNotFoundException()
    {
        var request = new UpdateTagRequest { Name = "Updated" };
        _tagService.UpdateTagAsync(Arg.Any<Tag>(), 999, AuthUserId)
            .Throws(new NotFoundException("Tag não encontrada"));

        var act = () => _sut.UpdateTag(request, 999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeleteTag

    [Fact]
    public async Task DeleteTag_WhenExists_ShouldReturn204NoContent()
    {
        var result = await _sut.DeleteTag(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteTag_WhenNotFound_ShouldThrowNotFoundException()
    {
        _tagService.DeleteTagAsync(999, AuthUserId)
            .Throws(new NotFoundException("Tag não encontrada"));

        var act = () => _sut.DeleteTag(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
