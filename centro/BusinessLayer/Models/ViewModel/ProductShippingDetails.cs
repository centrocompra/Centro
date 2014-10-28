using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BusinessLayer.Models.ViewModel
{
    public class ProductShippingDetails
    {
        public string ShipFromCountryName { get; set; }
        public string ShipToCountryName { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal ShippingCostAfterFirst { get; set; }
    }

    public class ProductShippingCountries
    {
        public int CountryID { get; set; }
        public string CountryName { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal ShippingCostAfterFirst { get; set; }
    }
}