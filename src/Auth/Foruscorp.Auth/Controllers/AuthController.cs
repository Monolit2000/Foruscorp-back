using Foruscorp.Auth.Contruct;
using Foruscorp.Auth.Domain.Users;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Foruscorp.Auth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController(
        ITokenProvider tokenProvider) : ControllerBase
    {


        [HttpPost("SetCurrentRouteCommand")]
        public async Task<ActionResult> SetCurrentRouteCommand()
        {
            return Ok();
        }


        //[HttpPost(Name = "Register")]
        //public ActionResult<User> Register(UserDto request)
        //{
        //    var user = new User
        //    {
        //        Id = Guid.NewGuid(),
        //        Email = request.Email,
        //        UserName = request.UserName 
        //    };  

        //    var hasedPass = new PasswordHasher<User>()
        //        .HashPassword(user, request.Password);

        //    user.PasswordHash = hasedPass;

        //    return Ok(user);    
        //}


        //[HttpPost(Name = "login")]
        //public ActionResult<string> Login(UserDto request)
        //{
        //    var user = new User
        //    {
        //        Id = Guid.NewGuid(),
        //        Email = request.Email,
        //        UserName = request.UserName
        //    };

        //    user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password); // Simulating a user fetch from a database   

        //    if (user.UserName != request.UserName)
        //    {
        //        return BadRequest("User not found");
        //    }

        //    if(new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        //    { 
        //        return BadRequest("Invalid password");  
        //    }

        //    var token = tokenProvider.Create(user);

        //    return Ok(token);
        //}


    }
}
