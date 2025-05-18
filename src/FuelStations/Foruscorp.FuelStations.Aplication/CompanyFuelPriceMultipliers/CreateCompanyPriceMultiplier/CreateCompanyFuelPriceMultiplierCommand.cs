using MediatR;

namespace Foruscorp.FuelStations.Aplication.CompanyPriceMultipliers.CompanyFuelPriceMultipliers
{
    public class CreateCompanyFuelPriceMultiplierCommand : IRequest<CompanyPriceMultiplierDto>
    {
        public Guid CompanyId { get; set; }
        public double PriceMultiplier { get; set; }
        public CreateCompanyFuelPriceMultiplierCommand(
            Guid companyId, 
            double priceMultiplier)
        {
            CompanyId = companyId;
            PriceMultiplier = priceMultiplier;
        }   
    }
}
