using Microsoft.EntityFrameworkCore.ChangeTracking;
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

        private User() { }
       
        public User(Guid userId)
        {
            UserId = userId;
            CreatedAt = DateTime.UtcNow;
        }

        public static User CreateNew(Guid userId)
        {
            return new User(userId);
        }

        //public void UpdateUser(string userName, string email)
        //{
        //    UserName = userName;
        //    Email = email;
        //    UpdatedAt = DateTime.UtcNow;
        //}
    }
}
