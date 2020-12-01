using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetCRA.Models
{
    public class UserModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public Boolean isAdmin { get; set; }

    }
}