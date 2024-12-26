using Microsoft.AspNetCore.Identity;

namespace JWT.Core.Model
{
    public class CustomUser : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }

    }
}
