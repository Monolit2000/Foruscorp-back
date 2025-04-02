using MediatR;

namespace Foruscorp.TrucksTracking.Aplication.Trucks.UpdateTruckStateInfo
{
    public class UpdateTruckStateInfoCommandHandler() : IRequestHandler<UpdateTruckStateInfoCommand>
    {
        public Task Handle(UpdateTruckStateInfoCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
