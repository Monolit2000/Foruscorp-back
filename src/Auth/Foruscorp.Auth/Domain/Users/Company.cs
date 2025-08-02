namespace Foruscorp.Auth.Domain.Users
{
    public class Company
    {
        public Guid CompanyId { get; set; }

        public string Name { get; set; }

        public List<User> Users { get; set; }

        public Company(Guid companyId, string name)
        {
            CompanyId = companyId;
            Name = name;
            Users = new List<User>();
        }

        public void AddUser(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            Users.Add(user);
        }

        public void RemoveUser(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            Users.Remove(user);
        }

        public static Company Create(Guid companyId, string name) 
            => new Company(companyId, name);
    }
}
