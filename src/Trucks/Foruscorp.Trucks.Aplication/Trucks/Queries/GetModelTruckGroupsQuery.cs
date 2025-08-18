using MediatR;

namespace Foruscorp.Trucks.Aplication.Trucks.Queries
{
    public class GetModelTruckGroupsQuery : IRequest<GetModelTruckGroupsResult>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
    }

    public class GetModelTruckGroupsResult
    {
        public List<ModelTruckGroupDto> ModelTruckGroups { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ModelTruckGroupDto
    {
        public Guid Id { get; set; }
        public string TruckGrouName { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public double AverageFuelConsumption { get; set; }
        public double AveregeWeight { get; set; }
        public double FuelCapacity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int TrucksCount { get; set; }
    }
}
