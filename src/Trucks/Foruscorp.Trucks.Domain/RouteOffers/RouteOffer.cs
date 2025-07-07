using Foruscorp.BuildingBlocks.Domain;
using Foruscorp.Trucks.Domain.Drivers;

namespace Foruscorp.Trucks.Domain.RouteOffers
{
    public class RouteOffer : Entity
    {
        public Guid Id { get; private set; }
        public Guid RouteId { get; private set; } 
        public Guid? TruckId { get; private set; }
        public Guid DriverId { get; private set; }
        public Driver Driver { get; private set; }
        public string Description { get; private set; }
        public RouteOfferStatus Status { get; private set; }
        public RouteOfferType Type { get; private set; }
        public DateTime CreatedAt { get; private set; } 
        private RouteOffer() { }

        private RouteOffer(Guid driverId, Guid routeId, string description = null)   
        {
            Id = Guid.NewGuid();
            DriverId = driverId;
            Description = description;
            Status = RouteOfferStatus.Pending;
            CreatedAt = DateTime.UtcNow;    
            RouteId = routeId;
        }

        public static RouteOffer CreateNew(Guid driverId, Guid routeId, string description = null)
        {

            return new RouteOffer(driverId, routeId, description);
        }

        public void CorrectOffer(string newDescription)
        {

            Description = newDescription;
        }

        public void Accept()
        {
            if (Status != RouteOfferStatus.Pending)
                throw new InvalidOperationException("Can only accept a pending route offer.");

            Status = RouteOfferStatus.Accepted;
        }

        public void Refuse()
        {
            if (Status != RouteOfferStatus.Pending)
                throw new InvalidOperationException("Can only refuse a pending route offer.");

            Status = RouteOfferStatus.Refused;
        }
    }

    public enum RouteOfferType
    {
        Correct
    }
    public enum RouteOfferStatus
    {
        Pending,
        Accepted,
        Refused
    }
}
