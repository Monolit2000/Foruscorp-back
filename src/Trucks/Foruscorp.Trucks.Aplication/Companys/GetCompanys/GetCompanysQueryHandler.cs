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
                                .Include(c => c.CompanyManagers)
                                    .ThenInclude(cm => cm.User)
                                        .ThenInclude(u => u.Contact)
                                .Select(c => new 
                                {
                                    Company = c,
                                    DriverCount = c.Drivers.Count,
                                    TruckCount = c.Trucks.Count
                                }).ToListAsync(cancellationToken);

            if (companys is null || !companys.Any())
                return new List<CompanyDto>();

            var companyDtos = companys
                .Select(c => c.Company.ToCompanyDto(c.DriverCount,c.TruckCount))
                .ToList();

            return companyDtos;
        }
    }
}
