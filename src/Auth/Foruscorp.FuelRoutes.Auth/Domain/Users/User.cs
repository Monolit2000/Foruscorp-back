﻿namespace Foruscorp.FuelRoutes.Auth.Domain.Users
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
    }
}
