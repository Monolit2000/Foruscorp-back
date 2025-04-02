using Foruscorp.TrucksTracking.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.TrucksTracking.Aplication.Trucks.UpdateTruckStateInfo
{
    public class UpdateTruckStateInfoCommandHandler(
        ITuckTrackingContext tuckTrackingContext) : IRequestHandler<UpdateTruckStateInfoCommand>
    {
        public async Task Handle(UpdateTruckStateInfoCommand request, CancellationToken cancellationToken)
        {
            var truck = await tuckTrackingContext.Trucks.FirstOrDefaultAsync(t => t.Id == request.TruckId, cancellationToken);

            if (truck == null)
                return;
            
            truck.UpdateTruck(request.CurrentTruckLocation, request.FuelStatus);

            await tuckTrackingContext.SaveChangesAsync(cancellationToken);  
        }
    }
}
