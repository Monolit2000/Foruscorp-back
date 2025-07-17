using System;
using MediatR;
using Foruscorp.TrucksTracking.Domain.Trucks;
using Foruscorp.TrucksTracking.Aplication.Contruct.RealTimeTruckModels;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTracker
{
    public class UpdateTruckTrackerCommand : IRequest 
    {
        //public Guid TruckId { get; set; }

        public TruckInfoUpdate TruckInfoStatsUpdate { get; set; }
    }
}
