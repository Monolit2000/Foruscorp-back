using Foruscorp.TrucksTracking.Aplication.Contruct.RealTimeTruckModels;
using MassTransit.Observables;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTrackerIfChanged
{
    public class UpdateTruckTrackerIfChangedCommand : IRequest
    {
        public List<TruckInfoUpdate> TruckStatsUpdates { get; set; }    
    }
}
