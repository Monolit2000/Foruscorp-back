using FluentResults;
using Foruscorp.Push.Domain.Devices;
using Foruscorp.Push.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Foruscorp.Push.Features.Devices.DeleteDevice
{
    public class DeleteDeviceCommandHandler : IRequestHandler<DeleteDeviceCommand, Result>
    {
        private readonly PushNotificationsContext context;
        private readonly ILogger<DeleteDeviceCommandHandler> logger;

        public DeleteDeviceCommandHandler(
            PushNotificationsContext deviceRepository,
            ILogger<DeleteDeviceCommandHandler> logger)
        {
            context = deviceRepository ?? throw new ArgumentNullException(nameof(deviceRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result> Handle(DeleteDeviceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var device = await context.Devices
                    .FirstOrDefaultAsync(d => d.Token.Value == request.ExpoToken, cancellationToken);

                if (device == null)
                {
                    logger.LogWarning("Device with token {ExpoToken} not found for deletion", request.ExpoToken);
                    return Result.Fail($"Device with token {request.ExpoToken} not found");
                }

                device.Deactivate();
                //context.Devices.Remove(device);
                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Device with token {ExpoToken} successfully deleted", request.ExpoToken);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting device with token {ExpoToken}", request.ExpoToken);
                return Result.Fail($"Error deleting device: {ex.Message}");
            }
        }
    }
}
