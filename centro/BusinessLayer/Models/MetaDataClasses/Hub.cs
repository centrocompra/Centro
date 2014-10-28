using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using BusinessLayer.Models.DataModel;
using System.Web.Mvc;
using BusinessLayer.Classes;

namespace BusinessLayer.Models.DataModel
{
    [MetadataType(typeof(HubMetaClass))]
    public partial class Hub
    {
        [AllowHtml]
        public string Count_0 { get; set; }
        public List<HubTopic> HubTopics { get; set; }
        public string HubTopicText { get; set; }
        public List<HubContent> HubItems { get; set; }
        public string HubOwnerUsername { get; set; }
        public string HubShopName { get; set; }
        [AllowHtml]
        public List<string> Count { get; set; }
    }

    public class HubMetaClass
    {
        [Required(ErrorMessage = "*Required.")]
        //[RegularExpression(@"([^<>\\^])*", ErrorMessage = "Html tags are not allowed in title")]
        [RegularExpression("^([A-Za-z0-9- ]*)$", ErrorMessage = "Only letters, numbers and hyphen(-) are allowed.")]
        [StringLength(200, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 200 characters are allowed.")]
        [Remote("UniqueHubName", "Home", AdditionalFields = "HubID",HttpMethod = "POST", ErrorMessage = "Hub Title already exists. Please enter a different hub title.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public int HubTemplateID { get; set; }        
        
        //[RegularExpression(@"^(http(?:s)?\:\/\/[a-zA-Z0-9\-]+(?:\.[a-zA-Z0-9\-]+)*\.[a-zA-Z]{2,6}(?:\/?|(?:\/[\w\-]+)*)(?:\/?|\/\w+\.[a-zA-Z]{2,4}(?:\?[\w]+\=[\w\-]+)?)?(?:\&[\w]+\=[\w\-]+)*)$",ErrorMessage="Invalid Url")]
        [StringLength(2000, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 2000 characters are allowed.")]
        public string HubURL { get; set; }

        [RegularExpression(@"([^<>\\^])*", ErrorMessage = "Html tags are not allowed in description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public int HubStatus { get; set; }

    }
}
