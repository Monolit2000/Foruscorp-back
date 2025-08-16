using MediatR;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Foruscorp.Trucks.Domain.Trucks;
using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Aplication.Trucks.Commands;

namespace Foruscorp.Trucks.Aplication.ModelTruckGroups.FormModelTruckGroupsForAllTrucks
{

    public class FormModelTruckGroupForAllTruksCommand : IRequest<Result>
    {
        public Guid TruckId { get; set; }

    }



    public class FormModelTruckGroupsForAllTrucksCommandHandler
       : IRequestHandler<FormModelTruckGroupForAllTruksCommand, Result>
    {
        private readonly ITruckContext _truckContext;
        private readonly ILogger<FormModelTruckGroupsForAllTrucksCommandHandler> _logger;

        public FormModelTruckGroupsForAllTrucksCommandHandler(
            ITruckContext truckContext,
            ILogger<FormModelTruckGroupsForAllTrucksCommandHandler> logger)
        {
            _truckContext = truckContext;
            _logger = logger;
        }

        public async Task<Result> Handle(FormModelTruckGroupForAllTruksCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var errors = new List<string>();
                int processedTrucks = 0;
                int createdGroups = 0;
                int updatedGroups = 0;

                var trucks = await _truckContext.Trucks
                    .Include(t => t.ModelTruckGroup)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Starting to process {TruckCount} trucks for ModelTruckGroup formation", trucks.Count);

                foreach (var truck in trucks)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(truck.Make) ||
                            string.IsNullOrWhiteSpace(truck.Model) ||
                            string.IsNullOrWhiteSpace(truck.Year))
                        {
                            errors.Add($"Truck {truck.Id}: Missing Make, Model, or Year");
                            continue;
                        }

                        var groupName = $"{truck.Make} {truck.Model} {truck.Year}";

                        // Если грузовик уже привязан к группе и не требуется принудительное пересоздание
                        if (truck.ModelTruckGroup != null)
                        {
                            _logger.LogDebug("Truck {TruckId} already has ModelTruckGroup {GroupId}",
                                truck.Id, truck.ModelTruckGroup.Id);
                            continue;
                        }

                        // Ищем существующую группу
                        var existingGroup = await _truckContext.ModelTruckGroups
                            .FirstOrDefaultAsync(m => m.TruckGrouName == groupName, cancellationToken);

                        ModelTruckGroup modelTruckGroup;

                        if (existingGroup == null)
                        {
                            // Создаем новую группу
                            modelTruckGroup = new ModelTruckGroup(truck.Make, truck.Model, truck.Year);
                            _truckContext.ModelTruckGroups.Add(modelTruckGroup);
                            createdGroups++;

                            _logger.LogInformation("Created new ModelTruckGroup: {GroupName} for Truck: {TruckId}",
                                groupName, truck.Id);
                        }
                        else
                        {
                            modelTruckGroup = existingGroup;
                            updatedGroups++;

                            if(truck.ModelTruckGroup == null)
                                truck.SetModelTruckGroup(modelTruckGroup);

                            _logger.LogInformation("Found existing ModelTruckGroup: {GroupName} for Truck: {TruckId}",
                                groupName, truck.Id);
                        }

                        // Привязываем грузовик к группе
                        truck.SetModelTruckGroup(modelTruckGroup);

                        await _truckContext.SaveChangesAsync(cancellationToken);
                        processedTrucks++;
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = $"Error processing truck {truck.Id}: {ex.Message}";
                        errors.Add(errorMessage);
                        _logger.LogError(ex, "Error processing truck {TruckId}", truck.Id);
                    }
                }

                // Сохраняем все изменения
                if (processedTrucks > 0)
                {
                    await _truckContext.SaveChangesAsync(cancellationToken);
                }

                _logger.LogInformation("Completed processing trucks. Processed: {Processed}, Created: {Created}, Updated: {Updated}",
                    processedTrucks, createdGroups, updatedGroups);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while forming ModelTruckGroups for all trucks");
                return Result.Fail($"An error occurred: {ex.Message}");
            }
        }
    }
}
