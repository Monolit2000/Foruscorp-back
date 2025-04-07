using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using Foruscorp.TrucksTracking.Domain.Trucks;
using MediatR;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.ActivateTruckTracker
{
    //public class ActivateTruckTrackerCommand : IRequest<TruckTrackerDto>
    //{
    //    public Guid TrackerId { get; set; }
    //}

    public record ActivateTruckTrackerCommand(Guid TruckId) : IRequest<Result<TruckTrackerDto>>;
}
