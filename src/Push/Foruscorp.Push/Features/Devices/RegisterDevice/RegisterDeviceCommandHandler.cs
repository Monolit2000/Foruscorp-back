using Foruscorp.Push.Domain.Devices;
using Foruscorp.Push.Infrastructure.Database;
using MediatR;

namespace Foruscorp.Push.Features.Devices.RegisterDevice
{
    public class RegisterDeviceCommandHandler : IRequestHandler<RegisterDeviceCommand, Guid>
    {
        private readonly PushNotificationsContext context;
        public RegisterDeviceCommandHandler(PushNotificationsContext deviceRepository)
        {
            context = deviceRepository ?? throw new ArgumentNullException(nameof(deviceRepository));
        }
        public async Task<Guid> Handle(RegisterDeviceCommand request, CancellationToken cancellationToken)
        {
            var device = new Device(new ExpoPushToken(request.PushToken), request.UserId);
            await context.AddAsync(device, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return device.Id;
        }
    }
}
