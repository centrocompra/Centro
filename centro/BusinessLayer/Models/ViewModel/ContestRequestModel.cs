using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessLayer.Models.ViewModel
{
    public class ContestRequestModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Synosis { get; set; }
        public string Criteria { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime Date { get; set; }
    }
}
