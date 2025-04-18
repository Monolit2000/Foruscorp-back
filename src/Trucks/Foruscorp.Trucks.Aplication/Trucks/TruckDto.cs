﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.Trucks.Domain.Drivers;

namespace Foruscorp.Trucks.Aplication.Trucks
{
    public class TruckDto
    {
        public Guid Id { get; set; }
        public string Ulid { get; set; }
        public string LicensePlate { get; set; }
        public string Status { get; set; }
        public Guid? DriverId { get; set; }

        public DriverDto Driver { get; set; }  
    }
}
