using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BusinessLayer.Models.DataModel;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models.ViewModel
{
    public class ShopCartItems
    {
        public int CartOrder { get; set; }
        public int ShopID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public int QuantityInStock { get; set; }
        public List<ProductShippingCountries> ShipToCountries { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalShippingPrice { get; set; }
        public decimal UnitShippingPrice { get; set; }
        public decimal UnitShippingAfterFirst { get; set; }
        public bool IsShippingAvailable { get; set; }
        public bool IsDownloadable { get; set; }
        public int? SendDownloadVia { get; set; }
    }

    public class ShopCart
    {
        public int ShopID { get; set; }
        public string NoteForShop { get; set; }
        public decimal ItemTotalPrice { get; set; }
        public decimal ItemTotalShippingPrice { get; set; }
        public int? ShipToCountryID { get; set; }
        public int? ShipToStateID { get; set; }
        public decimal? Tax { get; set; }
        public List<ShopCartItems> ShopCartItems { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)] 
        public decimal TotalAmountToBePaid { get; set; }
        public string ShopOwnerName { get; set; }
    }

    public class Cart
    {
        public int UserID { get; set; }
        public List<ShopCart> ShopCart { get; set; }
    }

    public class Donation
    {
        public int ContestID { get; set; }
        public int UserID { get; set; }
        public decimal Amount { get; set; }
        public string PayKey { get; set; }
    }

    public class PaymentDetails
    {
        public int ShopID { get; set; }
        public int UserID { get; set; }

        public BillingAddressViewModel BillingAddress { get; set; }
        public ShippingAddressViewModel ShippingAddress { get; set; }

        public string PaypalEmail { get; set; }

        /*
        public string BillingFirstName { get; set; }
        public string BillingLastName { get; set; }
        public string BillingEmail { get; set; }
        public int BillingCountry { get; set; }
        public int BillingState { get; set; }
        public int BillingCity { get; set; }
        public string BillingAddress { get; set; }
        public int BillingPostCode { get; set; }

        public string ShippingFirstName { get; set; }
        public string ShippingLastName { get; set; }
        public string ShippingEmail { get; set; }
        public int ShippingCountry { get; set; }
        public int ShippingState { get; set; }
        public int ShippingCity { get; set; }
        public string ShippingAddress { get; set; }
        public int ShippingPostCode { get; set; }
        */

        [Required(ErrorMessage = "*Required.")]
        [StringLength(20, MinimumLength = 12, ErrorMessage = "Mininum 12 and Maximum 20 characters are allowed.")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "Mininum 3 and Maximum 15 characters are allowed.")]
        public string NameOnCard { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public string ExpiryMonth { get; set; }

        [Required(ErrorMessage = "*Required.")]
        public string ExpiryYear { get; set; }

        [Required(ErrorMessage = "*Required.")]
        [RegularExpression(@"^[0-9]{3}$", ErrorMessage = "Invalid CVV")]
        public string CVV { get; set; }
    }

    public class TransactionStatus
    {
        public string TransactionID { get; set; } // 7
        public string Message { get; set; } // 4
        public string ResponseCode { get; set; } //1 :: 1 = Approved ,2 = Declined ,3 = Error ,4 = Held for Review
        public string AuthorizationCode { get; set; } // 5
        public string CustomerID { get; set; } // 13
    }
}
