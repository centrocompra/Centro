using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using BusinessLayer.Classes;
using System.Web.Mvc;

namespace BusinessLayer.Models.DataModel
{
    [MetadataType(typeof(SalesTaxMeta))]
    public partial class SalesTax
    {
        public string CountryName { get; set; }
        public string StateName { get; set; }
    }

    public class SalesTaxMeta
    {
        [Required(ErrorMessage = "*Required.")]
        [RegularExpression(@"^\d{0,2}(\.\d{1,2})?$",ErrorMessage="Invalid Tax value.")]
        public virtual string Tax { get; set; }

    }
}