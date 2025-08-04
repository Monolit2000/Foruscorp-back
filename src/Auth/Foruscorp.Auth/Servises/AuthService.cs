using Foruscorp.Auth.Controllers;
using Foruscorp.Auth.Contruct;
using Foruscorp.Auth.DataBase;
using Foruscorp.Auth.Domain.Users;
using Foruscorp.Auth.Infrastructure;
using Foruscorp.Users.IntegrationEvents;

//using Foruscorp.TrucksTracking.IntegrationEvents;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Auth.Servises
{
    public class AuthService(
        UserContext context, 
        IConfiguration configuration,
        ITokenProvider tokenProvider,
        IPublishEndpoint publishEndpoint) : IAuthService
    {
        public async Task<LoginResponce> LoginAsync(UserLoginDto request)
        {
            var user = await context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserName == request.UserName);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var verifyResult = new PasswordHasher<User>()
                .VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verifyResult == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Invalid password.");

            var token = tokenProvider.Create(user);

            return new LoginResponce { UserId = user.Id, Token = token };
        }

        public async Task GenerateRefreshToken(Guid userId)
        {

        }
        public async Task<User> RegisterAsync(UserAuthDto request)
        {
            if(await context.Users.AnyAsync(u => u.UserName == request.UserName))
            {
                return null;
            }

            var user = User.CreateNew(request.Email, request.UserName);

            if (request.CompanyId.HasValue)
                user.SetCompanyId(request.CompanyId.Value);

            var hasedPass = new PasswordHasher<User>()
                .HashPassword(user, request.Password);
            user.PasswordHash = hasedPass;

            await context.Users.AddAsync(user);

            await context.SaveChangesAsync();

            await publishEndpoint.Publish(new NewUserRegistratedIntegrationEvent
            {
                UserId = user.Id,
            });

            return user;
        }
    }
}
