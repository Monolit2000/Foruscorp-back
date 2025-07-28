using FluentResults;
using Foruscorp.Push.Domain.Devices;
using Foruscorp.Push.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Push.Features.Devices.RegisterDevice
{
    public class RegisterDeviceCommandHandler : IRequestHandler<RegisterDeviceCommand, Result<Guid>>
    {
        private readonly PushNotificationsContext context;
        public RegisterDeviceCommandHandler(PushNotificationsContext deviceRepository)
        {
            context = deviceRepository ?? throw new ArgumentNullException(nameof(deviceRepository));
        }
        public async Task<Result<Guid>> Handle(RegisterDeviceCommand request, CancellationToken cancellationToken)
        {
            var existedUserDevice = await context.Devices
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (existedUserDevice != null && existedUserDevice.Token.Value == request.PushToken) 
                return Result.Fail(new Error("Device with this token already registered."));

            var device = new Device(new ExpoPushToken(request.PushToken), request.UserId);
            await context.AddAsync(device, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return device.Id;
        }
    }
}
