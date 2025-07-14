using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.FuelStations.CheckNearFuelStation
{
    public record CheckNearFuelStationCommand(Guid TruckId, double Longitude, double Latitude) : IRequest;
    public class CheckNearFuelStationCommandHandler : IRequestHandler<CheckNearFuelStationCommand>
    {
        public async Task Handle(CheckNearFuelStationCommand request, CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
        }
    }
}
