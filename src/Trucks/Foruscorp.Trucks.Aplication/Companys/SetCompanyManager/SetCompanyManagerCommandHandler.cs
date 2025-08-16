using FluentResults;
using Foruscorp.Trucks.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Companys.SetCompanyManager
{
    public record SetCompanyManagerCommand(Guid UserId, Guid CompanyId) : IRequest<Result>;
    public class SetCompanyManagerCommandHandler(ITruckContext truckContext) : IRequestHandler<SetCompanyManagerCommand, Result>
    {
        public async Task<Result> Handle(SetCompanyManagerCommand request, CancellationToken cancellationToken)
        {
            var company = await truckContext.Companys
                .FirstOrDefaultAsync(c => c.Id == request.CompanyId);

            if (company == null)
                return Result.Fail("Company not found.");

            var IsUserExisst = await truckContext.Users
                .AnyAsync(u => u.UserId == request.UserId, cancellationToken);

            if (!IsUserExisst)
                return Result.Fail("User not found.");

            var manager = company.AddManager(request.UserId);

            await truckContext.SaveChangesAsync(cancellationToken);

            return Result.Ok().WithSuccess(manager.Id.ToString());
        }
    }
}
