namespace Foruscorp.TrucksTracking.Worker.Domain
{
    public class TruckTracker
    {
        public Guid TruckId { get; private set; }
        public string ProviderTruckId { get; private set; }

        private TruckTracker() { }

        public TruckTracker(
            Guid truckId, 
            string providerTruckId)
        {
            TruckId = truckId;
            ProviderTruckId = providerTruckId;
        }

        public void UpdateTruckTracker(Guid truckId, string providerTruckId)
        {
            TruckId = truckId;
            ProviderTruckId = providerTruckId;
        }
    }
}
