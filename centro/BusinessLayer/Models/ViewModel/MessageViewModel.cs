using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BusinessLayer.Models.DataModel;

namespace BusinessLayer.Models.ViewModel
{
    public class MessageViewModel
    {
        public int MessageID { get; set; }
        public int? ReplyMessageID { get; set; }
        public int SenderID { get; set; }
        public string SenderUsername { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsRead { get; set; }
        public bool IsArchived { get; set; }
        public IQueryable<ProductFileViewModel> Attachments { get; set; }
        public List<string> Receivers { get; set; }
        public string UserProfilePicture { get; set; }
    }
}