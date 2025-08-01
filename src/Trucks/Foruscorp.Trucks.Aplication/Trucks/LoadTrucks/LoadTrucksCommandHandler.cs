﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Domain.Trucks;
using Foruscorp.Trucks.IntegrationEvents;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Trucks.Aplication.Trucks.LoadTrucks
{
    public class LoadTrucksCommandHandler(
        ITruckContext tuckContext,
        IPublishEndpoint publishEndpoint,
        ITruckProviderService truckProviderService) : IRequestHandler<LoadTrucksCommand, Result<List<TruckDto>>>
    {
        public async Task<Result<List<TruckDto>>> Handle(LoadTrucksCommand request, CancellationToken cancellationToken)
        {
            var responce = await truckProviderService.GetVehiclesAsync(cancellationToken);

            var truks = responce.Data
                .Select(vehicle => vehicle.ToTruck())
                .ToList();

            if (!truks.Any() || truks.FirstOrDefault() == null)
                return Result.Fail("Empty responce");


            var truckEntity = await tuckContext.Trucks
                .AsNoTracking()
                .Where(t => truks.Select(x => x.ProviderTruckId).ToList().Contains(t.ProviderTruckId))
                .ToListAsync(cancellationToken);

            if(truckEntity.Any())
            {
                tuckContext.Trucks.UpdateRange(truckEntity);
                await tuckContext.SaveChangesAsync(cancellationToken);
                await PublishToMassageBrocker(truckEntity);

                //return truckEntity.Select(t => t.ToTruckDto()).ToList();

                truks = truks.Where(t => !truckEntity.Select(te => te.ProviderTruckId).Contains(t.ProviderTruckId)).ToList();
            }

            if (!truks.Any())
                return truks.Select(t => t.ToTruckDto()).ToList();

            await tuckContext.Trucks.AddRangeAsync(truks, cancellationToken);

            await tuckContext.SaveChangesAsync(cancellationToken);

            await PublishToMassageBrocker(truks);

            return truks.Select(t => t.ToTruckDto()).ToList();
        }


        private async Task DeleteAllTrucksIfAnyExist(CancellationToken cancellationToken = default)
        {
            if (!await tuckContext.Trucks.AnyAsync())
                return;
            await tuckContext.Trucks.ExecuteDeleteAsync(cancellationToken);
            await tuckContext.SaveChangesAsync(cancellationToken);
        }   

        private async Task PublishToMassageBrocker(List<Truck> trucks)
        {

            foreach (var truck in trucks)
            {
                await publishEndpoint.Publish(new TruckCreatedIntegrationEvent()
                {
                    TruckId = truck.Id,
                    ProviderTruckId = truck.ProviderTruckId,    
                });
            }
        }
    }
}
