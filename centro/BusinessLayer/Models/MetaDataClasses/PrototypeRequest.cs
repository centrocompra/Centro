using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Classes;

namespace BusinessLayer.Models.DataModel
{
    [MetadataType(typeof(PrototypeRequestMeta))]
    public partial class PrototypeRequest
    {
    }

    public class PrototypeRequestMeta
    {
        [Required(ErrorMessage = "*Required.")]
        public virtual int CategoryId { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(2000, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 2000 characters are allowed.")]
        public virtual string Requirements { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public virtual DateTime IdealStartDate { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Add Request Title")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 100 characters are allowed.")]
        [RegularExpression("^([A-Za-z0-9 ]*)$", ErrorMessage = "Only letters and numbers are allowed.")]
        public virtual string RequestTitle { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Description")]
        [StringLength(2000, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 2000 characters are allowed.")]
        public virtual string Description { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Dimensions")]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 25 characters are allowed.")]
        public virtual string Dimensions { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Material")]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 25 characters are allowed.")]
        public virtual string Material { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Range(0.1, Double.MaxValue, ErrorMessage="Invalid Budget.")]        
        public virtual decimal MinBudget { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Max")]
        [Range(0.1, Double.MaxValue, ErrorMessage = "Invalid Budget.")]        
        [NumericGreaterThan("MinBudget", AllowEquality = true, ErrorMessage = "Max Budget must be greater than or equal to min budget.")]
        public virtual decimal MaxBudget { get; set; }
    }
}