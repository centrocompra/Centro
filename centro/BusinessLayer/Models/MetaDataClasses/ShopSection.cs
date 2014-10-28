using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models.DataModel
{
    [MetadataType(typeof(ShopSectionMeta))]
    public partial class ShopSection
    {
    }

    public class ShopSectionMeta
    {
        [Required(ErrorMessage = "*Required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 100 characters are allowed.")]
        [RegularExpression("^([A-Za-z0-9 ]*)$", ErrorMessage = "Only letters and numbers are allowed.")]
        public virtual string SectionName { get; set; }
    }
}