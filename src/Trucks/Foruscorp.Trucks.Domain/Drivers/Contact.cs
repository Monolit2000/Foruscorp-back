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
        public string Phone { get; private set; }
        public string Email { get; private set; }
        public string TelegramLink { get; private set; }
        private Contact() { }
        private Contact(
            string phone,
            string email,
            string telegramLink)
        {
            //UserId = userId;
            //Name = name;
            Phone = phone;
            Email = email;
            TelegramLink = telegramLink;    
        }

        public static Contact Create(string phone, string email, string telegramLink)
            => new Contact(phone, email, telegramLink);
    }
}
