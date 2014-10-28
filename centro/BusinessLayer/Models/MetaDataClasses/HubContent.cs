using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using BusinessLayer.Models.DataModel;
using System.Web.Mvc;

namespace BusinessLayer.Models.DataModel
{
    [MetadataType(typeof(HubContentClass))]
    public partial class HubContent
    {
        [AllowHtml]
        public string Count_0 { get; set; }
    }

    public class HubContentClass
    {
        [Required(ErrorMessage = "*Required.")]        
        public string Count_0 { get; set; }
        
    }
}
