using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Models.DataModel;

namespace BusinessLayer.Models.ViewModel
{
    public class ContestViewModel
    {
        public int ContestID { get; set; }
        public int CategoryID { get; set; }
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ContestImage { get; set; }
        public string TermsAndCondition { get; set; }
        public decimal? Fund { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? TotalViews { get; set; }
        public int? WinnerID { get; set; }
        public bool IsActive { get; set; }
        public string CategoryName { get; set; }
        public int TotalEntries { get; set; }
        public int? Votes { get; set; }
    }
}
