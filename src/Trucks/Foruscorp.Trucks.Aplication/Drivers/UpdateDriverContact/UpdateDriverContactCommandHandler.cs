﻿using MediatR;
using MassTransit.Mediator;
using Foruscorp.Trucks.Aplication.Contruct;
using Microsoft.EntityFrameworkCore;
using FluentResults;

namespace Foruscorp.Trucks.Aplication.Drivers.UpdateDriverContact
{
    public class UpdateDriverContactCommandHandler(
        ITruckContext truckContext) : IRequestHandler<UpdateDriverContactCommand, Result<DriverDto>>
    {
        public async Task<Result<DriverDto>> Handle(UpdateDriverContactCommand request, CancellationToken cancellationToken)
        {
            var driver = await truckContext.Drivers
                .FirstOrDefaultAsync(d => d.Id == request.DriverId, cancellationToken);

            if (driver == null)
                return Result.Fail("Driver not found");

            driver.UpdateContact(request.Phone, request.Email, request.TelegramLink);

            await truckContext.SaveChangesAsync(cancellationToken);

            return driver.ToDriverDto(); 
        }
    }
}
