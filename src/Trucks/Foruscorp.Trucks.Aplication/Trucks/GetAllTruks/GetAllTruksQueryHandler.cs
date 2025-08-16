using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.Trucks.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Trucks.Aplication.Trucks.GetAllTruks
{
    public class GetAllTruksQueryHandler(
        ICurrentUser currentUser,
        ITruckContext truckContext) : IRequestHandler<GetAllTruksQuery, List<TruckDto>>
    {
        public async Task<List<TruckDto>> Handle(GetAllTruksQuery request, CancellationToken cancellationToken)
        {
            var truks = await truckContext.Trucks
                .Include(t => t.Driver)
                    .ThenInclude(d => d.DriverUser)
                        .ThenInclude(u => u.Contact)
                .AsNoTracking()
                .Where(t => currentUser.CompanyId == t.CompanyId ) 
                .ToListAsync(cancellationToken);

            if(!truks.Any())
                return new List<TruckDto>();

            var truksDto = truks
                .Select(t => t.ToTruckDto())
                .ToList();

            return truksDto;
        }
    }
}
