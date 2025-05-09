using MediatR;
using FluentResults;
using Foruscorp.Trucks.Aplication.Contruct;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Trucks.Aplication.DriverBonuses.IncreaseBonus
{
    internal class IncreaseBonusCommandHandler(
        ITruckContext truckContext) : IRequestHandler<IncreaseBonusCommand, Result>
    {
        public async Task<Result> Handle(IncreaseBonusCommand request, CancellationToken cancellationToken)
        {
            var driver = truckContext.Drivers
                .Include(d => d.Bonuses)
                .FirstOrDefault(d => d.Id == request.DriverId);

            if (driver == null)
                return Result.Fail("Driver not found");

            driver.IncreaseBonus(request.Bonus, request.Reason);

            await truckContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();    
        }
    }
}
