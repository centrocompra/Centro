using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BusinessLayer.Models.DataModel
{
    [MetadataType(typeof(SubCategoryMeta))]
    public partial class SubCategory
    {
        
    }
    public class SubCategoryMeta
    {
        
        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Name")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 200 characters are allowed.")]
        [RegularExpression("^([A-Za-z0-9& ]*)$", ErrorMessage = "Only letters and numbers are allowed.")]
        //[Remote("UniqueSubCategoryname", "Categories", HttpMethod = "POST", ErrorMessage = "Sub-Category name already exists. Please enter a different Category name.")]
        public virtual string Name { get; set; }
    }
}