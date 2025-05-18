using System;
using Foruscorp.FuelStations.Domain.CompanyFuelPriceMultipliers;
using Foruscorp.FuelStations.Aplication.CompanyPriceMultipliers;

namespace Foruscorp.FuelStations.Aplication.CompanyFuelPriceMultipliers
{
    public static class CompanyPriceMultiplierMapper
    {
        public static CompanyPriceMultiplierDto ToDto(this CompanyFuelPriceMultiplier entity)
        {
            return new CompanyPriceMultiplierDto(
                entity.Id,
                entity.CompanyId,
                entity.PriceMultiplier,
                entity.CreatedAt,
                entity.ChangedAt
            );
        }

        public static CompanyFuelPriceMultiplier ToEntity(this CompanyPriceMultiplierDto dto)
        {
            return CompanyFuelPriceMultiplier.CreateNew(dto.CompanyId, dto.PriceMultiplier);
        }
    }
}
