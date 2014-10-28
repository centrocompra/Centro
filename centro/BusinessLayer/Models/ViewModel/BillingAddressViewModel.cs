using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models.ViewModel
{
    public class BillingAddressViewModel
    {
        public int BillingAddressID { get; set; }
        public int UserID { get; set; }
        public bool IsPrimary { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "First Name")]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 25 characters are allowed.")]
        [RegularExpression("^([A-Za-z ]*)$", ErrorMessage = "Only letters are allowed.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Last Name")]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 25 characters are allowed.")]
        [RegularExpression("^([A-Za-z ]*)$", ErrorMessage = "Only letters are allowed.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Email")]
        [StringLength(25, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 25 characters are allowed.")]
        [RegularExpression(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-??]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$", ErrorMessage = "Email is not valid")]
        public string Email { get; set; }

        [Display(Name = "Country")]
        [Required(ErrorMessage = "*Required.")]
        public int CountryId { get; set; }

        [Display(Name = "State")]
        [Required(ErrorMessage = "*Required.")]
        public int StateId { get; set; }

        [Display(Name = "City")]
        [Required(ErrorMessage = "*Required.")]
        public int CityId { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Address")]
        [StringLength(2000, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 2000 characters are allowed.")]
        [RegularExpression("^([A-Za-z0-9 /,'()\\-,]*)$", ErrorMessage = "Only letters, numbers, /,',-,(, (comma) and ) are allowed.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Postal Code")]
        [Range(1, 999999, ErrorMessage = "Invalid Postal Code.")]
        public int PostCode { get; set; }
    }
}
