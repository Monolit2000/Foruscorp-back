using MediatR;
using Foruscorp.Auth.DataBase;
using Foruscorp.Auth.Domain.Users;

namespace Foruscorp.Auth.Features.CreateCompany
{
    public record CreateCompanyCommand(Guid CompanyId, string Name) : IRequest;

    public class CreateCompanyCommandHandler(
        UserContext userContext) : IRequestHandler<CreateCompanyCommand>
    {
        public async Task Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = Company.Create(request.CompanyId, request.Name);

            await userContext.Companys.AddAsync(company);

            await userContext.SaveChangesAsync(cancellationToken); 
        }
    }
}