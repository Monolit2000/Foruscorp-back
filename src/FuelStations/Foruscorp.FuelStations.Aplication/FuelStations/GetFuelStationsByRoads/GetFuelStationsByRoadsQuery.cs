using FluentResults;
using MediatR;
using static Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads.GetFuelStationsByRoadsQueryHandler;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    public class GetFuelStationsByRoadsQuery : IRequest<Result<GetFuelStationsByRoadsResponce>>
    {
        public List<RoadSectionDto> Roads { get; set; } = new List<RoadSectionDto>();

        public List<RequiredStationDto> RequiredFuelStations { get; set; } = new List<RequiredStationDto>();

        public double FinishFuel { get; set; } = 40.0; 

        public List<string> FuelProviderNameList { get; set; } = new List<string>();

        public int CurrentFuel { get; set; } = 20; 

        public double Weight { get; set; } = 40000.0; // in Paunds
    }


    public class RoadSectionDto
    {
        public string RoadSectionId { get; set; }
        public List<List<double>> Points { get; set; } = [];
    }   
}
