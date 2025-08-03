using Foruscorp.Trucks.Domain.Drivers;
using Foruscorp.Trucks.Domain.Trucks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Domain.Companys
{
    public class Company
    {
        public Guid Id { get; set; }
        public List<Truck> Trucks { get; private set; } = [];
        public List<Driver> Drivers { get; private set; } = [];

        public string ExternalToken { get; set; }

        public string Name { get; set; }
        public string Cnpj { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Company()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public static Company Create(string name)
        {
            return new Company
            {
                Name = name,
            };
        }

        public static Company Create(string name, string token)
        {
            return new Company
            {
                Name = name,
                ExternalToken = token
            };
        }
    }
}
