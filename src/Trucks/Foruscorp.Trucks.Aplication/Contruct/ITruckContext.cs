using Foruscorp.Trucks.Domain.Companys;
using Foruscorp.Trucks.Domain.DriverFuelHistorys;
using Foruscorp.Trucks.Domain.Drivers;
using Foruscorp.Trucks.Domain.RouteOffers;
using Foruscorp.Trucks.Domain.Trucks;
using Foruscorp.Trucks.Domain.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Contruct
{
    public interface ITruckContext : IDisposable
    {
        DbSet<Truck> Trucks { get; set; }
        DbSet<Driver> Drivers { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<DriverBonus> DriverBonuses { get; set; }
        DbSet<RouteOffer> RouteOffers { get; set; }
        DbSet<Company> Companys { get; set; }

        DbSet<ModelTruckGroup> ModelTruckGroups { get; set; }
        DbSet<DriverFuelHistory> DriverFuelHistories { get; set; }

        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
