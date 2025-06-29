using Foruscorp.TrucksTracking.Aplication.Contruct.RealTimeTruckModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.Contruct.RealTime
{
    public interface ISignalRNotificationSender
    {
        Task SendTruckStatusUpdateAsync(TruckStausUpdate truckStausUpdate);
    }
}
