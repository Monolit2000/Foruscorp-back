using MediatR;
using Microsoft.EntityFrameworkCore;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Aplication.CompanyPriceMultipliers;

namespace Foruscorp.FuelStations.Aplication.CompanyFuelPriceMultipliers.GetAllCompanyPriceMultipliers
{
    public class GetAllCompanyPriceMultipliersQueryHandler(
        IFuelStationContext fuelStationContext) : IRequestHandler<GetAllCompanyPriceMultipliersQuery, IEnumerable<CompanyPriceMultiplierDto>>
    {
        public async Task<IEnumerable<CompanyPriceMultiplierDto>> Handle(GetAllCompanyPriceMultipliersQuery request, CancellationToken cancellationToken)
        {
            var companyFuelPriceMultipliers = await fuelStationContext.CompanyPriceMultipliers.ToListAsync(cancellationToken);
            return companyFuelPriceMultipliers.Select(x => x.ToDto());
        }
    }   
}
