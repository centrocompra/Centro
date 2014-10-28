using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace BusinessLayer.Models.DataModel
{
    [MetadataType(typeof(ShippingAddressMeta))]
    public partial class ShippingAddress //: ISerializable
    {
    }

    public class ShippingAddressMeta
    {
        [Required(ErrorMessage = "*Required.")]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 25 characters are allowed.")]
        [RegularExpression(@"([^<>\\^])*", ErrorMessage = "< & > Symbols not allowed")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 25 characters are allowed.")]
        [RegularExpression(@"([^<>\\^])*", ErrorMessage = "< & > Symbols not allowed")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(25, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 25 characters are allowed.")]
        [RegularExpression(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-??]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$", ErrorMessage = "Email is not valid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public int CountryID { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public int StateID { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public int CityID { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(2000, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 2000 characters are allowed.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Range(100000, 999999, ErrorMessage = "Invalid Postal Code.")]
        public int PostCode { get; set; }
    }
     
}