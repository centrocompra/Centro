using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using BusinessLayer.Classes;
using BusinessLayer.Models.ViewModel;

namespace BusinessLayer.Models.DataModel
{
    [MetadataType(typeof(ReportMeta))]
    public partial class Report
    {
    }

    public class ReportMeta
    {
        [Required(ErrorMessage = "*Required")]
        public virtual string ReportType { get; set; }
        [Required(ErrorMessage = "*Required")]
        [StringLength(8000, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 8000 characters are allowed.")]
        public virtual string Message { get; set; }
    }

    
}
