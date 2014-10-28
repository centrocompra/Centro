using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BusinessLayer.Models.DataModel;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models.ViewModel
{
    public class AccountActivityViewModel
    {
        public int AccountActivityID { get; set; }
        public int ActivityID { get; set; }
        public int ActivityType { get; set; }
        public string ActivityText { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Username { get; set; }
        public string FollowingToName { get; set; }
        public string ActivityLink { get; set; }
        public string ActivityImage { get; set; }
    }
}
