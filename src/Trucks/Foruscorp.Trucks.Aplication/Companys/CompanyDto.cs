using Foruscorp.Trucks.Aplication.Drivers;
using Foruscorp.Trucks.Aplication.Trucks;
using Foruscorp.Trucks.Domain.Companys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Companys
{
    public class CompanyDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public int DriversCount { get; set; }
        public int TrucksCount { get; set; }
        //public DateTime UpdatedAt { get; set; }

        //public List<TruckDto> Trucks { get; set; } = new();
        //public List<DriverDto> Drivers { get; set; } = new();
    }

    public static class CompanyMapper
    {
        public static CompanyDto ToCompanyDto(this Company company)
        {
            return new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                //Cnpj = company.Cnpj,
                //Email = company.Email,
                //Phone = company.Phone,
                //Address = company.Address,
                CreatedAt = company.CreatedAt,
                DriversCount = company.Drivers.Count,
                TrucksCount = company.Trucks.Count,
                //UpdatedAt = company.UpdatedAt,
                //Trucks = company.Trucks.Select(t => new TruckDto
                //{
                //    Id = t.Id,
                //    LicensePlate = t.LicensePlate,
                //    Model = t.Model,
                //    Brand = t.Brand
                //}).ToList(),
                //Drivers = company.Drivers.Select(d => new DriverDto
                //{
                //    Id = d.Id,
                //    FullName = d.FullName,
                //    LicenseNumber = d.LicenseNumber
                //}).ToList()
            };
        }


        public static CompanyDto ToCompanyDto(this Company company, int driversCount, int trucksCount)
        {
            return new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                //Cnpj = company.Cnpj,
                //Email = company.Email,
                //Phone = company.Phone,
                //Address = company.Address,
                CreatedAt = company.CreatedAt,
                DriversCount = driversCount,
                TrucksCount = trucksCount,
                //UpdatedAt = company.UpdatedAt,
                //Trucks = company.Trucks.Select(t => new TruckDto
                //{
                //    Id = t.Id,
                //    LicensePlate = t.LicensePlate,
                //    Model = t.Model,
                //    Brand = t.Brand
                //}).ToList(),
                //Drivers = company.Drivers.Select(d => new DriverDto
                //{
                //    Id = d.Id,
                //    FullName = d.FullName,
                //    LicenseNumber = d.LicenseNumber
                //}).ToList()
            };
        }
    }
}
