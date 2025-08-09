using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Foruscorp.Trucks.Aplication.Contruct;

namespace Foruscorp.Trucks.Aplication.Companys.GetCompanyById
{
    public record GetCompanyByIdQuery(Guid CompanyId) : IRequest<Result<CompanyDto>>;
    public class GetCompanyByIdQueryHandler(
        ITruckContext truckContext) : IRequestHandler<GetCompanyByIdQuery, Result<CompanyDto>>
    {
        public async Task<Result<CompanyDto>> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
        {
            var company = await truckContext.Companys
                .AsNoTracking()
                .Include(c => c.Drivers)
                .Include(c => c.Trucks)
                .Include(c => c.CompanyManagers)
                    .ThenInclude(cm => cm.User)
                        .ThenInclude(u => u.Contact)
                .FirstOrDefaultAsync(c => c.Id == request.CompanyId);
            if (company == null)
                return Result.Fail("Company not found.");

            var companyDto = company.ToCompanyDto();    
            return Result.Ok(companyDto);
        }
    }
}
