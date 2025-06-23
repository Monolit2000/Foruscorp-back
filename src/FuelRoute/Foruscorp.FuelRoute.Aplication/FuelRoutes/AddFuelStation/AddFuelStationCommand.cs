using FluentResults;
using Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads;
using Foruscorp.FuelStations.Domain.FuelStations;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads.GetFuelStationsByRoadsQueryHandler;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.AddFuelStation
{
    public class AddFuelStationCommand : IRequest<Result<List<FuelStationDto>>>
    {
        public Guid RouteId { get; set; }
        public List<string> RouteSectionIds { get; set; }
        public List<RequiredStationDto> RequiredFuelStations { get; set; } = new List<RequiredStationDto>();
    }
}
