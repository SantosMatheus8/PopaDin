using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Repositories;
using PopaDin.Bkd.Domain.Models;
using PopaDin.Bkd.Service;

namespace PopaDin.Bkd.Service.Tests;

public class TagServiceTests
{
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly ITagCacheRepository _cacheRepository = Substitute.For<ITagCacheRepository>();
    private readonly ILogger<TagService> _logger = Substitute.For<ILogger<TagService>>();
    private readonly TagService _sut;

    public TagServiceTests()
    {
        _sut = new TagService(_tagRepository, _cacheRepository, _logger);
    }

    #region CreateTagAsync

    [Fact]
    public async Task CreateTagAsync_WithValidTag_ShouldCreateAndInvalidateCache()
    {
        var tag = new Tag { Name = "Food", TagType = OperationEnum.Outflow, Color = "#FF0000" };
        var createdTag = new Tag { Id = 1, Name = "Food", TagType = OperationEnum.Outflow };
        var fetchedTag = new Tag { Id = 1, Name = "Food", TagType = OperationEnum.Outflow, User = new User { Id = 1 } };

        _tagRepository.CreateTagAsync(Arg.Any<Tag>()).Returns(createdTag);
        _tagRepository.FindTagByIdAsync(1, 1).Returns(fetchedTag);

        var result = await _sut.CreateTagAsync(tag, 1);

        result.Should().Be(fetchedTag);
        await _cacheRepository.Received(1).InvalidateUserTagsAsync(1);
    }

    #endregion

    #region GetTagsAsync

    [Fact]
    public async Task GetTagsAsync_ShouldSetUserIdAndReturn()
    {
        var listTags = new ListTags { Page = 1, ItemsPerPage = 20 };
        var expected = new PaginatedResult<Tag> { Lines = [new Tag { Id = 1 }], Page = 1, TotalItems = 1 };
        _tagRepository.GetTagsAsync(Arg.Any<ListTags>()).Returns(expected);

        var result = await _sut.GetTagsAsync(listTags, 1);

        result.Should().Be(expected);
        listTags.UserId.Should().Be(1);
    }

    #endregion

    #region FindTagByIdAsync

    [Fact]
    public async Task FindTagByIdAsync_WhenCachedTagExists_ShouldReturnFromCache()
    {
        var cachedTag = new Tag { Id = 1, Name = "Cached" };
        _cacheRepository.GetUserTagsAsync(1).Returns(new List<Tag> { cachedTag });

        var result = await _sut.FindTagByIdAsync(1, 1);

        result.Should().Be(cachedTag);
        await _tagRepository.DidNotReceive().FindTagByIdAsync(1, 1);
    }

    [Fact]
    public async Task FindTagByIdAsync_WhenNotInCache_ShouldFetchFromRepository()
    {
        var tag = new Tag { Id = 1, Name = "Test" };
        _cacheRepository.GetUserTagsAsync(1).Returns((List<Tag>?)null);
        _tagRepository.FindAllTagsByUserIdAsync(1).Returns(new List<Tag> { tag });
        _tagRepository.FindTagByIdAsync(1, 1).Returns(tag);

        var result = await _sut.FindTagByIdAsync(1, 1);

        result.Should().Be(tag);
    }

    [Fact]
    public async Task FindTagByIdAsync_WhenCachedButNotMatchingId_ShouldFetchFromRepository()
    {
        var otherTag = new Tag { Id = 2, Name = "Other" };
        var expectedTag = new Tag { Id = 1, Name = "Expected" };
        _cacheRepository.GetUserTagsAsync(1).Returns(new List<Tag> { otherTag });
        _tagRepository.FindTagByIdAsync(1, 1).Returns(expectedTag);

        var result = await _sut.FindTagByIdAsync(1, 1);

        result.Should().Be(expectedTag);
    }

    [Fact]
    public async Task FindTagByIdAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _cacheRepository.GetUserTagsAsync(1).Returns((List<Tag>?)null);
        _tagRepository.FindAllTagsByUserIdAsync(1).Returns(new List<Tag>());
        _tagRepository.FindTagByIdAsync(999, 1).Returns((Tag?)null);

        var act = () => _sut.FindTagByIdAsync(999, 1);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Tag não encontrada");
    }

    #endregion

    #region UpdateTagAsync

    [Fact]
    public async Task UpdateTagAsync_WithValidData_ShouldUpdateAndInvalidateCache()
    {
        var existingTag = new Tag { Id = 1, Name = "Old", TagType = OperationEnum.Outflow, Color = "#000" };
        var updateRequest = new Tag { Name = "New", TagType = OperationEnum.Deposit, Description = "Updated", Color = "#FFF" };
        var updatedTag = new Tag { Id = 1, Name = "New", TagType = OperationEnum.Deposit };

        _tagRepository.FindTagByIdAsync(1, 1).Returns(existingTag, updatedTag);

        var result = await _sut.UpdateTagAsync(updateRequest, 1, 1);

        result.Name.Should().Be("New");
        await _cacheRepository.Received(1).InvalidateUserTagsAsync(1);
    }

    [Fact]
    public async Task UpdateTagAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _tagRepository.FindTagByIdAsync(999, 1).Returns((Tag?)null);

        var act = () => _sut.UpdateTagAsync(new Tag { Name = "X" }, 999, 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeleteTagAsync

    [Fact]
    public async Task DeleteTagAsync_WhenExists_ShouldDeleteAndInvalidateCache()
    {
        var tag = new Tag { Id = 1, Name = "Test" };
        _tagRepository.FindTagByIdAsync(1, 1).Returns(tag);

        await _sut.DeleteTagAsync(1, 1);

        await _tagRepository.Received(1).DeleteTagAsync(1);
        await _cacheRepository.Received(1).InvalidateUserTagsAsync(1);
    }

    [Fact]
    public async Task DeleteTagAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _tagRepository.FindTagByIdAsync(999, 1).Returns((Tag?)null);

        var act = () => _sut.DeleteTagAsync(999, 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
