using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Foruscorp.Trucks.Aplication.Contruct;

namespace Foruscorp.Trucks.Aplication.Companys.GetCompanys
{
    public record GetCompanysQuery() : IRequest<Result<List<CompanyDto>>>;
    public class GetCompanysQueryHandler(
        ITruckContext truckContext) : IRequestHandler<GetCompanysQuery, Result<List<CompanyDto>>>
    {
        public async Task<Result<List<CompanyDto>>> Handle(GetCompanysQuery request, CancellationToken cancellationToken)
        {
            var companys = await truckContext.Companys
                                .AsNoTracking()
                                .Select(c => new 
                                {
                                    company = c,
                                    DriverCount = c.Drivers.Count,
                                    TruckCount = c.Trucks.Count
                                }).ToListAsync(cancellationToken);

            if (companys is null || !companys.Any())
                throw new KeyNotFoundException("No companies found.");

            var companyDtos = companys
                .Select(c => c.company.ToCompanyDto(c.DriverCount,c.TruckCount))
                .ToList();

            return companyDtos;
        }
    }
}
