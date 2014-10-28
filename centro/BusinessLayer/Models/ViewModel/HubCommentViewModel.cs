using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessLayer.Models.ViewModel
{
    public class HubCommentViewModel
    {
        public int CommentID { get; set; }
        public int HubID { get; set; }
        public int UserID { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UserName { get; set; }
        public string UserImage { get; set; }
        public int HubUserID { get; set; }
        public string HubTitle { get; set; }
    }   
}
