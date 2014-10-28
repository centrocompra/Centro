using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BusinessLayer.Models.DataModel
{
    [MetadataType(typeof(CategoryMeta))]
    public partial class Category
    {
        public virtual string UpdatedCategoryName { get; set; }
    }
    public class CategoryMeta
    {
        
        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Name")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 200 characters are allowed.")]
        [RegularExpression("^([A-Za-z0-9& ]*)$", ErrorMessage = "Only letters and numbers are allowed.")]
        [Remote("UniqueCategoryname", "Categories", HttpMethod = "POST", ErrorMessage = "Category name already exists. Please enter a different Category name.")]
        public virtual string Name { get; set; }

        //[Required(ErrorMessage = "*Required.")]
        [Display(Name = "Description")]
        [StringLength(8000, ErrorMessage = "Maximum 8000 characters are allowed.")]
        public virtual string Description { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public virtual bool Published { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Name")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 200 characters are allowed.")]
        public virtual string UpdatedCategoryName { get; set; }

    }
}