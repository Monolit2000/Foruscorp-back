using Foruscorp.Trucks.IntegrationEvents;
using Foruscorp.Auth.Contruct;
using Foruscorp.Auth.Domain.Users;
using Foruscorp.Auth.DataBase;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Foruscorp.Auth.Features.CompanyManagerCreated
{
    public class CompanyManagerCreatedIntegrationEventHandler(
        UserContext userContext,
        IUserService userService) : IConsumer<CompanyManagerCreatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<CompanyManagerCreatedIntegrationEvent> context)
        {
            var message = context.Message;
            
            var existingUser = await userContext.Users
                .FirstOrDefaultAsync(u => u.Id == message.UserId, context.CancellationToken);
            
            if (existingUser == null)
            {
                var email = !string.IsNullOrWhiteSpace(message.Email)
                    ? message.Email
                    : $"manager_{message.UserId}@company.local";

                var username = !string.IsNullOrWhiteSpace(message.Name)
                    ? message.Name.Replace(" ", "_").ToLowerInvariant()
                    : $"manager_{message.UserId}";

                var user = User.CreateCompanyManager(message.UserId, email, username);

                var passwordHasher = new PasswordHasher<User>();
                user.PasswordHash = passwordHasher.HashPassword(user, "1234");
                
                await userContext.Users.AddAsync(user, context.CancellationToken);
                await userContext.SaveChangesAsync(context.CancellationToken);
            }
            
            await userService.SetUserRole(message.UserId, UserRoleType.Admin.ToString());
        }
    }
}
