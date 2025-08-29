using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Domain.RouteValidators
{
    public class FuelStationChange
    {
        public Guid FuelStationChangeId { get; set; }   
        public Guid RouteValidatorId { get; set; }
        public Guid Id { get; set; }
        public Guid FuelRouteStationId { get; set; }
        public FuelRouteStation FuelStation { get; set; }
        public double ForwardDistance { get; set; }
        public double Refill { get; set; }
        public double CurrentFuel { get; set; }
        public bool IsAlgo { get; set; }
        public bool IsManual { get; set; }

        public int StopOrder { get; set; }
        public double NextDistanceKm { get; set; }

        private FuelStationChange()
        {
                
        }

        public FuelStationChange(FuelRouteStation fuelRouteStation, double forwardDistance = default)
        {
            FuelStation = fuelRouteStation;
            Refill = Convert.ToDouble(fuelRouteStation.Refill);
            CurrentFuel = fuelRouteStation.CurrentFuel; 
            ForwardDistance = fuelRouteStation.ForwardDistance;
        }

        public static FuelStationChange CreateAlgo(FuelRouteStation fuelRouteStation)
        {
            var station = new FuelStationChange(fuelRouteStation);
            station.IsAlgo = true;
            station.Id = fuelRouteStation.FuelPointId;
            return station;
        }

        public static FuelStationChange CreateManual(FuelRouteStation fuelRouteStation)
        {
            var station = new FuelStationChange(fuelRouteStation);
            station.Id = fuelRouteStation.FuelPointId;
            station.IsManual = true;
            return station;
        }
    }
}
