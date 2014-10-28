using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Handler;
using BusinessLayer.Classes;
using System.IO;

namespace Centro.Controllers
{
    public class InvoiceController : FrontEndBaseController
    {
        public ActionResult Index()
        {
            return View(InvoiceHandler.GetInvoiceByUserAndType(SiteUserDetails.LoggedInUser.Id, MessagePlaceHolder.Inbox).List);
        }

       

        [HttpGet]
        public ActionResult Create(string id, int req_id)
        {
            // Buyer details
            User buyer = UsersHandler.UserByUsername(id).Object;
            if (buyer == null)
                return RedirectToAction("ManageRequests", "Shops");
            ViewBag.Buyer = buyer;

            ViewBag.Seller = UsersHandler.GetUserByID(SiteUserDetails.LoggedInUser.Id).Object;
            InvoiceViewModel model = new InvoiceViewModel();
            model.BuyerID = buyer.UserID;
            model.SellerID = SiteUserDetails.LoggedInUser.Id;
            model.RequestID = req_id;
            model.InvoiceStatus = InvoiceStatus.Pending;
            return View(model);
        }

        [HttpPost]
        public JsonResult Create(InvoiceViewModel model)
        {
            List<InvoiceItem> temp = model.InvoiceItems;
            model.InvoiceItems = new List<InvoiceItem>();
            string deleted_rows=Request["DeletedItems"];
            if (deleted_rows.Trim().Length > 0)
            {
                int[] Skip_Rows = deleted_rows.Split(new char[] { ',' }).Select(m => Convert.ToInt32(m)).ToArray();
                for (short i = 0; i < temp.Count; i++)
                {
                    if (!Skip_Rows.Contains(i))
                        model.InvoiceItems.Add(temp[i]);
                }
            }
            else
            {
                model.InvoiceItems = temp;
            }
            SiteFee siteFee = Config.SiteFee;
            model.InvoiceAmount = model.InvoiceItems.Sum(m => m.Amount);
            model.InvoiceAmount += siteFee.IsPercentage ? model.InvoiceAmount * siteFee.SiteFee1 / 100 : siteFee.SiteFee1;
            return Json(InvoiceHandler.CreateInvoice(model, SiteUserDetails.LoggedInUser.Username) ,JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public PartialViewResult _Invoices(int type)
        {
            ViewBag.Buyer = "false";
            return PartialView(InvoiceHandler.GetInvoiceByUserAndType(SiteUserDetails.LoggedInUser.Id, (MessagePlaceHolder)type).List);
        }

        public ActionResult View(int id, string userType)
        {
            ViewBag.LoggedInUser = SiteUserDetails.LoggedInUser;
            ViewBag.Type = userType;
            return View(InvoiceHandler.InvoiceViewModelById(id).Object);
        }
    }
}
