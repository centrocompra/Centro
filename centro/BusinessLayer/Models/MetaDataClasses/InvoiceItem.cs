using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using BusinessLayer.Classes;
using System.Web.Mvc;

namespace BusinessLayer.Models.DataModel
{
    [MetadataType(typeof(InvoiceItemMeta))]
    public partial class InvoiceItem
    {
        
    }

    public class InvoiceItemMeta
    {
        [Required(ErrorMessage = "*Required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 50 characters are allowed.")]
        [RegularExpression("^([A-Za-z0-9 ]*)$", ErrorMessage = "Only letters and numbers are allowed.")]
        public virtual string Title { get; set; }

        [StringLength(500, ErrorMessage = "Maximum 500 characters are allowed.")]
        public virtual string Description { get; set; }

        [Required(ErrorMessage = "*Required.")]
        //[RegularExpression(@"^\d{0,2}(\.\d{1,2})?$", ErrorMessage = "Invalid Amount.")]
        [Range(0,100000,ErrorMessage="Invalid Price.")]
        public virtual decimal Amount { get; set; }
    }

}
