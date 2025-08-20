using Foruscorp.Trucks.Domain.Users;
using Foruscorp.Trucks.Domain.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Domain.Companys
{
    public class CompanyManager
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }  
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public CompanyManager(Guid userId, Guid companyId)
        {
            UserId = userId;
            CompanyId = companyId;
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        public static CompanyManager Create(Guid userId, Guid companyId)
        {
            return new CompanyManager(userId, companyId);
        }

        public static CompanyManager CreateNew(string name, string phone, string email, string telegramLink, Guid companyId)
        {
            var userId = Guid.NewGuid();
            var user = User.CreateNew(userId, name);
            
            if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(phone) || 
                !string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(telegramLink))
            {
                var contact = Contact.Create(name, phone, email, telegramLink);
                user.Contact = contact;
                user.ContactId = contact.Id;
            }
            
            var companyManager = new CompanyManager(userId, companyId);
            companyManager.User = user;
            
            return companyManager;
        }
    }
}
