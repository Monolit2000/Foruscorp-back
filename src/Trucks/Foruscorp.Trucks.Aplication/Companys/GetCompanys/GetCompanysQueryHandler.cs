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
                .ToListAsync(cancellationToken);
            if (companys is null || !companys.Any())
                throw new KeyNotFoundException("No companies found.");

            var companyDtos = companys
                .Select(c => c.ToCompanyDto())
                .ToList();

            return companyDtos;
        }
    }
}
