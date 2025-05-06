using System;
using MediatR;
using Foruscorp.TrucksTracking.Domain.Trucks;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTracker
{
    public class UpdateTruckTrackerCommand : IRequest 
    {
        public Guid TruckId { get; set; }

        public GeoPoint CurrentTruckLocation { get; set; }

        public decimal FuelStatus { get; set; } 
    }
}
