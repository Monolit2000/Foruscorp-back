using MediatR;
using FluentResults;

namespace Foruscorp.Trucks.Aplication.Trucks.GetTruckById
{
    public class GetTruckByIdQuery : IRequest<Result<TruckDto>>
    {
        public Guid TruckId { get; set; }
    }
}
