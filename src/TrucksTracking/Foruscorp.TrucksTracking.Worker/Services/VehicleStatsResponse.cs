namespace Foruscorp.TrucksTracking.Worker.Services
{
    public class VehicleStatsResponse
    {
        public VehicleStat[] Data { get; set; }
        public Pagination Pagination { get; set; }
    }

    public class VehicleStat
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ExternalIds ExternalIds { get; set; }
        public List<GpsData> Gps { get; set; }
        public List<FuelData> FuelPercents { get; set; }
        public List<EngineStateData> EngineStates { get; set; }
    }

    public class GpsData
    {
        public string Time { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double HeadingDegrees { get; set; }
        public double SpeedMilesPerHour { get; set; }
        public ReverseGeo ReverseGeo { get; set; }
        public bool IsEcuSpeed { get; set; }
    }

    public class FuelData
    {
        public string Time { get; set; }
        public int Value { get; set; }
    }

    public class EngineStateData
    {
        public string Time { get; set; }
        public string Value { get; set; } // "Off", "On", "Idle"
    }

    public class ReverseGeo
    {
        public string FormattedLocation { get; set; }
    }


    public class VehicleResponse
    {
        public Vehicle[] Data { get; set; }
        public Pagination Pagination { get; set; }
    }

    public class Vehicle
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Vin { get; set; }
        public string Serial { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Year { get; set; }
        public string LicensePlate { get; set; }
        public string Notes { get; set; }
        public string HarshAccelerationSettingType { get; set; }
        public DateTime CreatedAtTime { get; set; }
        public DateTime UpdatedAtTime { get; set; }
        public ExternalIds ExternalIds { get; set; }
    }

    public class ExternalIds
    {
        public string SamsaraSerial { get; set; }
        public string SamsaraVin { get; set; }
    }

    public class Pagination
    {
        public string EndCursor { get; set; }
        public bool HasNextPage { get; set; }
    }

}
