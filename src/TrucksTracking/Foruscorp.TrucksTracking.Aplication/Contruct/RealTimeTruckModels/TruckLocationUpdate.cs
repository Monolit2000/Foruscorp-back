using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.Contruct.RealTimeTruckModels
{
    public sealed record TruckLocationUpdate(
      string TruckId,
      string TruckName,
      double Longitude,
      double Latitude,
      string Time,
      double HeadingDegrees);
}
