using MediatR;
using FluentResults;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    public class GetFuelStationsByRoadsQuery : IRequest<Result<List<FuelStationDto>>>
    {
        public List<Road> Roads { get; set; } = new List<Road>();   
    }


    public class Road
    {
        public string Id { get; set; }
        public List<List<double>> Points { get; set; } = [];
    }   
}
