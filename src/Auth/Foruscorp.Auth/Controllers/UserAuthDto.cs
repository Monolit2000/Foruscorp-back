namespace Foruscorp.Auth.Controllers
{
    public class UserAuthDto
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }        
        public Guid? CompanyId { get; set; } 
    }
}
