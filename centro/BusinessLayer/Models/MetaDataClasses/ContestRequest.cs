using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models.DataModel
{
    [MetadataType(typeof(ContestRequestMetaClass))]
    public partial class ContestRequest
    {
        
    }

    public class ContestRequestMetaClass
    {
        [Required(ErrorMessage="*Required")]        
        [StringLength(250, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 250 characters are allowed.")]
        [RegularExpression("^([A-Za-z0-9 ]*)$", ErrorMessage = "Only letters and numbers are allowed.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "*Required")]
        [StringLength(8000, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 8000 characters are allowed.")]
        public string Synosis { get; set; }

        [Required(ErrorMessage = "*Required")]
        [StringLength(8000, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 8000 characters are allowed.")]
        public string Criteria { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Email")]
        [RegularExpression(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-??]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$", ErrorMessage = "Email is not valid")]
        public virtual string Email { get; set; }
    }
}
