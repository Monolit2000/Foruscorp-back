using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.BuildingBlocks.Domain;

namespace Foruscorp.Trucks.Domain.Drivers
{
    public class Contact /*: ValueObject*/
    {
        public Guid Id { get; set; } 
        public string FullName { get; private set; }
        public string Phone { get; private set; }
        public string Email { get; private set; }
        public string TelegramLink { get; private set; }
        private Contact() { }
        private Contact(
            string fullName,
            string phone,
            string email,
            string telegramLink)
        {
            Id = Guid.NewGuid();

            if (FullName != fullName)
                FullName = fullName;

            if(Phone != phone)
                Phone = phone;

            if(Email != email)
                Email = email;

            if(TelegramLink != telegramLink)
                TelegramLink = telegramLink;    
        }

        public static Contact Create(string fullName, string phone, string email, string telegramLink)
            => new Contact(fullName, phone, email, telegramLink);
    }
}
