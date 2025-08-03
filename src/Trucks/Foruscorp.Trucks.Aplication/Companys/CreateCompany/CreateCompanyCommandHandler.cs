using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Foruscorp.Trucks.Domain.Companys;
using Foruscorp.Trucks.Aplication.Contruct;
using MassTransit;
using Foruscorp.Trucks.IntegrationEvents;

namespace Foruscorp.Trucks.Aplication.Companys.CreateCompany
{
    public record CreateCompanyCommand(string Name, string SamsaraToken) : IRequest<Result>;
    public class CreateCompanyCommandHandler(
        ITruckContext context,
        IPublishEndpoint publishEndpoint) : IRequestHandler<CreateCompanyCommand, Result>
    {
        public async Task<Result> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
        {
            var isCompanyExist = await context.Companys
                .AnyAsync(c => c.Name == request.Name, cancellationToken);

            if (isCompanyExist)
                return Result.Fail($"Company with name {request.Name} already exists.");

            var company = Company.Create(request.Name, request.SamsaraToken);

            await context.Companys.AddAsync(company);

            await context.SaveChangesAsync(cancellationToken);

            await publishEndpoint.Publish(new CompanyCreatedIntegrationEvent(company.Id, company.Name));

            return Result.Ok().WithSuccess("Company created successfully.");    
        }
    } 
}
