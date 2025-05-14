using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopaDin.Bkd.Api.Dtos.Auth;
using PopaDin.Bkd.Api.Dtos.User;
using PopaDin.Bkd.Domain.Exceptions;
using PopaDin.Bkd.Domain.Models.User;

namespace PopaDin.API.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var token = await authService.GenerateToken(loginRequest.Email, loginRequest.Password);
                var response = new LoginResponse { Access_token = token };
                return Ok(response);
            }
            catch (PopaBaseException ex)
            {
                return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
            }
        }

        // [Authorize]
        [HttpGet]
        [Route("profile")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponse>> GetProfile()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            User user = await authService.GetProfile(token);
            var userResponse = user.Adapt<UserResponse>();
            return Ok(userResponse);
        }
    }
}