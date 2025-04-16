using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using MediatR;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.DeactivateTruckTracker
{
    public class DeactivateTruckTrackerCommand : IRequest<Result>
    {
        public Guid TruckId { get; set; }
    }
}
