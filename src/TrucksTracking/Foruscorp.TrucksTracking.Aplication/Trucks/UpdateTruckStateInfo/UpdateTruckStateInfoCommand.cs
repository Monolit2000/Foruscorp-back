using System;
using MediatR;
using Foruscorp.TrucksTracking.Domain.Trucks;

namespace Foruscorp.TrucksTracking.Aplication.Trucks.UpdateTruckStateInfo
{
    public class UpdateTruckStateInfoCommand : IRequest 
    {
        GeoPoint CurrentTruckLocation { get; set; }
       
        decimal NewFuelStatus { get; set; } 
    }
}
