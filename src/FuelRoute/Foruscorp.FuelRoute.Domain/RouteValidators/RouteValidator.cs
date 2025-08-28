using Foruscorp.FuelRoutes.Domain.FuelRoutes;

namespace Foruscorp.FuelRoutes.Domain.RouteValidators
{
    public class RouteValidator
    {
        public Guid Id { get; set; }
        public FuelRoute FuelRoute { get; set; }
        
        public bool IsValid { get; set; }

        public FuelRouteSection FuelRouteSection { get; set; }

        public List<FuelStationChange> FuelStationChanges { get; set; }

        private RouteValidator()
        {
                
        }

        public RouteValidator(FuelRoute fuelRoute, FuelRouteSection fuelRouteSection )
        {
            Id = Guid.NewGuid();
            InitFuelStationChanges(fuelRouteSection.FuelRouteStations);
            FuelRouteSection = fuelRouteSection;
            fuelRoute = FuelRoute;
            IsValid = true; 
        }

        public void MarkAsInvalid()
        {
            IsValid = false;
        }

        public void AddFuelStationChange(FuelStationChange fuelStationChange)
        {
            if (fuelStationChange == null)
                throw new ArgumentNullException(nameof(fuelStationChange));

            fuelStationChange.RouteValidatorId = Id;
            FuelStationChanges.Add(fuelStationChange);
        }

        public void RemoveFuelStationChange(Guid fuelStationChangeId)
        {
            FuelStationChanges.RemoveAll(fsc => fsc.FuelRouteStationId == fuelStationChangeId);
        }

        private void InitFuelStationChanges(List<FuelRouteStation> fuelRouteStations)
        {
            if (fuelRouteStations?.Any() == true)
            {
                var algorithmicStations = fuelRouteStations
                    .Where(fs => fs.IsAlgorithm)
                    .Select(fs => FuelStationChange.CreateAlgo(fs))
                    .ToList();

                foreach (var station in algorithmicStations)
                {
                    station.RouteValidatorId = Id;
                    FuelStationChanges.Add(station);
                }
            }
        }
    }
}
