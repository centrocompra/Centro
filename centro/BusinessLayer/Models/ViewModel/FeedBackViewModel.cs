using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessLayer.Models.ViewModel
{
    public class FeedBackViewModel
    {
        public int FeedBackID { get; set; }
        public int? ProductID { get; set; }
        public int? ShopID { get; set; }
        public Int64? RequestID { get; set; }
        public int? OrderID { get; set; }
        public int FeedBackType { get; set; }
        public int UserID { get; set; }
        public string Review { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string UserImage { get; set; }
        public string RequestTitle { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
    }
}
