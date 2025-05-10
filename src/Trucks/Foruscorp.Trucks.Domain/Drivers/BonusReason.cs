using Foruscorp.BuildingBlocks.Domain;

namespace Foruscorp.Trucks.Domain.Drivers
{
    public class BonusReason : ValueObject
    {
        public string Reason { get; private set; }

        public static BonusReason Reason1 => new BonusReason(nameof(Reason1));
        public static BonusReason Reason2 => new BonusReason(nameof(Reason2));
        public static BonusReason Reason3 => new BonusReason(nameof(Reason3));

        private BonusReason(string reason)
        {
            Reason = reason;  
        }

    }
}
