using MediatR;
using FluentResults;
using Foruscorp.FuelStations.Aplication.Contructs.WebScrapers;

namespace Foruscorp.FuelStations.Aplication.FuelStations.RefreshFuelStationDataByRadius
{
    public class RefreshFuelStationDataByRadiusCommandHandler(
        IFuelStationsService fuelStationsService) : IRequestHandler<RefreshFuelStationDataByRadiusCommand, Result>
    {
        public Task<Result> Handle(RefreshFuelStationDataByRadiusCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
