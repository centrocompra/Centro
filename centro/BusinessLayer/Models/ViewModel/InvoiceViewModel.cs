using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BusinessLayer.Classes;
using BusinessLayer.Models.DataModel;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models.ViewModel
{
    public class InvoiceViewModel
    {
        public long RequestID { get; set; }
        public int InvoiceID { get; set; }

        [Required(ErrorMessage="*Required")]
        public string InvoiceNumber { get; set; }

        [Required(ErrorMessage="*Required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 100 characters are allowed.")]
        public string Title { get; set; }

        public int? BuyerID { get; set; }
        public int? SellerID { get; set; }
        public string Buyer { get; set; }
        public string Seller { get; set; }

        public InvoiceStatus InvoiceStatus { get; set; }

        [Required(ErrorMessage = "*Required")]
        public decimal? InvoiceAmount { get; set; }
        public List<InvoiceItem> InvoiceItems { get; set; }

        public IEnumerable<InvoiceItem> InvoiceViewItems { get; set; }

        public string TermsAndCondition { get; set; }
        public string NoteForBuyer { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime? EscrowedOn { get; set; }
        public DateTime? ReleasedOn { get; set; }
    }

    public class InvoiceItemsViewModel
    {
        [Required(ErrorMessage = "*Required")]
        public string Title { get; set; }
        [Required(ErrorMessage = "*Required")]
        public string Description { get; set; }
        [Required(ErrorMessage = "*Required")]
        public decimal Amount { get; set; }
    }
}