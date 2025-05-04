using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers
{
    public class TruckTrackerDto
    {
        public Guid TrackerId { get; set; }

        public Guid TruckId { get; set; }

        public string ProviderTruckId { get; set; }

        public string Status { get; set; } 
        
    }
}
