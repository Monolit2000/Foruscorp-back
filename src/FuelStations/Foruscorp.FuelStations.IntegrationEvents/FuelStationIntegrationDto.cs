namespace Foruscorp.FuelStations.IntegrationEvents
{
    public class FuelStationIntegrationDto
    {   
        public Guid Id { get; set; }
        public string FuelStationProviderId { get; set; }
        public string ProviderName { get; set; }
        public string Address { get; set; }
        public string FuelProvider { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
