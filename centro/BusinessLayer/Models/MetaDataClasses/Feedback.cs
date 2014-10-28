using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using BusinessLayer.Classes;
using BusinessLayer.Models.ViewModel;

namespace BusinessLayer.Models.DataModel
{
    [MetadataType(typeof(FeedBackMeta))]
    public partial class Feedback
    {
        
    }

    public class FeedBackMeta
    {
        [Required(ErrorMessage = "Review is required")]
        [StringLength(8000, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 8000 characters are allowed.")]
        public virtual string Review { get; set; }
    }
}
