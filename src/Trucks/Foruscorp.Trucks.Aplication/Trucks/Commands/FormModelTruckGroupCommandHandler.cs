using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Domain.Trucks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Foruscorp.Trucks.Aplication.Trucks.Commands
{
    public class FormModelTruckGroupCommandHandler : IRequestHandler<FormModelTruckGroupCommand, FormModelTruckGroupResult>
    {
        private readonly ITruckContext _truckContext;
        private readonly ILogger<FormModelTruckGroupCommandHandler> _logger;

        public FormModelTruckGroupCommandHandler(
            ITruckContext truckContext,
            ILogger<FormModelTruckGroupCommandHandler> logger)
        {
            _truckContext = truckContext;
            _logger = logger;
        }

        public async Task<FormModelTruckGroupResult> Handle(FormModelTruckGroupCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Получаем грузовик
                var truck = await _truckContext.Trucks.FindAsync(request.TruckId);
                if (truck == null)
                {
                    return FormModelTruckGroupResult.FailureResult($"Truck with ID {request.TruckId} not found");
                }

                // Проверяем, что у грузовика есть необходимые данные
                if (string.IsNullOrWhiteSpace(truck.Make) || 
                    string.IsNullOrWhiteSpace(truck.Model) || 
                    string.IsNullOrWhiteSpace(truck.Year))
                {
                    return FormModelTruckGroupResult.FailureResult("Truck must have Make, Model, and Year specified");
                }

                // Формируем название группы
                var groupName = $"{truck.Make} {truck.Model} {truck.Year}";

                // Ищем существующую группу
                var existingGroup = await _truckContext.ModelTruckGroups
                    .FirstOrDefaultAsync(m => m.TruckGrouName == groupName, cancellationToken);

                ModelTruckGroup modelTruckGroup;
                bool isNewGroup = false;

                if (existingGroup == null)
                {
                    // Создаем новую группу
                    modelTruckGroup = new ModelTruckGroup(truck.Make, truck.Model, truck.Year);
                    _truckContext.ModelTruckGroups.Add(modelTruckGroup);
                    isNewGroup = true;

                    _logger.LogInformation("Created new ModelTruckGroup: {GroupName} for Truck: {TruckId}", 
                        groupName, truck.Id);
                }
                else
                {
                    modelTruckGroup = existingGroup;
                    _logger.LogInformation("Found existing ModelTruckGroup: {GroupName} for Truck: {TruckId}", 
                        groupName, truck.Id);
                }

                // Привязываем грузовик к группе
                truck.SetModelTruckGroup(modelTruckGroup);

                // Сохраняем изменения
                await _truckContext.SaveChangesAsync(cancellationToken);

                return FormModelTruckGroupResult.SuccessResult(modelTruckGroup.Id, isNewGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while forming ModelTruckGroup for Truck: {TruckId}", request.TruckId);
                return FormModelTruckGroupResult.FailureResult($"An error occurred: {ex.Message}");
            }
        }
    }
}
