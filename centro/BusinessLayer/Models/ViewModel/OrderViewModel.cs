using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Classes;

namespace BusinessLayer.Models.ViewModel
{
    public class OrderViewModel
    {
        public Int32 OrderId { get; set; }
        public Int32 ShopId { get; set; }
        public Int32 ShopOwnerId { get; set; }
        public string ShopwOwnerUsername { get; set; }
        public Int32 BuyerId { get; set; }
        public String ShopName { get; set; }
        public string BuyerProfilePic { get; set; }
        public string SellerProfilePic { get; set; }
        public String BuyerName { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public ShippingStatus ShippingStatus { get; set; }
        public DateTime OrderCreatedOn { get; set; }
        public Decimal OrderAmount { get; set; }
        public decimal ItemTotalPrice { get; set; }
        public decimal ItemTotalShippingPrice { get; set; }
        public decimal AdminCommission { get; set; }
        public bool IsPercentage { get; set; }
        public decimal? Tax { get; set; }
        public string Type { get; set; }
        public string TrackingID { get; set; }
        public bool IsFeedbackGiven { get; set; }
    }
}
