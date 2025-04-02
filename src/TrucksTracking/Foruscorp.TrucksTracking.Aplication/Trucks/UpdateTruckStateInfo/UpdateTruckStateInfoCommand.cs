using System;
using MediatR;
using Foruscorp.TrucksTracking.Domain.Trucks;

namespace Foruscorp.TrucksTracking.Aplication.Trucks.UpdateTruckStateInfo
{
    public class UpdateTruckStateInfoCommand : IRequest 
    {
        public Guid TruckId { get; set; }

        public GeoPoint CurrentTruckLocation { get; set; }

        public decimal FuelStatus { get; set; } 
    }
}
