using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace BusinessLayer.Models.DataModel
{
    [MetadataType(typeof(HubCommentMeta))]
    public partial class HubComment
    {
        public Hub MyHub { get; set; }
        public string HubTopicText { get; set; }
        public string HubOwnerUserName { get; set; }
        public string HubPicture { get; set; }
    }

    public class HubCommentMeta
    {
        [Required(ErrorMessage="*Required.")]
        [StringLength(3000, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 3000 characters are allowed.")]
        [RegularExpression(@"([^<>\\^])*", ErrorMessage = "Html tags are not allowed in comment.")]
        public string Comment { get; set; }
    }
}
