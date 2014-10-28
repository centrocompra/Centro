using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models.DataModel
{
    [MetadataType(typeof(ContestsMetaClass))]
    public partial class Contest
    {
        public List<Category> Categories { get; set; }
    }

    public class ContestsMetaClass
    {
        [Required(ErrorMessage="*Required")]        
        [StringLength(250, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 250 characters are allowed.")]
        [RegularExpression("^([A-Za-z0-9 ]*)$", ErrorMessage = "Only letters and numbers are allowed.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "*Required")]
        [StringLength(8000, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 8000 characters are allowed.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "*Required")]
        [StringLength(8000, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 8000 characters are allowed.")]
        public string TermsAndCondition { get; set; }

        [Required(ErrorMessage = "*Required")]
        [StringLength(8000, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 8000 characters are allowed.")]
        public string Criteria { get; set; }

        [Required(ErrorMessage = "*Required")]
        [StringLength(8000, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 8000 characters are allowed.")]
        public string Rules { get; set; }

        [Required(ErrorMessage = "*Required")]
        [StringLength(8000, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 8000 characters are allowed.")]
        public string WhyBotherJoining { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "*Required")]
        public int CategoryID { get; set; }

        public List<Category> Categories { get; set; }
    }
}
