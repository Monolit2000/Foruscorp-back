using MediatR;

namespace Foruscorp.Trucks.Aplication.Trucks.Commands
{
    public class FormModelTruckGroupCommand : IRequest<FormModelTruckGroupResult>
    {
        public Guid TruckId { get; set; }

        public FormModelTruckGroupCommand(Guid truckId)
        {
            TruckId = truckId;
        }
    }

    public class FormModelTruckGroupResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Guid? ModelTruckGroupId { get; set; }
        public bool IsNewGroup { get; set; }

        public static FormModelTruckGroupResult SuccessResult(Guid modelTruckGroupId, bool isNewGroup = false)
        {
            return new FormModelTruckGroupResult
            {
                Success = true,
                Message = isNewGroup ? "ModelTruckGroup created successfully" : "ModelTruckGroup found and assigned",
                ModelTruckGroupId = modelTruckGroupId,
                IsNewGroup = isNewGroup
            };
        }

        public static FormModelTruckGroupResult FailureResult(string message)
        {
            return new FormModelTruckGroupResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
