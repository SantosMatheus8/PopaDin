using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopaDin.Bkd.Api.Dtos.Auth;
using PopaDin.Bkd.Domain.Exceptions;

namespace PopaDin.API.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var token = await _authService.GenerateToken(loginRequest.Email, loginRequest.Password);
                var response = new LoginResponse { Access_token = token };
                return Ok(response);
            }
            catch (PopaBaseException ex)
            {
                return StatusCode(ex.StatusCode, new { ErrorMessage = ex.Message });
            }
        }

        // [Authorize]
        // [HttpGet]
        // [Route("profile")]
        // [ProducesResponseType(typeof(UserTokenDTO), StatusCodes.Status200OK)]
        // [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        // public async Task<ActionResult<UserTokenDTO>> GetProfile()
        // {
        //     var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        //     var userClaims = await _authService.GetProfile(token);

        //     return Ok(userClaims);
        // }
    }
}