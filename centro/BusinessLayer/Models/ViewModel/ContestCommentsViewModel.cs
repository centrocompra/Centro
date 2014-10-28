using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Models.DataModel;

namespace BusinessLayer.Models.ViewModel
{
    public class ContestCommentsViewModel
    {
        public int ContestCommentID { get; set; }
        public int ContestParticipantID { get; set; }
        public string Username { get; set; }
        public string Comment { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ShopID { get; set; }
        public string ShopName { get; set; }
    }
}
