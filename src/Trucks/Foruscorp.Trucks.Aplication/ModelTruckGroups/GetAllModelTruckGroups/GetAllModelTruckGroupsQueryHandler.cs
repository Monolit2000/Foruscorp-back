using Foruscorp.Trucks.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.ModelTruckGroups.GetAllModelTruckGroups
{
    public record GetAllModelTruckGroupsQuery : IRequest<List<ModelTruckGroupsDto>>;

    public class GetAllModelTruckGroupsQueryHandler(ITruckContext truckContext) : IRequestHandler<GetAllModelTruckGroupsQuery, List<ModelTruckGroupsDto>>
    {
        public async Task<List<ModelTruckGroupsDto>> Handle(GetAllModelTruckGroupsQuery request, CancellationToken cancellationToken)
        {
            var modelTruckGroups = await truckContext.ModelTruckGroups
                .Include(mtg => mtg.Trucks)
                .AsNoTracking()
                .Select(mtg => new ModelTruckGroupsDto
                {
                    Id = mtg.Id,
                    TruckGroupName = mtg.TruckGrouName,
                    Weight = mtg.AveregeWeight,
                    FuelCapacity = mtg.FuelCapacity,
                    TrucksCount = mtg.Trucks.Count
                })
                .ToListAsync(cancellationToken);

            return modelTruckGroups;
        }
    }

    public class ModelTruckGroupsDto
    {
        public Guid Id { get; set; }
        public string TruckGroupName { get; set; }
        public double Weight { get; set; }
        public double FuelCapacity { get; set; } 
        public int TrucksCount { get; set; } 
    }
}
