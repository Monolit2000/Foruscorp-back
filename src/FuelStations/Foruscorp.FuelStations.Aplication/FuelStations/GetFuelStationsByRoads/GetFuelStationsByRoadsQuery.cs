using MediatR;
using FluentResults;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    public class GetFuelStationsByRoadsQuery : IRequest<Result<List<FuelStationDto>>>
    {
        public List<RoadSectionDto> Roads { get; set; } = new List<RoadSectionDto>();
    }


    public class RoadSectionDto
    {
        public string RoadSectionId { get; set; }
        public List<List<double>> Points { get; set; } = [];
    }   
}
