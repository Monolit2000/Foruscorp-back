using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Drivers
{
    public class DriverDto
    {
        public Guid Id { get; set; }
        public Guid TruckId { get; set; }   
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int Bonus { get; set; }
        public string TelegramLink { get; set; }
    }

    public class SmallTruckDto
    {
        
    }
}
