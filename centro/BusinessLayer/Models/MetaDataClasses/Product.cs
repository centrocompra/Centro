using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using BusinessLayer.Classes;
using  BusinessLayer.Models.ViewModel;

namespace BusinessLayer.Models.DataModel
{
    [MetadataType(typeof(ProductMeta))]
    public partial class Product
    {
        public virtual int ManufacturerID { get; set; }
        public virtual List<Picture> ProductImages { get; set; }
        public List<ProductShippingDetails> ProductShippingDetails { get; set; }
        public string ShopOwnerUsername { get; set; }
        public string ShopName { get; set; }
        public virtual string DownloadURL { get; set; }
        public virtual int SendDownloadViaProp { get; set; }
    }

    public class ProductMeta
    {
        [Required(ErrorMessage = "Centro only allows original products to be posted and that all users must adhere to the terms of use (link), and privacy agreement (link) and agree to be legally liable and waive Replictity of any liability..etc")]
        public virtual string ManufacturerID { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public virtual string DownloadURL { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(200, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 200 characters are allowed.")]
        public virtual string Manufacturer { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(8000, ErrorMessage = "Maximum 8000 characters are allowed.")]
        public virtual string DownlodableShippingPolicy { get; set; }

        [Required(ErrorMessage = "*Required.")]        
        public virtual int CategoryId { get; set; }

        [Required(ErrorMessage="*Required.")]
        public virtual int SendDownloadViaProp { get; set; }

        [Required(ErrorMessage = "*Required.")]        
        public virtual int Condition { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public virtual int Quantity { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public virtual int ShipFromId { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(500, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 500 characters are allowed.")]
        [RegularExpression("^([A-Za-z0-9 ]*)$", ErrorMessage = "Only letters and numbers are allowed.")]
        public virtual string Title { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public virtual int ShopSectionId { get; set; }

        public virtual string PrimaryPicture { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(8000, MinimumLength = 6, ErrorMessage = "Mininum 6 and Maximum 8000 characters are allowed.")]
        [RegularExpression(@"([^<>\\^])*", ErrorMessage = "Html tags are not allowed in Description")]
        public virtual string Description { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [Range(0, 100000, ErrorMessage = "Invalid price.")]
        public virtual decimal UnitPrice { get; set; }

    }
}