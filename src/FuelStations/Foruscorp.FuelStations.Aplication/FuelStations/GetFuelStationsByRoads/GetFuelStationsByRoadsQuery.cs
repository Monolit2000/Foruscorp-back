using FluentResults;
using MediatR;
using static Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads.GetFuelStationsByRoadsQueryHandler;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    public class GetFuelStationsByRoadsQuery : IRequest<Result<List<FuelStationDto>>>
    {
        public List<RoadSectionDto> Roads { get; set; } = new List<RoadSectionDto>();

        public List<RequiredStationDto> RequiredFuelStations { get; set; } = new List<RequiredStationDto>();
    }


    public class RoadSectionDto
    {
        public string RoadSectionId { get; set; }
        public List<List<double>> Points { get; set; } = [];
    }   
}
