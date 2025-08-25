using FluentResults;
using Foruscorp.Trucks.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Foruscorp.Trucks.Aplication.DriverBonuses.CalculateBonuses
{
    public record CalculateBonusesCommand(List<Guid> TransactionIds) : IRequest<Result>;

    public class CalculateBonusesCommandHandler(
        ITruckContext truckContext,
        ITruckTrackingService truckTrackingService,
        ILogger<CalculateBonusesCommandHandler> logger) : IRequestHandler<CalculateBonusesCommand, Result>
    {
        private readonly ILogger<CalculateBonusesCommandHandler> _logger = logger;

        public async Task<Result> Handle(CalculateBonusesCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting CalculateBonuses for {TransactionCount} transactions", request.TransactionIds.Count);

            var transactions = await truckContext.Transactions
                .Where(t => request.TransactionIds.Contains(t.Id) && !t.IsProcessed)
                .Include(t => t.Fills)
                    .ThenInclude(f => f.Items)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {TransactionCount} unprocessed transactions", transactions.Count);

            var fills = transactions.SelectMany(t => t.Fills);

            foreach (var fill in fills)
            {
                var unitNumber = fill.GetUnitNumber();
                _logger.LogInformation("Processing fill with UnitNumber {UnitNumber}", unitNumber);

                var truck = await truckContext.Trucks
                    .Include(t => t.ModelTruckGroup)
                    .FirstOrDefaultAsync(t => t.Name.Contains(unitNumber), cancellationToken);

                if (truck == null)
                {
                    _logger.LogWarning("No truck found for UnitNumber {UnitNumber}", unitNumber);
                    continue;
                }

                var tankCapacity = truck?.ModelTruckGroup?.FuelCapacity ?? 200;
                var trunsactionTime = fill.GetTransactionDateTime();
                var quantity = fill.Items.FirstOrDefault()?.Quantity ?? 0;

                _logger.LogInformation("Truck {TruckId} at {TrunsactionTime}, Quantity: {Quantity}, TankCapacity: {TankCapacity}",
                    truck.Id, trunsactionTime, quantity, tankCapacity);

                var bonusIndex = await truckTrackingService.GetNearFuelStationBonusAsync(
                    truck.Id,
                    trunsactionTime,
                    quantity,
                    tankCapacity,
                    cancellationToken);

                _logger.LogInformation("Calculated BonusIndex {BonusIndex} for Truck {TruckId}", bonusIndex, truck.Id);

                var trunsactionTimeUtc = TimeZoneInfo.ConvertTimeToUtc(trunsactionTime,
                    TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

                var truckUsage = await truckContext.TruckUsages
                    .Where(u => u.TruckId == truck.Id &&
                                u.StartedAt <= trunsactionTimeUtc &&
                                (u.EndedAt == null || u.EndedAt >= trunsactionTimeUtc))
                    .FirstOrDefaultAsync();

                if (truckUsage == null)
                {
                    _logger.LogWarning("No TruckUsage found for Truck {TruckId} at {TrunsactionTimeUtc}", truck.Id, trunsactionTimeUtc);
                    continue;
                }

                switch (bonusIndex)
                {
                    case 0:
                        truckUsage.Driver.IncreaseBonus(0, "Wrong stop");
                        _logger.LogInformation("Assigned 0 points to Driver {DriverId} for wrong stop", truckUsage.Driver.Id);
                        break;
                    case 1:
                        truckUsage.Driver.IncreaseBonus(50, "Correct stop with incorrect fuel amount");
                        _logger.LogInformation("Assigned 50 points to Driver {DriverId} for correct stop with incorrect fuel amount", truckUsage.Driver.Id);
                        break;
                    case 2:
                        truckUsage.Driver.IncreaseBonus(100, "Correct stop with correct fuel amount");
                        _logger.LogInformation("Assigned 100 points to Driver {DriverId} for correct stop with correct fuel amount", truckUsage.Driver.Id);
                        break;
                    default:
                        _logger.LogWarning("Unknown bonusIndex {BonusIndex} for Truck {TruckId}", bonusIndex, truck.Id);
                        break;
                }

                await truckContext.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Finished CalculateBonuses");

            return Result.Ok();
        }
    }
}
