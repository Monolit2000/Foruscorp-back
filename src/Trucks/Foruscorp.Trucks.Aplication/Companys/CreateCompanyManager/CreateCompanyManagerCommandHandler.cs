using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Foruscorp.Trucks.Domain.Companys;
using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Aplication.Companys;
using MassTransit;
using Foruscorp.Trucks.IntegrationEvents;

namespace Foruscorp.Trucks.Aplication.Companys.CreateCompanyManager
{
    public record CreateCompanyManagerCommand(string Name, string Phone, string Email, string TelegramLink, Guid CompanyId) : IRequest<Result<CompanyManagerDto>>;
    
    public class CreateCompanyManagerCommandHandler(
        ITruckContext context,
        IPublishEndpoint publishEndpoint) : IRequestHandler<CreateCompanyManagerCommand, Result<CompanyManagerDto>>
    {
        public async Task<Result<CompanyManagerDto>> Handle(CreateCompanyManagerCommand request, CancellationToken cancellationToken)
        {
            var company = await context.Companys
                .FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken);

            if (company == null)
                return Result.Fail<CompanyManagerDto>($"Company with ID {request.CompanyId} not found.");

            var existingManager = await context.CompanyManagers
                .Include(cm => cm.User)
                .ThenInclude(u => u.Contact)
                .AnyAsync(cm => cm.User.Contact.Email == request.Email && cm.CompanyId == request.CompanyId, cancellationToken);

            if (existingManager)
                return Result.Fail<CompanyManagerDto>($"Manager with email {request.Email} already exists in company {request.CompanyId}.");

            var companyManager = CompanyManager.CreateNew(request.Name, request.Phone, request.Email, request.TelegramLink, request.CompanyId);

            //if (companyManager.User.Contact != null)
            //{
            //    await context.Contacts.AddAsync(companyManager.User.Contact, cancellationToken);
            //}
            
            //await context.Users.AddAsync(companyManager.User, cancellationToken);
            
            await context.CompanyManagers.AddAsync(companyManager, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            await publishEndpoint.Publish(new CompanyManagerCreatedIntegrationEvent(
                companyManager.Id,
                companyManager.UserId,
                companyManager.CompanyId,
                request.Name,
                request.Email,
                company.Name), 
                cancellationToken);

            var companyManagerDto = new CompanyManagerDto
            {
                Id = companyManager.Id,
                UserId = companyManager.UserId,
                CompanyId = companyManager.CompanyId,
                FullName = companyManager.User.Contact?.FullName ?? request.Name,
                CreatedAt = companyManager.CreatedAt,
                UpdatedAt = companyManager.UpdatedAt
            };

            return Result.Ok(companyManagerDto);
        }
    }
}
