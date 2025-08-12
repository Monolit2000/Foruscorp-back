using Foruscorp.Auth.Controllers;
using Foruscorp.Auth.Contruct;
using Foruscorp.Auth.DataBase;
using Foruscorp.Auth.Domain.Users;
using Foruscorp.Auth.Infrastructure;
using Foruscorp.Users.IntegrationEvents;

//using Foruscorp.TrucksTracking.IntegrationEvents;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Foruscorp.Auth.Servises
{
    public class AuthService(
        UserContext context, 
        IConfiguration configuration,
        ITokenProvider tokenProvider,
        IPublishEndpoint publishEndpoint,
        IHttpContextAccessor httpContextAccessor) : IAuthService
    {

        public async Task LogoutAsync(bool allDevices = false)
        {
            var http = httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No HttpContext");

            var refreshCookie = http.Request.Cookies["refreshToken"];
            Guid? userId = null;

            if (!string.IsNullOrEmpty(refreshCookie))
            {
                var current = await context.RefreshTokens
                    .FirstOrDefaultAsync(t => t.Token == refreshCookie);

                if (current != null)
                {
                    userId = current.UserId;
                    if (!current.IsRevoked)
                    {
                        current.IsRevoked = true; 
                    }
                }
            }
    
            if (allDevices)
            {
              
                if (userId is null)
                {
                    var claim = http.User?.FindFirst(ClaimTypes.NameIdentifier)
                                ?? http.User?.FindFirst("sub");
                    if (claim != null && Guid.TryParse(claim.Value, out var parsed))
                        userId = parsed;
                }

                if (userId.HasValue)
                {

                    await context.RefreshTokens
                        .Where(t => t.UserId == userId.Value && !t.IsRevoked)
                        .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsRevoked, true));
//                var tokens = await context.RefreshTokens
//                    .Where(t => t.UserId == userId.Value && !t.IsRevoked)
//                    .ToListAsync();
//                foreach (var t in tokens) t.IsRevoked = true;
                }
            }

            await context.SaveChangesAsync();

            // 3) Удалить cookie (важно совпадение Path с тем, как устанавливали)
            http.Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = false,          // В продакшене: true (HTTPS)!
                SameSite = SameSiteMode.Lax,
                Path = "/Auth/refresh",  // тот же путь, что при установке
                Domain = null
            });
        }



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
            var refreshToken = await GenerateRefreshToken(user.Id);

            httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/Auth/refresh",
                Domain = null
            });

            return new LoginResponce { UserId = user.Id, Token = token };
        }

        public async Task<string> GenerateRefreshToken(Guid userId)
        {
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)), 
                ExpiresAt = DateTime.UtcNow.AddDays(7), 
                Created = DateTime.UtcNow,
                IsRevoked = false
            };

            context.RefreshTokens.Add(refreshToken);
            await context.SaveChangesAsync();

            return refreshToken.Token;
        }


        public async Task<bool> RevokeRefreshToken(string token)
        {
            var refreshToken = await context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
            if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.Now)
                return false;

            refreshToken.IsRevoked = true;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<RefreshTokenResponce> RefreshAsync()
        {
            var refreshToken = httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedAccessException("No refresh token provided");

            var refreshTokenEntity = await context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);
            if (refreshTokenEntity == null)
                throw new UnauthorizedAccessException("Invalid or expired refresh token");

            var user = await context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == refreshTokenEntity.UserId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            var newAccessToken = tokenProvider.Create(user);
            var newRefreshToken = await GenerateRefreshToken(user.Id);

            // Обновляем cookie
            httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/Auth/refresh",
                Domain = null
            });

            await RevokeRefreshToken(refreshToken);

            return new RefreshTokenResponce { Token = newAccessToken };
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
                UserName = user.UserName
            });

            return user;
        }
    }
}
