using FluentResults;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.IntegrationEvents;
using MediatR;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationByIdQuery
{
    public record GetFuelStationByIdQuery(Guid FuelStationId) : IRequest<Result<FuelStationIntegrationDto>>;
 
    public class GetFuelStationByIdQueryHandler(
        IFuelStationContext fuelStationContext) : IRequestHandler<GetFuelStationByIdQuery, Result<FuelStationIntegrationDto>>
    {
        public async Task<Result<FuelStationIntegrationDto>> Handle(GetFuelStationByIdQuery request, CancellationToken cancellationToken)
        {
            var fuelStation = await fuelStationContext.FuelStations.FirstOrDefaultAsync(fs => fs.Id == request.FuelStationId);

            if (fuelStation == null)
            {
                return Result.Fail("Not found");
            }

            var fuelStationDto = new FuelStationIntegrationDto
            {
                Id = fuelStation.Id,
                FuelStationProviderId = fuelStation.FuelStationProviderId,
                ProviderName = fuelStation.ProviderName,
                Latitude = fuelStation.Coordinates.Latitude,
                Longitude = fuelStation.Coordinates.Longitude,
            };

            return fuelStationDto;
        }
    }
}
