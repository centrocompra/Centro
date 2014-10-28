using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer.Classes;
using BusinessLayer.Handler;
using System.Web.Security;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Models.DataModel;


namespace Centro.Controllers
{
    [NoCache]
    public class FeedBackController : FrontEndBaseController
    {
        //
        // GET: /FeedBack/



        public ActionResult Index()
        {
            return View();
        }

        [SkipAuthentication]
        public ActionResult ShopFeedBack(string ShopName, int ShopId)
        {
            ShopName = Utility.HifenToSpace(ShopName);
            ViewBag.ShopName = ShopName;
            ViewBag.CentroUsers = SiteUserDetails.LoggedInUser != null ? SiteUserDetails : null;
            ViewBag.IsShopFavorite = false;
            //User user = UsersHandler.UserByUsername(UserName).Object;
            User user = SellersHandler.ShopOwnerByShopName(ShopName).Object;
            if (user.ProfilePicUrl != null)
            {
                string profileImg = user.ProfilePicUrl;
                profileImg = profileImg.Replace(user.UserName + "/", user.UserName + "/thumb_");
                user.ProfilePicUrl = profileImg;
            }
            //Hub obj = HubHandler.GetHubByHubTitle(Utility.HifenToSpace(HubTitle), user.UserID).Object;
            ViewBag.User = user;
            Shop shop = SellersHandler.ShopByUserId(user.UserID).Object;
            ViewBag.Shop = shop;
            if (SiteUserDetails.LoggedInUser != null)
            {
                ViewBag.UserId = SiteUserDetails.LoggedInUser.Id;
                var fav_result = shop != null ? UsersHandler.CheckIsShopFavoriteByUserID(SiteUserDetails.LoggedInUser.Id, shop.ShopID).Object : null;
                if (fav_result != null)
                {
                    ViewBag.IsShopFavorite = true;
                }
            }
            else
                ViewBag.UserId = 0;
            var result = FeedBackHandler.GetFeedBackByShopID(1, 20, "CreatedOn", "desc", "", ShopId).List;
            return View(result);
        }


        public JsonResult LeaveFeedBackPartial(int id, int type, string requestType = "")
        {
            ViewBag.Action = type;
            Feedback obj = new Feedback();
            if (type == 1)
            {


            }
            else
            {
                if (requestType.ToLower() == "byorder")
                {
                    obj.ShopID = SellersHandler.GetShopByOrderID(id).Object.ShopID;
                    obj.OrderID = id;
                }
                else if (requestType.ToLower() == "byrequest")
                {
                    obj.ShopID = SellersHandler.GetShopByRequestID(id).Object.ShopID;
                    obj.RequestID = id;
                }

            }

            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string> 
                { 
                    RenderRazorViewToString("LeaveFeedBackPartial",obj )
                }
            });
        }

        [HttpPost]
        public JsonResult LeaveFeedBack(Feedback obj)
        {
            obj.UserID = SiteUserDetails.LoggedInUser.Id;
            if (obj.Rating == null)
                obj.Rating = 0;
            if (obj.ProductID > 0)
                obj.FeedBackType = 1;
            else
                obj.FeedBackType = 2;
            var result = FeedBackHandler.SaveFeedBack(obj);
            return Json(result, JsonRequestBehavior.AllowGet);
        }



    }
}
