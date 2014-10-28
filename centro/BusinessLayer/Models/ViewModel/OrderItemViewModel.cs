using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessLayer.Models.ViewModel
{
    public class OrderItemViewModel
    {
        public int OrderItemsID { get; set; }
        public int OrderID { get; set; }
        public int ShopID { get; set; }
        public string ShopName { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitShippingPrice { get; set; }
        public decimal UnitShippingAfterFirst { get; set; }
        public decimal TotalShippingPrice { get; set; }
        public bool IsShippingAvailable { get; set; }
        public bool IsDownloabale { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ProductTitle { get; set; }
        public string PrimaryImage { get; set; }
        public string ShopOwnerName { get; set; }
    }
}
