using System;

namespace Foruscorp.Trucks.Aplication.Users
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? ContactId { get; set; }
        public ContactDto Contact { get; set; }
    }

    public class ContactDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string TelegramLink { get; set; }
    }
}
