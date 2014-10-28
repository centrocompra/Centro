using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessLayer.Models.ViewModel
{
    public class ShopViewModel
    {
        public int JobApplicationID { get; set; }
        public int ShopID { get; set; }
        public string ShopName { get; set; }
        public string ShopOwnerName { get; set; }
        public int UserID { get; set; }
        public decimal? BidAmount { get; set; }
        public bool? RequestSent { get; set; }
        public bool? RequestAccepted { get; set; }
        public int FeedbackCount { get; set; }
        public double AvgRating { get; set; }
    }
}
