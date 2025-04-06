using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.Trucks.Domain.Drivers;

namespace Foruscorp.Trucks.Aplication.Trucks
{
    public class DriverDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Status { get; set; }

        public DriverDto(Guid id, string fullName, string status)
        {
            Id = id;
            FullName = fullName;
            Status = status;    
        }

    }
}
