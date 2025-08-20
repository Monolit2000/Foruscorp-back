using Foruscorp.Trucks.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.ModelTruckGroups.GetModelTruckGroupById
{
    public record GetModelTruckGroupByIdQuery(Guid Id) : IRequest<ModelTruckGroupByIdDto?>;

    public class GetModelTruckGroupByIdQueryHandler(ITruckContext truckContext) : IRequestHandler<GetModelTruckGroupByIdQuery, ModelTruckGroupByIdDto?>
    {
        public async Task<ModelTruckGroupByIdDto?> Handle(GetModelTruckGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var modelTruckGroup = await truckContext.ModelTruckGroups
                .Include(mtg => mtg.Trucks)
                .AsNoTracking()
                .Where(mtg => mtg.Id == request.Id)
                .Select(mtg => new ModelTruckGroupByIdDto
                {
                    Id = mtg.Id,
                    TruckGroupName = mtg.TruckGrouName,
                    Make = mtg.Make,
                    Model = mtg.Model,
                    Year = mtg.Year,
                    AverageFuelConsumption = mtg.AverageFuelConsumption,
                    Weight = mtg.AveregeWeight,
                    FuelCapacity = mtg.FuelCapacity,
                    CreatedAt = mtg.CreatedAt,
                    UpdatedAt = mtg.UpdatedAt,
                    TrucksCount = mtg.Trucks.Count
                })
                .FirstOrDefaultAsync(cancellationToken);

            return modelTruckGroup;
        }
    }

    public class ModelTruckGroupByIdDto
    {
        public Guid Id { get; set; }
        public string TruckGroupName { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Year { get; set; }
        public double AverageFuelConsumption { get; set; }
        public double Weight { get; set; }
        public double FuelCapacity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int TrucksCount { get; set; }
    }
}
