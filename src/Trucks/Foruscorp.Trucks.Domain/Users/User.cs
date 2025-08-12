using Foruscorp.Trucks.Domain.Drivers;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Domain.Users
{
    public class User
    {
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; }
        public Guid? ContactId { get; set; }
        public Contact Contact { get; set; }

        private User() { }
       
        public User(Guid userId, string userName)
        {
            UserId = userId;
            CreatedAt = DateTime.UtcNow;
        }

        public static User CreateNew(Guid userId, string userName)  
        {
            return new User(userId, userName);
        }

        //public void UpdateUser(string userName, string email)
        //{
        //    UserName = userName;
        //    Email = email;
        //    UpdatedAt = DateTime.UtcNow;
        //}
    }
}
