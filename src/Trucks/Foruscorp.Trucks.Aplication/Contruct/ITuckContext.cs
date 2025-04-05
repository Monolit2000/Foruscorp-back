using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.Trucks.Domain.DriverFuelHistorys;
using Foruscorp.Trucks.Domain.Drivers;
using Foruscorp.Trucks.Domain.Trucks;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Trucks.Aplication.Contruct
{
    public interface ITuckContext : IDisposable
    {
        DbSet<Truck> Trucks { get; set; }

        DbSet<Driver> Drivers { get; set; }

        DbSet<DriverBonus> DriverBonuses { get; set; }

        DbSet<DriverFuelHistory> DriverFuelHistories { get; set; }

        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
