using System.Web.Mvc;
using BusinessLayer.Classes;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Models.ViewModel;
using System.Collections.Generic;
using BusinessLayer.Handler;
using System.Web;
using System;

namespace Centro.Areas.Admin.Controllers
{
    public class HomeController : AdminBaseController
    {
        [SkipAuthentication]
        public ActionResult LogIn()
        {
            //if (Request.IsAuthenticated)
            //    return RedirectToAction("Dashboard", "Home");
            return View(new User());
        }

        [AuthenticateUser]
        public ActionResult Dashboard()
        {
            return View();
        }

        [AuthenticateUser]
        public ActionResult ChangePassword()
        {
            ChangePasswordModel model = new ChangePasswordModel();
            return View(model);
        }

        [AuthenticateUser]
        [HttpPost]
        public JsonResult ChangePassword(ChangePasswordModel obj)
        {
            var result = UsersHandler.ChangeUserPassword(CentroUsers.LoggedInUser.Id, obj);
            if (result.Status == ActionStatus.Successfull)
            {
                return Json(new ActionOutput
                {
                    Status = result.Status,
                    Message = result.Message,
                    Results = new List<string> 
                    { 
                    Url.Action("ManageUser", "User") 
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new ActionOutput
                {
                    Status = result.Status,
                    Message = result.Message,

                }, JsonRequestBehavior.AllowGet);
            }

        }

        [AuthenticateUser]
        public ActionResult SiteFee()
        {
            return View(AdminHandler.GetSiteFee().Object);
        }

        [AuthenticateUser]
        public JsonResult SiteFeeAddOrUpdate(SiteFee obj)
        {
            return Json(AdminHandler.SiteFeeAddOrUpdate(obj), JsonRequestBehavior.AllowGet);
        }

        [AuthenticateUser]
        public ActionResult CMS(int? id = null)
        {
            var result = id.HasValue && id.Value > 0 ? CMSHandler.Page(id.Value) : new CM();
            return View(result);
        }

        [AuthenticateUser]
        public ActionResult SavePageContent(CM model)
        {
            var o = CMSHandler.SavePageContent(model);
            TempData["Msg"] = o.Message;
            return RedirectToAction("CMS", "Home", new { @id = model.CmsID });
        }

        [AuthenticateUser]
        public ActionResult ManageDeals()
        {
            return View(ProductsHandler.GetDeals());
        }

        [AuthenticateUser, HttpGet]
        public ActionResult SaveDeal(int? id)
        {
            return View(id.HasValue ? ProductsHandler.GetDeal(id.Value) : new DealViewModel());
        }

        [AuthenticateUser, HttpPost]
        public ActionResult SaveDeal(DealViewModel model)
        {
            foreach (var file in model.DealItemsList)
            {
                if (file.Picture != null)
                {
                    string name = Guid.NewGuid().ToString() + "_" + file.Picture.FileName;
                    file.PictureName = name;
                    if (!System.IO.Directory.Exists(Server.MapPath("~/Deals/")))
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Deals/"));
                    file.Picture.SaveAs(Server.MapPath("~/Deals/" + name));
                }
                else
                {
                    file.PictureName = "default_product.png";
                }
            }
            ProductsHandler.SaveDeal(model);
            return RedirectToAction("ManageDeals");
        }

        public ActionResult DeleteDeal(int id)
        {
            ProductsHandler.DeleteDeal(id);
            return RedirectToAction("ManageDeals");
        }

        public ActionResult ActivateDeactivateDeal(int id)
        {
            ProductsHandler.ActivateDeactivateDeal(id);
            return RedirectToAction("ManageDeals");
        }

        public ActionResult MergeDealProducts(int id)
        {
            ViewBag.DealItem = ProductsHandler.GetDealItem(id);
            ViewBag.DealID = id;
            ViewBag.Deal = ProductsHandler.GetDeal(id);
            return View(ProductsHandler.MergeDealProducts(id));
        }

        public ActionResult MapDealProducts(string data, int dealid)
        {
            return Json(ProductsHandler.MapDealProducts(data, dealid), JsonRequestBehavior.AllowGet);
        }
    }
}
