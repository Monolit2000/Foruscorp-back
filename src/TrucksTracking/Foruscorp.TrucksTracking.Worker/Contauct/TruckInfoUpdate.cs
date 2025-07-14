using Foruscorp.TrucksTracking.Worker.Services;

namespace Foruscorp.TrucksTracking.Worker.Contauct
{
    public sealed record TruckInfoUpdate(
      string TruckId,
      string TruckName,
      double Longitude,
      double Latitude,
      string Time,
      double HeadingDegrees,
      double fuelPercents,
      string formattedLocation,
      EngineStateData engineStateData);
}
