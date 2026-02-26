using Microsoft.AspNetCore.Mvc;
using PopaDin.Bkd.Api.Dtos.User;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using Mapster;
using PopaDin.Bkd.Domain.Models.User;

namespace PopaDin.Bkd.Api.Controllers;

// [Authorize]
[Route("v1/[controller]")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
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
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest createUserRequest)
    {
        var user = createUserRequest.Adapt<User>();
        User userCreated = await userService.CreateUserAsync(user);
        return Ok(userCreated.Adapt<UserResponse>());
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de buscar uma lista paginada de users
    /// </summary>
    /// <param name="listUsersRequest">O objeto de requisicao para buscar a lista paginada de users</param>
    /// <returns>Uma lista paginada de users</returns>
    /// <response code="200">Sucesso, e retorna uma lista paginada de users</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<UserResponse>>> GetUsers([FromQuery] ListUsersRequest listUsersRequest)
    {
        // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var listUsers = listUsersRequest.Adapt<ListUsers>();
        PaginatedResult<User> users = await userService.GetUsersAsync(listUsers);
        return Ok(users.Adapt<PaginatedResult<UserResponse>>());
    }

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de buscar um User
    /// </summary>
    /// <param name="userId">O codigo User</param>
    /// <returns>O User consultado</returns>
    /// <response code="200">Sucesso, e retorna um User</response>
    [HttpGet("{userId:decimal}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> FindUserById([FromRoute] decimal userId)
    {
        // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        User user = await userService.FindUserByIdAsync(userId);
        return Ok(user.Adapt<UserResponse>());
    }

    [HttpPut("{userId:decimal}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> UpdateUser([FromBody] UpdateUserRequest updateUserRequest,
        [FromRoute] decimal userId)
    {
        // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = updateUserRequest.Adapt<User>();
        User updatedUser = await userService.UpdateUserAsync(user, userId);
        return Ok(updatedUser.Adapt<UserResponse>());
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
        // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await userService.DeleteUserAsync(userId);
        return NoContent();
    }
}
