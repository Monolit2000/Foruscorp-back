using Foruscorp.Trucks.Aplication.Contruct;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Infrastructure.Services
{
    public class CurrentUser : ICurrentUser
    {
        public Guid UserId { get; }
        public string Email { get; }
        public Guid? CompanyId { get; }
        public List<string> Roles { get; }

        public CurrentUser(IHttpContextAccessor contextAccessor)
        {
            var user = contextAccessor.HttpContext?.User;
            if (user == null || !user.Identity?.IsAuthenticated == true)
                throw new UnauthorizedAccessException("User is not authenticated"); 

            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(id, out var parsedId))
                throw new UnauthorizedAccessException("Invalid user ID");

            UserId = parsedId;
            Email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

            var companyIdStr = user.FindFirst("company_id")?.Value;
            CompanyId = Guid.TryParse(companyIdStr, out var companyGuid) ? companyGuid : null;

            Roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
        }

        public bool IsInRole(string role) => Roles.Contains(role);
    }
}
