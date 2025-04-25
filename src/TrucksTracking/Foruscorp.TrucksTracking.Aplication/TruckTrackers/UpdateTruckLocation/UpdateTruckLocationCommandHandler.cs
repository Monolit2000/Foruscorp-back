using Foruscorp.TrucksTracking.Domain.Trucks;
using MediatR;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckLocation
{
    public class UpdateTruckLocationCommandHandler() : IRequestHandler<UpdateTruckLocationCommand>
    {
        public Task Handle(UpdateTruckLocationCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }   
 
}
