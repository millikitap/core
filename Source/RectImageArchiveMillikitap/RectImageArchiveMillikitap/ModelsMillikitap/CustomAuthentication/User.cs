using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using static RectImageArchiveMillikitap.Models.CustomAuthentication.DataAccess;

namespace RectImageArchiveMillikitap.Models.User
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public Guid ActivationCode { get; set; }
        public string Telephone { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
        public virtual ICollection<Likes> Likes { get; set; }        
    }

    public class RegistrationModelUser: User
    {
        [NotMapped]
        public string ConfirmPassword { get; set; }
    }
}