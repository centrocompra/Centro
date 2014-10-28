using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer.Classes;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Handler;
using BusinessLayer.Models.ViewModel;
using PayPal.AdaptivePayments.Model;
using System.Web.Script.Serialization;
using PayPal.AdaptivePayments;

namespace Centro.Controllers
{
    [NoCache]
    public class HomeController : FrontEndBaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            /*
            string controllerName = filterContext.Controller.GetType().Name;
            string actionName = filterContext.ActionDescriptor.ActionName;
            string userName = filterContext.HttpContext.User.Identity.Name;

            if (controllerName == "HomeController" && actionName == "Index" && CentroUsers.LoggedInUser!=null)
            {
                if (CentroUsers.LoggedInUser.UserRole == UserRole.Buyer) filterContext.Result = RedirectToAction(CentroDefaults.BuyerDefaultPage, CentroDefaults.BuyerSection);
                else if (CentroUsers.LoggedInUser.UserRole == UserRole.Seller) filterContext.Result = RedirectToAction(CentroDefaults.SellerDefaultPage, CentroDefaults.SellerSection);
                else if (CentroUsers.LoggedInUser.UserRole == UserRole.Administrator) filterContext.Result = RedirectToAction(CentroDefaults.AdminDefaultPage, CentroDefaults.AdminSection);
            }
            */
            base.OnActionExecuting(filterContext);
        }

        [SkipAuthentication]
        public ActionResult Index()
        {
            ViewBag.LastViewedProducts = ProductsHandler.LastViewedProducts(6).List;
            ViewBag.IsFeatured = true;
            ViewBag.HubList = HubHandler.GetRandomHubs(2).List;
            return View(ProductsHandler.FeaturedProductsPaging(1, 8, "ProductID", "DESC", ""));
        }

        #region ForgotPassword
        [SkipAuthentication]
        public ActionResult ForgotPassword()
        {

            return View();
        }

        [HttpPost]
        [SkipAuthentication]
        public JsonResult ForgotPassword(User obj)
        {
            // Login the activated user
            obj.EmailId = obj.PasswordRecoveryEmailId;
            var result = UsersHandler.GetUserByEmail(obj.EmailId, false);
            if (result.Status == ActionStatus.Successfull && result.Object.IsVerified)
            {
                String resetUrl = "";
                string code = Server.UrlEncode(Utility.EncryptString(obj.EmailId));
                if (Request.Url.Host.ToLower() == "localhost")
                    resetUrl = "http://" + Request.Url.Host + ":" + Request.Url.Port + "/Home/ResetPassword?u=" + code;
                else
                    resetUrl = "http://" + Request.Url.Host + "/Home/ResetPassword?u=" + code;
                UsersHandler.SendUserPassword(result.Object, resetUrl);
                return Json(new ActionOutput { Status = ActionStatus.Successfull, Message = "" }, JsonRequestBehavior.AllowGet);
            }
            else if (result.Object != null && !result.Object.IsVerified)
                return Json(new ActionOutput { Status = ActionStatus.Error, Message = string.Format("You have not verified your email yet. Please click <a href=\"javascript:;\" onclick=\"Resend({0}, this)\">here</a> to resend the verification email.", result.Object.UserID) }, JsonRequestBehavior.AllowGet);
            return Json(new ActionOutput { Status = ActionStatus.Error, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ResetPassword
        [SkipAuthentication]
        public ActionResult ResetPassword(String u)
        {
            if (String.IsNullOrEmpty(u))
                return RedirectToAction("Index", "Home");
            else
            {
                ViewBag.Code = u;
            }
            return View(new User());
        }

        [SkipAuthentication]
        [HttpPost]
        public JsonResult ResetPassword(User u, string code)
        {
            var result = UsersHandler.ResetPassword(code, u.Password);
            if (result.Status == ActionStatus.Successfull)
            {
                return Json(new ActionOutput { Status = ActionStatus.Successfull, Message = result.Message, Results = new List<string> { Url.Action("SignIn", "Home") } }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Invalid reset password link" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        [SkipAuthentication]
        public ActionResult AboutUs()
        {
            return View(CMSHandler.Page("about us"));
        }

        [SkipAuthentication]
        public ActionResult ContactUs()
        {
            return View(CMSHandler.Page("Contact Us"));
        }

        [SkipAuthentication]
        public ActionResult FAQ()
        {
            return View(CMSHandler.Page("FAQ"));
        }

        [SkipAuthentication]
        public ActionResult SignupSuccess(int id)
        {
            return View(id);
        }

        #region BillingAddress
        public ActionResult ManageBillingAddress()
        {

            ViewBag.BillingAddress = UsersHandler.GetBillingAddress(SiteUserDetails.LoggedInUser.Id).List;
            return View();
        }

        [NoCache]
        public PartialViewResult _BillingAddress(int? billingAddressId)
        {
            ViewBag.Countries = SellersHandler.CountryGetAll().List;
            BillingAddress billingAddress;
            if (!billingAddressId.HasValue)
            {
                billingAddress = new BillingAddress();
                ViewBag.States = SellersHandler.GetStateByCountryId(0).List;
                ViewBag.Cities = SellersHandler.GetCityByStateId(0).List;
            }
            else
            {
                billingAddress = UsersHandler.GetBillingAddressByID(SiteUserDetails.LoggedInUser.Id, billingAddressId.Value);
                ViewBag.States = SellersHandler.GetStateByCountryId(billingAddress == null ? 0 : billingAddress.CountryId).List;
                ViewBag.Cities = SellersHandler.GetCityByStateId(billingAddress == null ? 0 : billingAddress.StateId).List;
            }

            return PartialView(billingAddress);
        }

        [HttpPost]
        public JsonResult BillingAddress(BillingAddress obj)
        {
            ActionOutput output = UsersHandler.AddUpdateBillingAddress(SiteUserDetails.LoggedInUser.Id, obj);
            return Json(output, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Search
        [SkipAuthentication]
        public ActionResult Search()
        {
            ViewBag.CentroUsers = SiteUserDetails;
            string search = Request["search"];
            string type = Request["searchType"];
            ViewBag.Search = search;
            ViewBag.Type = type??"All";
            ViewBag.LastViewedProducts = ProductsHandler.LastViewedProducts(6).List;
            ViewBag.IsFeatured = true;
            ViewBag.HubList = HubHandler.GetRandomHubs(2).List;
            return View(ProductsHandler.ProductsSearch(1, 12, "CreatedOn", "Desc", search, type));
        }

        [SkipAuthentication]
        public JsonResult _SearchPaging(int page_no, int per_page_result, string sortOrder, string sortColumn, string search, string type)
        {
            var list = ProductsHandler.ProductsSearch(page_no, per_page_result, sortColumn, sortOrder, search, type);
            string data = RenderRazorViewToString("_Products", list);
            return Json(new ActionOutput
            {
                Status = list.Status,
                Message = list.Message,
                Results = new List<string> 
                { 
                    data, list.TotalCount.ToString() 
                }
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        [SkipAuthentication]
        public JsonResult AddToFavorite(int ID, bool ForProduct)
        {
            if (SiteUserDetails.LoggedInUser != null)
                return Json(UsersHandler.AddToFavotire(ID, ForProduct, SiteUserDetails.LoggedInUser.Id), JsonRequestBehavior.AllowGet);
            return Json(new ActionOutput { Status = ActionStatus.Error }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RemoveFromFavorite(int ID, bool ForProduct)
        {
            if (SiteUserDetails.LoggedInUser != null)
                return Json(UsersHandler.RemoveFromFavorite(ID, ForProduct, SiteUserDetails.LoggedInUser.Id), JsonRequestBehavior.AllowGet);
            return Json(new ActionOutput { Status = ActionStatus.Error }, JsonRequestBehavior.AllowGet);
        }

        // [SkipAuthentication]
        public JsonResult Follow(int id)
        {
            if (SiteUserDetails.LoggedInUser != null)
            {
                var list = AccountActivityHandler.GetFollower(SiteUserDetails.LoggedInUser.Id, id).List;
                ViewBag.followedID = id;
                return Json(new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Results = new List<string>
                {
                    RenderRazorViewToString("Follow", new FollowerFavoriteViewModel{ Favorites=null, Followers=list})
                }
                });
            }
            else
            {
                return Json(new ActionOutput { Status = ActionStatus.Error }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult FollowUser()
        {
            int followed_id = Convert.ToInt32(Request["FollowedID"]);
            string[] followArr = Request["FollowArr"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (followArr.Length > 0)
            {
                var result = AccountActivityHandler.SaveFollower(followArr.Select(m => Convert.ToInt32(m)).ToArray(), SiteUserDetails.LoggedInUser.Id, followed_id);
            }
            else
            {
                var result = AccountActivityHandler.RemoveFolower(SiteUserDetails.LoggedInUser.Id, followed_id);
            }
            return Json(new ActionOutput { Status = ActionStatus.Successfull, Message = "Request has been processed." }, JsonRequestBehavior.AllowGet);
        }

        [SkipAuthentication]
        public ActionResult help()
        {
            return View(CMSHandler.Page("help"));
        }

        [SkipAuthentication]
        public ActionResult CareerBlogs()
        {
            return View(CMSHandler.Page("Careers Blog"));
        }

        [SkipAuthentication]
        public ActionResult CancelReturns()
        {
            return View(CMSHandler.Page("Cancellation & Returns "));
        }

        [SkipAuthentication]
        public ActionResult Payments()
        {
            return View(CMSHandler.Page("Payments "));
        }

        [SkipAuthentication]
        public ActionResult Shipping()
        {
            return View(CMSHandler.Page("Shipping"));
        }

        [SkipAuthentication]
        public ActionResult Press()
        {
            return View(CMSHandler.Page("Press"));
        }
        [SkipAuthentication]
        public ActionResult Misc()
        {
            return View(CMSHandler.Page("Misc"));
        }
        [SkipAuthentication]
        public ActionResult Affiliate()
        {
            return View(CMSHandler.Page("Affiliate"));
        }
        
        [SkipAuthentication]
        public ActionResult eGiftVoucher()
        {
            return View(CMSHandler.Page("e-Gift Voucher"));
        }
    }
}
