using Foruscorp.Users.IntegrationEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Foruscorp.Push.Features.Devices.DeleteDevice
{
    public class UserMobileLogoutIntegrationEventHandler : IConsumer<UserLogoutIntegrationEvent>
    {
        private readonly ISender sender;
        private readonly ILogger<UserMobileLogoutIntegrationEventHandler> logger;

        public UserMobileLogoutIntegrationEventHandler(
            ISender sender,
            ILogger<UserMobileLogoutIntegrationEventHandler> logger)
        {
            this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<UserLogoutIntegrationEvent> context)
        {
            var message = context.Message;
            
            logger.LogInformation(
                "User {UserId} logged out at {LogoutAt}. Deleting device with token {ExpoToken}.", 
                message.UserId, 
                message.LogoutAt,
                message.ExpoToken);

            if (string.IsNullOrEmpty(message.ExpoToken))
            {
                logger.LogWarning("Expo token is empty for user {UserId}, skipping device deletion", message.UserId);
                return;
            }

            try
            {
                await sender.Send(new DeleteDeviceCommand(message.ExpoToken), context.CancellationToken);
                logger.LogInformation("Device with token {ExpoToken} successfully deleted for user {UserId}", 
                    message.ExpoToken, message.UserId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete device with token {ExpoToken} for user {UserId}", 
                    message.ExpoToken, message.UserId);
                throw;
            }
        }
    }
}
