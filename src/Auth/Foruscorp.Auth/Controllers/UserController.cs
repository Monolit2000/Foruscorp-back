using Foruscorp.Auth.Contruct;
using Foruscorp.Auth.Domain.Users;
using Microsoft.AspNetCore.Mvc;

namespace Foruscorp.Auth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController(
        IUserService userService) : ControllerBase
    {
        [HttpPost("set-user-role")]
        public async Task<ActionResult> SetUserRole(SetUserRoleDto request)
        {
           var token = await userService.SetUserRole(request.userId, request.roleName);
           return Ok(token);
        }

        [HttpPost("set-company")]
        public async Task<ActionResult> SetCompanyId(SetCompanyRequest request)
        {
           var token = await userService.SetCompanyId(request.UserId, request.CompanyId);
           return Ok(token);
        }

        
    }
}
