using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Domain.FuelRoutes
{
    public record RouteSectionInfo(double Tolls, double Gallons, double Miles, int DriveTime);
}
