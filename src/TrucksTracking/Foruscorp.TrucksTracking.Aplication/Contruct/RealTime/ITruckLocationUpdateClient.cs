using Foruscorp.TrucksTracking.Aplication.Contruct.RealTimeTruckModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.Contruct.RealTime
{
    public interface ITruckLocationUpdateClient
    {
        Task ReceiveTruckLocationUpdate(TruckLocationUpdate truckLocationUpdate);
        Task ReceiveTruckFuelUpdate(TruckFuelUpdate truckLocationUpdate);
        Task ReceiveTruckStatusUpdate(TruckStausUpdate truckLocationUpdate);
    }
}
