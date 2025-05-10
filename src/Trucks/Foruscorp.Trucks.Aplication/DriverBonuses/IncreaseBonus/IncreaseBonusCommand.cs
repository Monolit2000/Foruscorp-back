using MediatR;
using FluentResults;

namespace Foruscorp.Trucks.Aplication.DriverBonuses.IncreaseBonus
{
    public class IncreaseBonusCommand : IRequest<Result>
    {
        public Guid DriverId { get; set; }
        public int Bonus { get; set; }
        public string Reason { get; set; }  
    }
}
