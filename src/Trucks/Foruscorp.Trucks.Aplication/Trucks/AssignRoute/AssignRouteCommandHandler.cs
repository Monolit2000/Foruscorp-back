using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Domain.RouteOffers;
using Foruscorp.Trucks.IntegrationEvents;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Trucks.Aplication.Trucks.AssignRoute
{
    public record AssignRouteCommand(Guid RouteId, Guid TruckId) : IRequest;
    public class AssignRouteCommandHandler(
        ITruckContext context,
        IPublishEndpoint publishEndpoint) : IRequestHandler<AssignRouteCommand>
    {
        public async Task Handle(AssignRouteCommand request, CancellationToken cancellationToken)
        {
            var truck = await context.Trucks
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(x => x.Id == request.TruckId);

            if (truck == null)
                throw new ArgumentException($"Truck with ID {request.TruckId} not found.");

            var routeOffer = RouteOffer.CreateNew(
                truck.Driver.Id,
                request.RouteId);

            await context.RouteOffers.AddAsync(routeOffer);

            await context.SaveChangesAsync(cancellationToken);

            await publishEndpoint.Publish(new RouteOfferedIntegrationEvent(truck.Driver.UserId.Value, request.RouteId));
        }
    }
}
