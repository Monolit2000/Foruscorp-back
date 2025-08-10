using Foruscorp.Trucks.Domain.Users;
using Foruscorp.Trucks.Domain.Drivers;

namespace Foruscorp.Trucks.Aplication.Users
{
    public static class UserMapper
    {
        public static UserDto ToUserDto(this User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                CreatedAt = user.CreatedAt,
                ContactId = user.ContactId,
                Contact = user.Contact?.ToContactDto()
            };
        }

        public static ContactDto ToContactDto(this Contact contact)
        {
            if (contact == null)
                return null;

            return new ContactDto
            {
                Id = contact.Id,
                FullName = contact.FullName,
                Phone = contact.Phone,
                Email = contact.Email,
                TelegramLink = contact.TelegramLink
            };
        }
    }
}
