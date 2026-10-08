using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RectImageArchiveMillikitap.Models.CustomAuthentication
{
    public class DataAccess
    {
        public class Role
        {
            public int RoleId { get; set; }
            public string RoleName { get; set; }
            public virtual ICollection<User.User> Users { get; set; }
        }
    }
}