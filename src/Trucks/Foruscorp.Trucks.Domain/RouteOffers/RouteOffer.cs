using Foruscorp.BuildingBlocks.Domain;
using Foruscorp.Trucks.Domain.Drivers;

namespace Foruscorp.Trucks.Domain.RouteOffers
{
    public class RouteOffer : Entity
    {
        public Guid Id { get; private set; }
        public Guid DriverId { get; private set; }
        public Driver Driver { get; private set; }
        public string Description { get; private set; }
        public RouteOfferStatus Status { get; private set; }
        public RouteOfferType Type { get; private set; }
        public DateTime CreatedAt { get; private set; } 
        private RouteOffer() { }

        private RouteOffer(Guid driverId, string description)   
        {
            Id = Guid.NewGuid();
            DriverId = driverId;
            Description = description;
            Status = RouteOfferStatus.Pending;
            CreatedAt = DateTime.UtcNow;    
        }

        public static RouteOffer CreateNew(Guid driverId, string description)
        {
            if (driverId == Guid.Empty)
                throw new ArgumentException("Driver identifier cannot be empty.", nameof(driverId));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description cannot be empty.", nameof(description));

            return new RouteOffer(driverId, description);
        }

        public void CorrectOffer(string newDescription)
        {
            if (Status != RouteOfferStatus.Pending)
                throw new InvalidOperationException("Can only correct a pending route offer.");
            if (string.IsNullOrWhiteSpace(newDescription))
                throw new ArgumentException("New description cannot be empty.", nameof(newDescription));

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
