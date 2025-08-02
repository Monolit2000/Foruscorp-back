using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Foruscorp.Trucks.Aplication.Contruct;

namespace Foruscorp.Trucks.Aplication.Trucks.SetCompany
{
    public record SetCompanyCommand(Guid TruckId ,Guid CompanyId) : IRequest<Result>;
    public class SetCompanyCommandHandler(
        ITruckContext truckContext) : IRequestHandler<SetCompanyCommand, Result>
    {
        public async Task<Result> Handle(SetCompanyCommand request, CancellationToken cancellationToken)
        {
            var truck = await truckContext.Trucks
                .FirstOrDefaultAsync(t => t.Id == request.TruckId);

            if (truck == null)
                return Result.Fail("Truck not found.");

            var company = await truckContext.Companys
                .FirstOrDefaultAsync(c => c.Id == request.CompanyId);

            if(company == null)
                return Result.Fail("Company not found.");

            truck.SetCompany(company.Id);

            await truckContext.SaveChangesAsync(cancellationToken); 

            return Result.Ok().WithSuccess("Company set successfully for the truck.");  
        }
    }
}
