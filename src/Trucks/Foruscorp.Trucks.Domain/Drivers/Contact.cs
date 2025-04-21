using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.BuildingBlocks.Domain;

namespace Foruscorp.Trucks.Domain.Drivers
{
    public class Contact : ValueObject
    {
        //public Guid UserId { get; set; } 

        //public Guid Id { get; set; } = Guid.NewGuid();
        //public string Name { get; private set; }
        public string Phone { get; private set; }
        public string Email { get; private set; }
        private Contact() { }
        private Contact(
            Guid userId,    
            //string name,
            string phone,
            string email)
        {
            //UserId = userId;
            //Name = name;
            Phone = phone;
            Email = email;
        }

        public static Contact Create(Guid userId,/* string name,*/ string phone, string email)
            => new Contact(userId,/* name,*/ phone, email);
    }
}
