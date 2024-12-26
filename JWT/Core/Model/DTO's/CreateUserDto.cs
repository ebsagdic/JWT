using System;
using System.Collections.Generic;

namespace JWT.Model
{
    public class UserForLoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }

        
    }

    public class UserForRegisterDto
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SelectedRole { get; set; }
    }
}
