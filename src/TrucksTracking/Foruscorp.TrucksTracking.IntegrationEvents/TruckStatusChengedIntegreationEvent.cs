using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.IntegrationEvents
{
    public class TruckStatusChengedIntegreationEvent 
    {
        public Guid TruckId { get; set; }
    }
}
