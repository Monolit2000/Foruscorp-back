using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Trucks.UpdateTruckStatus
{
    public class UpdateTruckStatusCommand : IRequest
    {
        public Guid TruckId { get; set; }
        public int Status { get; set; }
    }
}
