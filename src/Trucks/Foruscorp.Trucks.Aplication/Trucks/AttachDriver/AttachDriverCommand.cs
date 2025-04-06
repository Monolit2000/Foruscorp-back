using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

using FluentResults;

namespace Foruscorp.Trucks.Aplication.Trucks.AttachDriver
{
    public class AttachDriverCommand : IRequest<Result>
    {
        public Guid TruckId { get; set; }
        public Guid DriverId { get; set; }
        public AttachDriverCommand(Guid truckId, Guid driverId)
        {
            TruckId = truckId;
            DriverId = driverId;
        }   
    }
}
