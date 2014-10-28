using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using BusinessLayer.Classes;
using System.Web.Mvc;
using BusinessLayer.Models.ViewModel;

namespace BusinessLayer.Models.DataModel
{
    [MetadataType(typeof(UserMeta))]
    public partial class User
    {
        public List<ShopPicture> ShopPictures { get; set; }
        public Shop ShopDetails { get; set; }
        public String ConfirmPassword { get; set; }
        public string EmailOrUsername { get; set; }
        public virtual string PasswordRecoveryEmailId { get; set; }
        public String UserTagsList { get; set; }
        public String ProfilePicUrl { get; set; }
        public String UserLocation { get; set; }
        public string ShopName { get; set; }
        public int ShopID { get; set; }
        public string PaypalIdOptional { get; set; }
        public FileAttachmentViewModel Attachments { get; set; }
    }

    public class UserMeta
    {
        [Display(Name = "Licence")]
        [AllowHtml]
        public virtual string Licence { get; set; }


        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "User Name")]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 25 characters are allowed.")]
        public virtual string EmailOrUsername { get; set; }

        [Display(Name = "Industry")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 200 characters are allowed.")]
        public virtual string Industry { get; set; }
        
        [StringLength(375, MinimumLength = 100, ErrorMessage = "Mininum 100 and Maximum 375 characters are allowed.")]
        public virtual string AboutUs { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "User Name")]
        [StringLength(25, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 25 characters are allowed.")]
        [Remote("UniqueUsername", "Home", HttpMethod = "POST", ErrorMessage = "Username already exists. Please enter a different Username.")]
        [RegularExpression("^([A-Za-z0-9 ]*)$", ErrorMessage = "Only letters and numbers are allowed.")]

        public virtual string UserName { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Password")]
        [StringLength(25, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 25 characters are allowed.")]
        public virtual string Password { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Email Id")]
        [RegularExpression(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-??]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$", ErrorMessage = "Email is not valid")]
        [Remote("UniqueEmail", "Home", HttpMethod = "POST", ErrorMessage = "Email-id already exists. Please enter a different email-id.")]
        public virtual string EmailId { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Email Id")]
        [RegularExpression(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-??]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$", ErrorMessage = "Email is not valid")]
        public virtual string PasswordRecoveryEmailId { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "First Name")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Mininum 3 characters are required.")]
        public virtual string FirstName { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Last Name")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Mininum 3 characters are required.")]
        public virtual string LastName { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password does not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Paypal Id")]
        [RegularExpression(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-??]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$", ErrorMessage = "Email is not valid")]
        public virtual string PaypalID { get; set; }

        [Display(Name = "Paypal Id")]
        [RegularExpression(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-??]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$", ErrorMessage = "Email is not valid")]
        public virtual string PaypalIdOptional { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Street Address 1")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 500 characters are required.")]
        public virtual string StreetAddress1 { get; set; }

        [Display(Name = "Street Address 2")]
        [StringLength(500, ErrorMessage = "Maximum 500 characters are required.")]
        public virtual string StreetAddress2 { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "PostalCode")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage="Invalid Postal Code.")]
        [StringLength(6, ErrorMessage = "Maximum 6 characters are required.")]
        public virtual string PostalCode { get; set; }
    }

    public class ChangePasswordModel 
    {

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "Current Password")]
        [StringLength(25, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 25 characters are allowed.")]
        public virtual string CurrentPassword { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Display(Name = "New Password")]
        [StringLength(25, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 25 characters are allowed.")]
        public virtual string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password does not match.")]
        public string ConfirmPassword { get; set; }
    }
   
}
