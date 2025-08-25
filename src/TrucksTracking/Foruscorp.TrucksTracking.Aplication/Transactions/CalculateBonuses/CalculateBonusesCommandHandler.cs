using MediatR;
using FluentResults;
using Foruscorp.TrucksTracking.Aplication.Contruct;

namespace Foruscorp.TrucksTracking.Aplication.Transactions.CalculateBonuses
{
    public record CalculateBonusesCommand() : IRequest<Result>;
    public class CalculateBonusesCommandHandler(
        ITruckTrackingContext truckTrackingContext) : IRequestHandler<CalculateBonusesCommand, Result>
    {
        public Task<Result> Handle(CalculateBonusesCommand request, CancellationToken cancellationToken)
        {
            var nearStationPlans = truckTrackingContext.NearFuelStationPlans.ToList();

            var truckIds = nearStationPlans.Select(n => n.TruckId).Distinct().ToList();
            var fuelStationIds = nearStationPlans.Select(n => n.FuelStationId).Distinct().ToList();

            var trunsaction = truckTrackingContext.Transactions.ToList();

            foreach (var transaction in trunsaction)
            {

            }

            throw new NotImplementedException();
        }
    }
}
