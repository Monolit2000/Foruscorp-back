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
            var IsTokenExist = await context.Devices
                .AnyAsync(d => d.Token.Value == request.PushToken, cancellationToken);

            if(IsTokenExist)
                return Result.Fail(new Error("Device with this token already registered."));

            var device = new Device(new ExpoPushToken(request.PushToken), request.UserId);
            await context.AddAsync(device, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return device.Id;
        }
    }
}
