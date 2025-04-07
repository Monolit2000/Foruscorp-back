using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.GetAllTruckTrackers
{
    public class GetAllTruckTrackersQuery : IRequest<List<TruckTrackerDto>>
    {
    }
}
