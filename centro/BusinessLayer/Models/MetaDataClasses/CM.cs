using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BusinessLayer.Models.DataModel
{
    [MetadataType(typeof(CMMeta))]
    public partial class CM
    {
        
    }
    public class CMMeta
    {
        
        [Required(ErrorMessage = "*Required.")]
        [AllowHtml]
        public virtual string PageContent { get; set; }
    }
}