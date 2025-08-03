using Foruscorp.Auth.Contruct;
using Foruscorp.Auth.Domain.Users;
using MediatR;
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


        [HttpPost("delete-user-role")]
        public async Task<ActionResult> DeleteUserRole(SetUserRoleDto request)
        {
            var token = await userService.DeleteUserRole(request.userId, request.roleName);
            return Ok(token);
        }

        [HttpPost("set-company")]
        public async Task<ActionResult> SetCompanyId(SetCompanyRequest request)
        {
           var token = await userService.SetCompanyId(request.UserId, request.CompanyId);
           return Ok(token);
        }

        [HttpGet("all")]
        public async Task<ActionResult> GetAllUsers(CancellationToken cancellationToken)
        {
           var token = await userService.GetAllUsers();
           return Ok(token);
        }

        [HttpGet("by-id/{id:guid}")]
        public async Task<ActionResult> GetUserByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken)
        {
           var result = await userService.GetUserByIdAsync(id);
            if (result.IsFailed)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        [HttpGet("by-name/{name}")]
        public async Task<ActionResult> GetUserByNameAsync([FromRoute] string name, CancellationToken cancellationToken)
        {
           var result = await userService.GetUserByNameAsync(name);
            if (result.IsFailed)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }
    }
}
