using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer.Handler;

namespace Centro.Areas.Admin.Controllers
{
    public class OrdersController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NormalOrders()
        {
            return View(SellersHandler.GetAllOrders(1, 10, "CreatedOn", "Desc", ""));
        }
    }
}
