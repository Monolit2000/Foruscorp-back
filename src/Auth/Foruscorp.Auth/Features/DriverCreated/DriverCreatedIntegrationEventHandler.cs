using Foruscorp.Trucks.IntegrationEvents;
using Foruscorp.Auth.Contruct;
using Foruscorp.Auth.Domain.Users;
using Foruscorp.Auth.DataBase;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Foruscorp.Auth.Features.DriverCreated
{
    public class DriverCreatedIntegrationEventHandler(
        UserContext userContext,
        IUserService userService) : IConsumer<DriverCreatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<DriverCreatedIntegrationEvent> context)
        {
            var message = context.Message;
            
            var existingUser = await userContext.Users
                .FirstOrDefaultAsync(u => u.Id == message.UserId, context.CancellationToken);
            
            if (existingUser == null)
            {
                var email = !string.IsNullOrWhiteSpace(message.Email) 
                    ? message.Email 
                    : $"driver_{message.UserId}@trucks.local";
                
                var username = !string.IsNullOrWhiteSpace(message.Name) 
                    ? message.Name.Replace(" ", "_").ToLowerInvariant() 
                    : $"driver_{message.UserId}";
                
                var user = User.CreateNew(email, username);

                user.SetDriverRole();   

                var passwordHasher = new PasswordHasher<User>();
                user.PasswordHash = passwordHasher.HashPassword(user, "1234");
                
                await userContext.Users.AddAsync(user, context.CancellationToken);
                await userContext.SaveChangesAsync(context.CancellationToken);
                
                await userService.SetUserRole(user.Id, UserRoleType.Driver.ToString());
            }
            else
            {
                await userService.SetUserRole(message.UserId, UserRoleType.Driver.ToString());
            }
        }
    }
}
