using Foruscorp.FuelStations.Domain.FuelStations;
using System;
using System.Collections.Generic;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    public class StopPlanInfo
    {
        public List<FuelStopPlan> StopPlan { get; set; } = new List<FuelStopPlan>();
        public FinishInfo Finish { get; set; } = new FinishInfo();
    }

    public class FinishInfo
    {
        public double RemainingFuelLiters { get; set; }
    }

    public class FuelStopPlan
    {
        public FuelStation Station { get; set; } = null!;
        public double StopAtKm { get; set; }
        public double RefillLiters { get; set; }
        public double CurrentFuelLiters { get; set; }
        public string RoadSectionId { get; set; } = string.Empty;
    }

    public class RequiredStationDto
    {
        public Guid StationId { get; set; }
        public double RefillLiters { get; set; }
    }

    public class FuelStationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Latitude { get; set; } = null!;
        public string Longitude { get; set; } = null!;
        public string Price { get; set; } = null!;
        public string? Discount { get; set; }
        public string? PriceAfterDiscount { get; set; }
        public bool IsAlgorithm { get; set; } 
        public string Refill { get; set; } = null!;
        public int StopOrder { get; set; }
        public string NextDistanceKm { get; set; } = null!;
        public string RoadSectionId { get; set; } = string.Empty;
        public double CurrentFuel { get; set; } = 0.0; 
    }

    public class RouteStopsForRoadInfo
    {
        public List<FuelStopPlan> StopPlan { get; set; } = new List<FuelStopPlan>();
        public List<FuelStationDto> StationsWithoutAlgorithm { get; set; } = new List<FuelStationDto>();
        public FinishInfo Finish { get; set; } = new FinishInfo();
    }
}
