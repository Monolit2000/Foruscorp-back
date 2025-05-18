using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.FuelStations.Aplication.CompanyPriceMultipliers;
using MediatR;

namespace Foruscorp.FuelStations.Aplication.CompanyFuelPriceMultipliers.GetAllCompanyPriceMultipliers
{
    public class GetAllCompanyPriceMultipliersQuery : IRequest<IEnumerable<CompanyPriceMultiplierDto>>
    {
    }
}
