using Foruscorp.Auth.Contruct;
using Foruscorp.Auth.Domain.Users;
using Foruscorp.Auth.Servises;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                var response = await authService.RefreshAsync();
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto request)
        {
            try
            {
                var response = await authService.LoginAsync(request);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
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
