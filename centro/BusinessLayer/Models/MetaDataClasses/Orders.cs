using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Models.ViewModel;

namespace BusinessLayer.Models.DataModel
{
    public partial class Order
    {
        public List<OrderItemViewModel> OrderList { get; set; }
        public string BuyerName { get; set; }
    }
}
