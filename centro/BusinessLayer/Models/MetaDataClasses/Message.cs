using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using BusinessLayer.Classes;
using System.Web.Mvc;

namespace BusinessLayer.Models.DataModel
{
    [MetadataType(typeof(MessageMeta))]
    public partial class Message
    {
        public virtual string Receiver { get; set; }
        public virtual string[] Receivers { get; set; }
    }

    public class MessageMeta
    {
        [Required(ErrorMessage = "*Required.")]        
        public virtual string Receiver { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(2000, ErrorMessage = "Maximum 2000 characters are allowed.")]
        public virtual string Subject { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public virtual string Body { get; set; }

    }
}
