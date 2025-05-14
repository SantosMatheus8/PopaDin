using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PopaDin.Bkd.Api.Dtos.User;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using Mapster;
using PopaDin.Bkd.Domain.Models.User;

namespace PopaDin.Bkd.Api.Controllers;

// [Authorize]
[Route("v1/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de criar um user
    /// </summary>
    /// <param name="createUserRequest">O objeto de requisicao para criar um user</param>
    /// <returns>O user criado</returns>
    /// <response code="201">Sucesso, e retorna um user</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> CreateUser(
        [FromBody] CreateUserRequest createUserRequest
    )
    {
        try
        {
            var user = createUserRequest.Adapt<User>();
            User userCreated = await _userService.CreateUserAsync(user);
            var userResponse = userCreated.Adapt<UserResponse>();

            return Ok(userResponse);
        }
        catch (PopaBaseException ex)
        {
            return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
        }
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de buscar uma lista paginada de users
    /// </summary>
    /// <param name="listUsersRequest">O objeto de requisicao para buscar a lista paginada de users</param>
    /// <returns>Uma lista paginada de users</returns>
    /// <response code="200">Sucesso, e retorna uma lista paginada de users</response>
    [HttpGet]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<UserResponse>>> GetUsers([FromQuery] ListUsersRequest listUsersRequest)
    {
        try
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var listUsers = listUsersRequest.Adapt<ListUsers>();
            PaginatedResult<User> users = await _userService.GetUsersAsync(listUsers);
            var userResponse = users.Adapt<PaginatedResult<UserResponse>>();
            return Ok(userResponse);
        }
        catch (PopaBaseException ex)
        {
            return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
        }
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de buscar um User
    /// </summary>
    /// <param name="userId">O codigo User</param>
    /// <returns>O User consultado</returns>
    /// <response code="200">Sucesso, e retorna um User</response>
    [HttpGet("{userId:decimal}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> FindUserById([FromRoute] decimal userId)
    {
        try
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            User user = await _userService.FindUserByIdAsync(userId);
            var userResponse = user.Adapt<UserResponse>();
            return Ok(userResponse);
        }
        catch (PopaBaseException ex)
        {
            return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
        }
    }

    [HttpPut("{userId:decimal}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> UpdateUser([FromBody] UpdateUserRequest updateUserRequest,
        [FromRoute] decimal userId)
    {
        try
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = updateUserRequest.Adapt<User>();
            User updatedUser = await _userService.UpdateUserAsync(user, userId);
            var userResponse = updatedUser.Adapt<UserResponse>();
            return Ok(userResponse);
        }
        catch (PopaBaseException ex)
        {
            return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
        }
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de deletar um user
    /// </summary>
    /// <param name="userId">O codigo do user</param>
    /// <returns>Confirmação de deleção</returns>
    /// <response code="204">Sucesso, e retorna confirmação de deleção</response>
    [HttpDelete("{userId:decimal}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUser([FromRoute] decimal userId)
    {
        try
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _userService.DeleteUserAsync(userId);
            return NoContent();
        }
        catch (PopaBaseException ex)
        {
            return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
        }
    }
}
