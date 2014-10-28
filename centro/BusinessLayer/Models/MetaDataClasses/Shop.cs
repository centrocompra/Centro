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
    [MetadataType(typeof(ShopMeta))]
    public partial class Shop
    {
        public virtual List<int> Services { get; set; }
        public string ServiceId { get; set; }
        public List<ProductViewModel> ShopProducts { get; set; }
        public int TotalProducts { get; set; }
    }

    public class ShopMeta
    {
        [Required(ErrorMessage = "*Required.")]
        [StringLength(500, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 500 characters are allowed.")]
        [Remote("UniqueShopname", "Home", HttpMethod = "POST", ErrorMessage = "Shop name already exists. Please enter a different Shoname.")]
        public virtual string ShopName { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(500, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 500 characters are allowed.")]
        [RegularExpression("^([A-Za-z0-9 ]*)$", ErrorMessage = "Only letters and numbers are allowed.")]
        public virtual string ShopTitle { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(200, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 200 characters are allowed.")]
        public virtual string ShopAnnouncement { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(2000, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 2000 characters are allowed.")]
        public virtual string MessageForBuyers { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(150, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 150 characters are allowed.")]
        public virtual string WelcomeMessage { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(8000, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 8000 characters are allowed.")]
        public virtual string PaymentPolicy { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(8000, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 8000 characters are allowed.")]
        public virtual string RefundPolicy { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(8000, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 8000 characters are allowed.")]
        public virtual string SellerInformation { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(8000, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 8000 characters are allowed.")]
        public virtual string AdditionalInformation { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(8000, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 8000 characters are allowed.")]
        public virtual string DeliveryInformation { get; set; }

        [Display(Name = "Printer Type")]
        public virtual string PrinterType { get; set; }

        [Display(Name = "Dimensions")]
        public virtual string Dimensions { get; set; }

        [Display(Name = "Materials")]
        public virtual string Materials { get; set; }

        [Display(Name="Accept Jobs")]
        public virtual bool AcceptJob { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(50, ErrorMessage = "Maximum 50 characters are allowed.")]
        public virtual string ContactFirstName { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(50, ErrorMessage = "Maximum 50 characters are allowed.")]
        public virtual string ContactLastName { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(50, ErrorMessage = "Maximum 50 characters are allowed.")]
        [RegularExpression(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-??]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$", ErrorMessage = "Email is not valid")]
        public virtual string ContactEmail { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(500, ErrorMessage = "Maximum 500 characters are allowed.")]
        public virtual string ContactAddress { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public virtual int ContactCity { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public virtual int ContactState { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public virtual int ContactCountry { get; set; }
    }
}