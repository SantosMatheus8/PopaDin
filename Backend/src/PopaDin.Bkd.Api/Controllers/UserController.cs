using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopaDin.Bkd.Api.Dtos.User;
using PopaDin.Bkd.Api.Extensions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using Mapster;

namespace PopaDin.Bkd.Api.Controllers;

[Authorize]
[Route("v1/[controller]")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest createUserRequest)
    {
        var user = createUserRequest.Adapt<User>();
        User userCreated = await userService.CreateUserAsync(user);
        var userResponse = userCreated.Adapt<UserResponse>();
        return StatusCode(StatusCodes.Status201Created, userResponse);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<UserResponse>>> GetUsers([FromQuery] ListUsersRequest listUsersRequest)
    {
        var userId = User.GetUserId();
        var listUsers = listUsersRequest.Adapt<ListUsers>();
        PaginatedResult<User> users = await userService.GetUsersAsync(listUsers, userId);
        var usersResponse = users.Adapt<PaginatedResult<UserResponse>>();
        return Ok(usersResponse);
    }

    [HttpGet("{userId:int}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> FindUserById([FromRoute] int userId)
    {
        var authenticatedUserId = User.GetUserId();
        User user = await userService.FindUserByIdAsync(userId, authenticatedUserId);
        var userResponse = user.Adapt<UserResponse>();
        return Ok(userResponse);
    }

    [HttpPut("{userId:int}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> UpdateUser([FromBody] UpdateUserRequest updateUserRequest,
        [FromRoute] int userId)
    {
        var authenticatedUserId = User.GetUserId();
        var user = updateUserRequest.Adapt<User>();
        User updatedUser = await userService.UpdateUserAsync(user, userId, authenticatedUserId);
        var userResponse = updatedUser.Adapt<UserResponse>();
        return Ok(userResponse);
    }

    [HttpDelete("{userId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUser([FromRoute] int userId)
    {
        var authenticatedUserId = User.GetUserId();
        await userService.DeleteUserAsync(userId, authenticatedUserId);
        return NoContent();
    }

    [HttpPatch("{userId:int}/balance")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> AdjustBalance(
        [FromBody] AdjustBalanceRequest adjustBalanceRequest,
        [FromRoute] int userId)
    {
        var authenticatedUserId = User.GetUserId();
        User updatedUser = await userService.AdjustBalanceAsync(userId, authenticatedUserId, adjustBalanceRequest.Balance);
        var userResponse = updatedUser.Adapt<UserResponse>();
        return Ok(userResponse);
    }

    [HttpPost("{userId:int}/profile-picture")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult> UploadProfilePicture([FromRoute] int userId, IFormFile file)
    {
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest(new ProblemDetails { Detail = "Tipo de arquivo não permitido. Use JPG, PNG ou WebP." });

        var authenticatedUserId = User.GetUserId();
        using var stream = file.OpenReadStream();
        var url = await userService.UploadProfilePictureAsync(userId, authenticatedUserId, stream, file.ContentType);
        return Ok(new { profilePictureUrl = url });
    }

    [HttpDelete("{userId:int}/profile-picture")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteProfilePicture([FromRoute] int userId)
    {
        var authenticatedUserId = User.GetUserId();
        await userService.DeleteProfilePictureAsync(userId, authenticatedUserId);
        return NoContent();
    }
}
