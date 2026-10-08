using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RectImageArchiveMillikitap.ModelsMillikitap.SignalRModel
{
    public class UserSignalR
    {
        public string ConnectionId { get; set; }
        public int BookId { get; set; }
        public int? PageId { get; set; }
        public int UserId { get; set; }
        public string UserCode { get; set; }
        public string UserEmail { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
    }
}