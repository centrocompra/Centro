using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Models.DataModel;

namespace BusinessLayer.Models.ViewModel
{
    public class ContestParticipantsViewModel
    {
        public ContestParticipant ContestParticipant { get; set; }
        public string ProductImage { get; set; }
        public string Username { get; set; }
        public string ProductTitle { get; set; }
        public int ShopID { get; set; }
        public int? VoteUp { get; set; }
        public int? VoteDown { get; set; }
        public int UserID { get; set; }
    }
}
