using Foruscorp.Push.Domain.Devices;
using Foruscorp.Push.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Push.Features.Devices.UpdateDeviceToken
{
    public record UpdateDeviceTokenCommand(Guid DeviceId, string NewPushToken) : IRequest<Guid>;
    public class UpdateDeviceTokenCommandHandler : IRequestHandler<UpdateDeviceTokenCommand, Guid>
    {
        private readonly PushNotificationsContext _db;
        public UpdateDeviceTokenCommandHandler(PushNotificationsContext db) => _db = db;

        public async Task<Guid> Handle(UpdateDeviceTokenCommand cmd, CancellationToken ct)
        {
            var device = await _db.Devices.FirstOrDefaultAsync(d => d.Id == cmd.DeviceId)
                          ?? throw new KeyNotFoundException("Device not found");

            device.UpdateToken(new ExpoPushToken(cmd.NewPushToken));
            await _db.SaveChangesAsync(ct);
            return device.Id;
        }
    }
}
