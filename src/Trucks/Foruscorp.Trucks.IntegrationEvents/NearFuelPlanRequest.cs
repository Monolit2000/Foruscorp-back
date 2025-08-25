using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.IntegrationEvents
{
    public class NearFuelPlanRequest
    {
        public Guid TruckId { get; set; }
        public DateTime TransactionTime { get; set; }
        public double Quantity { get; set; }
        public double TankCapacity { get; set; } = 200;
    }
}
