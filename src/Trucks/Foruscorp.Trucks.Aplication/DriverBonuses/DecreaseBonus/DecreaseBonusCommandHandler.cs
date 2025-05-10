using MediatR;
using FluentResults;
using Foruscorp.Trucks.Aplication.Contruct;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Trucks.Aplication.DriverBonuses.DecreaseBonus
{
    public class DecreaseBonusCommandHandler(
        ITruckContext truckContext) : IRequestHandler<DecreaseBonusCommand, Result>
    {
        public async Task<Result> Handle(DecreaseBonusCommand request, CancellationToken cancellationToken)
        {
            var driver = truckContext.Drivers
                .FirstOrDefault(d => d.Id == request.DriverId);

            if (driver == null)
                return Result.Fail("Driver not found");

            var decreasesBonuse = driver.DecreaseBonus(request.Bonus, request.Reason);

            await truckContext.DriverBonuses.AddAsync(decreasesBonuse); 

            await truckContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
