using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessLayer.Models.ViewModel
{
    public class ProductViewModel
    {
        public int ProductID { get; set; }
        public int ShopId { get; set; }
        public string Manufacturer { get; set; }
        public int CategoryId { get; set; }
        public int Condition { get; set; }
        public int Quantity { get; set; }
        public int ShipFromId { get; set; }
        public string Title { get; set; }
        public int ShopSectionId { get; set; }
        public bool IsDeleted { get; set; }
        public string Tags { get; set; }
        public string Materials { get; set; }
        public string PrimaryPicture { get; set; }
        public string Description { get; set; }
        public decimal? UnitPrice { get; set; }
        public DateTime? LastViewedOn { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public int? DeletedBy { get; set; }
        public bool? IsFeatured { get; set; }
        public bool IsDownloadable { get; set; }
        public string ShopName { get; set; }
        public string ShopOwnerName { get; set; }
    }
}
