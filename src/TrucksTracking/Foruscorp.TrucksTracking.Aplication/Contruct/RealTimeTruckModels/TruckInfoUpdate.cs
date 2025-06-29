using Foruscorp.TrucksTracking.Aplication.Contruct.TruckProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.Contruct.RealTimeTruckModels
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
