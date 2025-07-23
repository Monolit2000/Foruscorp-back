using FluentResults;
using MediatR;
using System.Text.Json.Serialization;

namespace Foruscorp.Push.Features.Devices.RegisterDevice
{
    public record RegisterDeviceCommand(Guid? UserId, string PushToken) : IRequest<Result<Guid>>;
}
