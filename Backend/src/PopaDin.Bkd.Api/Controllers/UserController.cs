using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopaDin.Bkd.Api.Dtos.User;
using PopaDin.Bkd.Domain.Interfaces.Services;
using PopaDin.Bkd.Domain.Models;
using Mapster;
using PopaDin.Bkd.Domain.Models.User;

namespace PopaDin.Bkd.Api.Controllers;

[Authorize]
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

    /// <summary>
    ///     Atraves dessa rota voce sera capaz de buscar uma lista paginada de users
    /// </summary>
    /// <param name="listUsersRequest">O objeto de requisicao para buscar a lista paginada de users</param>
    /// <returns>Uma lista paginada de users</returns>
    /// <response code="200">Sucesso, e retorna uma lista paginada de users</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<UserResponse>>> GetUsers([FromQuery] ListUsersRequest listUsersRequest)
    {
        var listUsers = listUsersRequest.Adapt<ListUsers>();
        PaginatedResult<User> users = await userService.GetUsersAsync(listUsers);
        var usersResponse = users.Adapt<PaginatedResult<UserResponse>>();
        return Ok(usersResponse);
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
        User user = await userService.FindUserByIdAsync(userId);
        var userResponse = user.Adapt<UserResponse>();
        return Ok(userResponse);
    }

    [HttpPut("{userId:decimal}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> UpdateUser([FromBody] UpdateUserRequest updateUserRequest,
        [FromRoute] decimal userId)
    {
        var user = updateUserRequest.Adapt<User>();
        User updatedUser = await userService.UpdateUserAsync(user, userId);
        var userResponse = updatedUser.Adapt<UserResponse>();
        return Ok(userResponse);
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
        await userService.DeleteUserAsync(userId);
        return NoContent();
    }
}
