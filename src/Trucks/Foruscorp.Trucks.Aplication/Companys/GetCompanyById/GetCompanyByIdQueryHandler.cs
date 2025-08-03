using FluentResults;
using Foruscorp.Trucks.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Companys.GetCompanyById
{
    public record GetCompanyByIdQuery(Guid CompanyId) : IRequest<Result<CompanyDto>>;
    public class GetCompanyByIdQueryHandler(
        ITruckContext truckContext) : IRequestHandler<GetCompanyByIdQuery, Result<CompanyDto>>
    {
        public async Task<Result<CompanyDto>> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
        {
            var company = await truckContext.Companys.FirstOrDefaultAsync(c => c.Id == request.CompanyId);
            if (company == null)
                return Result.Fail("Company not found.");

            var companyDto = company.ToCompanyDto();    
            return Result.Ok(companyDto);
        }
    }
}
