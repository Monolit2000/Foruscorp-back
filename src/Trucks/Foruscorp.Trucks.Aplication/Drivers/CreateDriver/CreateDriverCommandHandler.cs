using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Domain.Drivers;
using MediatR;

namespace Foruscorp.Trucks.Aplication.Drivers.CreateDriver
{
    public class CreateDriverCommandHandler(ITruckContext context) : IRequestHandler<CreateDriverCommand, DriverDto>
    {
        public async Task<DriverDto> Handle(CreateDriverCommand request, CancellationToken cancellationToken)
        {
            var driver = Driver.CreateNew(request.Name);

            await context.Drivers.AddAsync(driver, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            return new DriverDto() 
            {
                Id = driver.Id,
                FullName = driver.FullName
            };

        }
    }   
}
