using MediatR;
using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route.ApiClients;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.DropPiont
{
    public class DropPiontCommandHandler(
        ITruckerPathApi truckerPathApi) : IRequestHandler<DropPiontCommand, Result<SimpleDropPointResponse>>
    {
        public async Task<Result<SimpleDropPointResponse>> Handle(DropPiontCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var responce = await truckerPathApi.DropPoint(request.latitude, request.longitude, request.level, request.radius);

                return Result.Ok(responce); 
            }
            catch (Exception ex)
            {

                return Result.Fail(ex.Message);
            }
        }
    }
}
