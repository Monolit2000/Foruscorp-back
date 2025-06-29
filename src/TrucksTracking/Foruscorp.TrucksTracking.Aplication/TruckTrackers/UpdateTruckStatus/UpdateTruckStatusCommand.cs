using MassTransit.Configuration;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckStatus
{
    public class UpdateTruckStatusCommand : IRequest
    {
        public Guid TruckId { get; set; }   
        public string EngineStatus { get; set; }    
    }
}
