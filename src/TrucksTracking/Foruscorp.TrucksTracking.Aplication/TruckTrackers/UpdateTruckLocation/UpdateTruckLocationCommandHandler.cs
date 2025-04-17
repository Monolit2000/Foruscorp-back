using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckLocation
{
    public class UpdateTruckLocationCommandHandler : IRequestHandler<UpdateTruckLocationCommand>
    {
        public Task Handle(UpdateTruckLocationCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }   
 
}
