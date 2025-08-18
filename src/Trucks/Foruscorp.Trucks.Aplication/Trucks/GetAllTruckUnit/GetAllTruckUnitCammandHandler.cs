using Foruscorp.Trucks.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Trucks.GetAllTruckUnit
{
    public class TruckUnitDto
    {
        public Guid TruckId { get; set; }
        public string Unit { get; set; }
    }

    public record GetAllTruckUnitCammand() : IRequest<List<TruckUnitDto>>;
    public class GetAllTruckUnitCammandHandler(
        ICurrentUser currentUser,
        ITruckContext truckContext) : IRequestHandler<GetAllTruckUnitCammand, List<TruckUnitDto>>
    {
        public async Task<List<TruckUnitDto>> Handle(GetAllTruckUnitCammand request, CancellationToken cancellationToken)
        {
            var trucks = await truckContext.Trucks
                .Where(t => t.CompanyId == currentUser.CompanyId)
                .Select(t => new TruckUnitDto
                {
                    TruckId = t.Id,
                    Unit = t.Name,
                })
                .ToListAsync(cancellationToken);   
            
            if (trucks == null || !trucks.Any())
                return new List<TruckUnitDto>();

            return trucks;
        }
    }
}
