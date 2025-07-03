using Foruscorp.Auth.Contruct;
using Foruscorp.Auth.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Foruscorp.Auth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController(
        IAuthService authService) : ControllerBase
    {

        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            var user = await authService.RegisterAsync(request);
            if(user is null)
                return BadRequest("User already exists");

            return Ok(user);
        }


        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserLoginDto request)
        {
            var token = await authService.LoginAsync(request); 
            if (string.IsNullOrEmpty(token))
                return Unauthorized("Invalid credentials");

            return Ok(token);   

        }

        [Authorize] // обязательная авторизация
        [HttpGet("me")]
        public ActionResult<string> GetCurrentUserId()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Token does not contain user ID");
            }

            return Ok(userId);
        }
    }
}
