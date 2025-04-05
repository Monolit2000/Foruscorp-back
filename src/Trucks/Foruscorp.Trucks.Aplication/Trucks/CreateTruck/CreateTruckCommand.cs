using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Foruscorp.Trucks.Aplication.Trucks.CreateTruck
{
    public class CreateTruckCommand : IRequest<TruckDto>
    {
        public string Ulid { get; set; }
        public string LicensePlate { get; set; }
    }
}
