//using Foruscorp.FuelRoutes.Aplication.Contruct;
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.GetFuelRoute
//{
//    public record GetFuelRoute(Guid RouteId, Guid RouteSectionId) : IRequest<FuelRouteDto>;

//    public class GetFuelRouteQueryHandler(
//        IFuelRouteContext fuelRouteContext) : IRequestHandler<GetFuelRoute, FuelRouteDto>
//    {
//        public async Task<FuelRouteDto> Handle(GetFuelRoute request, CancellationToken cancellationToken)
//        {

//            var fuelRoad = await fuelRouteContext.FuelRoutes
//                .Include(x => x.FuelRouteStations)
//                .Include(x => x.RouteSections)
//                .FirstOrDefaultAsync(x => x.Id == request.RouteId, cancellationToken);



//            return fuelRoute.ToDto();
//        }
//    }
//}
