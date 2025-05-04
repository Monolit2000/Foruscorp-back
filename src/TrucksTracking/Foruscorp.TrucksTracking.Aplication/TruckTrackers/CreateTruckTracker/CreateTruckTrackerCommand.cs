using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.CreateTruckTracker
{
    public class CreateTruckTrackerCommand : IRequest
    {
        public Guid TruckId { get; set; }

        public string ProviderTruckId { get; set; }
    }
}
