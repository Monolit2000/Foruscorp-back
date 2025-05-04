using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using Foruscorp.Trucks.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Trucks.Aplication.Trucks.LoadTrucks
{
    public class LoadTrucksCommandHandler(
        ITuckContext tuckContext,
        ITruckProviderService truckProviderService) : IRequestHandler<LoadTrucksCommand, Result<TruckDto>>
    {
        public async Task<Result<TruckDto>> Handle(LoadTrucksCommand request, CancellationToken cancellationToken)
        {
            var responce = await truckProviderService.GetVehiclesAsync(cancellationToken);

            var truks = responce.Data
                .Select(vehicle => vehicle.ToTruck())
                .ToList();

            if (!truks.Any() || truks.FirstOrDefault() == null)
                return Result.Fail("Empty responce");

            if (await tuckContext.Trucks.AnyAsync())
            {
                await tuckContext.Trucks.ExecuteDeleteAsync(cancellationToken);
                await tuckContext.SaveChangesAsync(cancellationToken);
            }

            await tuckContext.Trucks.AddRangeAsync(truks, cancellationToken);

            await tuckContext.SaveChangesAsync(cancellationToken);

            throw new NotImplementedException();
        }
    }
}
