using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads.GetFuelStationsByRoadsQueryHandler;
using FuelStationDto = Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads.FuelStationDto;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.GenerateFuelStations
{
    public class GetFuelStationsCommand : IRequest<Result<List<FuelStationDto>>>
    { 
        public Guid RouteId { get; set; }
        public List<string> RouteSectionIds { get; set; }
        public List<RequiredStationDto> RequiredFuelStations { get; set; } = new List<RequiredStationDto>();    
    }
}
