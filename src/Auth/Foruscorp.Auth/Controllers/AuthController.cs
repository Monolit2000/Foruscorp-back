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
        IAuthService authService,
        IUserService userService) : ControllerBase
    {
        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register(UserAuthDto request)
        {
            var user = await authService.RegisterAsync(request);
            if(user is null)
                return BadRequest("User already exists");

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserLoginDto request)
        {
            try
            {
                var responce = await authService.LoginAsync(request);
                if (string.IsNullOrEmpty(responce.Token))
                    return Unauthorized("Invalid credentials");

                return Ok(responce);
            }
            catch (KeyNotFoundException)
            {
                return Unauthorized("User not found");
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid password");
            }

        }

        [Authorize] // ������������ �����������
        [HttpGet("me")]
        public ActionResult<string> GetCurrentUserId()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Token does not contain user ID");
            }

            var user = new
            {
                UserId = userId,
                CompanyId = HttpContext.User.FindFirst("company_id")?.Value,
                Roles = HttpContext.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList()
            };

            return Ok(user);
        }
    }
}
