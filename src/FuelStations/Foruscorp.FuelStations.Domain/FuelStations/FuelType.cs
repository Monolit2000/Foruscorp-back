using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Domain.FuelStations
{
    public record FuelType(string Name)
    {
        public static FuelType Gasoline92 => new("Gasoline 92");
        public static FuelType Gasoline95 => new("Gasoline 95");
        public static FuelType Diesel => new("Diesel");
    }
}
