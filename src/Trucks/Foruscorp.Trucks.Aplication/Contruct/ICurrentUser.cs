using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Contruct
{
    public interface ICurrentUser
    {
        Guid UserId { get; }
        string Email { get; }
        Guid? CompanyId { get; }
        List<string> Roles { get; }
        bool IsInRole(string role);
    }
}
