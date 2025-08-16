using Foruscorp.Trucks.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Foruscorp.Trucks.Aplication.Trucks.Queries
{
    public class GetModelTruckGroupsQueryHandler : IRequestHandler<GetModelTruckGroupsQuery, GetModelTruckGroupsResult>
    {
        private readonly ITruckContext _truckContext;
        private readonly ILogger<GetModelTruckGroupsQueryHandler> _logger;

        public GetModelTruckGroupsQueryHandler(
            ITruckContext truckContext,
            ILogger<GetModelTruckGroupsQueryHandler> logger)
        {
            _truckContext = truckContext;
            _logger = logger;
        }

        public async Task<GetModelTruckGroupsResult> Handle(GetModelTruckGroupsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _truckContext.ModelTruckGroups.AsQueryable();

                // Применяем поиск, если указан
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    query = query.Where(m => 
                        m.TruckGrouName.ToLower().Contains(searchTerm) ||
                        m.Make.ToLower().Contains(searchTerm) ||
                        m.Model.ToLower().Contains(searchTerm) ||
                        m.Year.ToLower().Contains(searchTerm));
                }

                // Получаем общее количество
                var totalCount = await query.CountAsync(cancellationToken);

                // Применяем пагинацию
                var skip = (request.Page - 1) * request.PageSize;
                var modelTruckGroups = await query
                    .Skip(skip)
                    .Take(request.PageSize)
                    .Select(m => new ModelTruckGroupDto
                    {
                        Id = m.Id,
                        TruckGrouName = m.TruckGrouName,
                        Make = m.Make,
                        Model = m.Model,
                        Year = m.Year,
                        AverageFuelConsumption = m.AverageFuelConsumption,
                        AveregeWeight = m.AveregeWeight,
                        FuelCapacity = m.FuelCapacity,
                        CreatedAt = m.CreatedAt,
                        UpdatedAt = m.UpdatedAt,
                        TrucksCount = m.Trucks.Count
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                return new GetModelTruckGroupsResult
                {
                    ModelTruckGroups = modelTruckGroups,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting ModelTruckGroups");
                throw;
            }
        }
    }
}
