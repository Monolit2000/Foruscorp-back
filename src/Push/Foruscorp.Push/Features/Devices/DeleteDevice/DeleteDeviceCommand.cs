using FluentResults;
using MediatR;

namespace Foruscorp.Push.Features.Devices.DeleteDevice
{
    public record DeleteDeviceCommand(string ExpoToken) : IRequest<Result>;
}
