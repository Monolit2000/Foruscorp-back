using MediatR;
using Microsoft.EntityFrameworkCore;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Domain.CompanyFuelPriceMultipliers;
using Foruscorp.FuelStations.Aplication.CompanyFuelPriceMultipliers;

namespace Foruscorp.FuelStations.Aplication.CompanyPriceMultipliers.CompanyFuelPriceMultipliers
{
    internal class CreateCompanyFuelPriceMultiplierCommandHandler(
        IFuelStationContext fuelStationContext) : IRequestHandler<CreateCompanyFuelPriceMultiplierCommand, CompanyPriceMultiplierDto>
    {
        public async Task<CompanyPriceMultiplierDto> Handle(CreateCompanyFuelPriceMultiplierCommand request, CancellationToken cancellationToken)
        {
            var isCompanyPriceMultiplierDtoExist = await fuelStationContext.CompanyPriceMultipliers
                .AnyAsync(x => x.CompanyId == request.CompanyId, cancellationToken);

            if (isCompanyPriceMultiplierDtoExist)
                throw new Exception("Company Price Multiplier already exists");

            var companyPriceMultiplier = CompanyFuelPriceMultiplier.CreateNew(request.CompanyId, request.PriceMultiplier);

            await fuelStationContext.CompanyPriceMultipliers.AddAsync(companyPriceMultiplier, cancellationToken);

            await fuelStationContext.SaveChangesAsync();

            return companyPriceMultiplier.ToDto();  
        }
    }   
}
