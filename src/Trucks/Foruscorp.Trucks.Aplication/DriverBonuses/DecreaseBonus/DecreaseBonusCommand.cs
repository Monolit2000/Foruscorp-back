using MediatR;
using FluentResults;

namespace Foruscorp.Trucks.Aplication.DriverBonuses.DecreaseBonus
{
    public class DecreaseBonusCommand : IRequest<Result>
    {
        public Guid DriverId { get; set; }
        public int Bonus { get; set; }
        public string Reason { get; set; }
    }
}
