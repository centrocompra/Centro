using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer.Handler;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Classes;
using BusinessLayer.Models.DataModel;

namespace Centro.Controllers
{
    public class ProductsController : FrontEndBaseController
    {
        [SkipAuthentication]
        public ActionResult Index()
        {
            ViewBag.CentroUsers = SiteUserDetails;
            Shop shop = SiteUserDetails.LoggedInUser != null ? SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object : null;
            var model = ProductsHandler.ProductsSearch(1, 20, "CreatedOn", "Desc", "", "");
            return View(model);
        }

        [SkipAuthentication]
        public ActionResult Category(string id)
        {
            id = Utility.HifenToSpace(id);
            ViewBag.Category = id;
            return View(ProductsHandler.ProductsPaging(1, 12, "ProductID", "DESC", "", id, "",""));
        }

        [SkipAuthentication]
        public ActionResult ExclusiveDeals(int id, int? itemid)
        {
            ViewBag.Deal = ProductsHandler.GetDeal(id);
            if (itemid.HasValue)
            {
                ViewBag.DealItem = ProductsHandler.GetDealItemByID(itemid.Value);
            }
            return View(ProductsHandler.ExclusiveDeals(1, 12, "ProductID", "DESC", id, itemid));
        }

        [SkipAuthentication]
        public ActionResult SubCategory(int id, string cat, string name)
        {
            ViewBag.LoggedInUser = SiteUserDetails.LoggedInUser;
            name = Utility.HifenToSpace(name);
            cat = Utility.HifenToSpace(cat);
            ViewBag.Category = cat;
            ViewBag.SubCategory = name;
            ViewBag.SubCategoryID = id;
            return View(ProductsHandler.ProductsPaging(1, 12, "ProductID", "DESC", "", cat, name, ""));
        }

        [SkipAuthentication]
        public ActionResult Type(int id, string cat, string name, string type)
        {
            ViewBag.LoggedInUser = SiteUserDetails.LoggedInUser;
            name = Utility.HifenToSpace(name);
            cat = Utility.HifenToSpace(cat);
            ViewBag.Category = cat;
            ViewBag.SubCategory = name;
            ViewBag.Type = type;
            ViewBag.TypeID = id;
            return View(ProductsHandler.ProductsPaging(1, 12, "ProductID", "DESC", "", cat, name, type));
        }

        [SkipAuthentication]
        public JsonResult ProductsPaging(int page_no, int per_page_result, string sortOrder, string sortColumn, string search, string category)
        {
            var list = ProductsHandler.ProductsPaging(page_no, per_page_result, sortColumn, sortOrder, search, category,"","");

            string data = RenderRazorViewToString("_Products", list);
            return Json(new ActionOutput
            {
                Status = list.Status,
                Message = list.Message,
                Results = new List<string> 
                { 
                    data, list.TotalCount.ToString(),list.List.Count().ToString() 
                }
            }, JsonRequestBehavior.AllowGet);
        }

        [SkipAuthentication]
        public JsonResult FeaturedProductsPaging(int page_no, int per_page_result, string sortOrder, string sortColumn, string search)
        {
            var list = ProductsHandler.FeaturedProductsPaging(page_no, per_page_result, sortColumn, sortOrder, search);
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

        [SkipAuthentication]
        public ActionResult Product(string shopname, int product_id)
        {
            // Set last viewed date
            shopname = Utility.HifenToSpace(shopname);
            ProductsHandler.SetLastViewedDate(product_id);
            ViewBag.IsShopFavorite = false;
            ViewBag.CentroUsers = SiteUserDetails.LoggedInUser != null ? SiteUserDetails : null;
            Shop shop = SellersHandler.ShopByShopName(shopname).Object;
            if (SiteUserDetails.LoggedInUser != null)
            {
                ViewBag.UserId = SiteUserDetails.LoggedInUser.Id;
                var fav_result = UsersHandler.CheckIsShopFavoriteByUserID(SiteUserDetails.LoggedInUser.Id, shop.ShopID).Object;
                if (fav_result != null)
                {
                    ViewBag.IsShopFavorite = true;
                }
            }
            else
                ViewBag.UserId = 0;
            Product prod = ProductsHandler.ProductById(product_id).Object;
            ViewBag.Shop = shop;
            ViewBag.Product = prod;
            ViewBag.ShopOwner = SellersHandler.ShopOwnerByShopName(shopname).Object;
            ViewBag.ShopProducts = ProductsHandler.ProductsByShopId(shop.ShopID).List;
            ViewBag.SimilarProducts = ProductsHandler.SimilarProducts(prod.CategoryId).List;
            //ViewBag.FeedBack = FeedBackHandler.GetFeedBackByProductID(product_id).List;

            return View();
        }

        [SkipAuthentication]
        public ActionResult Contests(string shopname, int shopid)
        {
            // Set last viewed date
            shopname = Utility.HifenToSpace(shopname);
            ViewBag.IsShopFavorite = false;
            ViewBag.CentroUsers = SiteUserDetails.LoggedInUser != null ? SiteUserDetails : null;
            Shop shop = SellersHandler.ShopByShopName(shopname).Object;
            if (SiteUserDetails.LoggedInUser != null)
            {
                ViewBag.UserId = SiteUserDetails.LoggedInUser.Id;
                var fav_result = UsersHandler.CheckIsShopFavoriteByUserID(SiteUserDetails.LoggedInUser.Id, shop.ShopID).Object;
                if (fav_result != null)
                {
                    ViewBag.IsShopFavorite = true;
                }
            }
            ViewBag.UserId = shop.UserId;
            //Product prod = ProductsHandler.ProductById(product_id).Object;
            var model = ContestHandler.GetContestListing(1, 9, "Desc", "CreatedOn", "", null, shop.UserId);
            ViewBag.Shop = shop;
            ViewBag.ShopOwner = SellersHandler.ShopOwnerByShopName(shopname).Object;
            ViewBag.ShopProducts = ProductsHandler.ProductsByShopId(shop.ShopID).List;

            return View(model);
        }

        [SkipAuthentication]
        public JsonResult _Contest(int page_no, int per_page_result, string sortOrder, string sortColumn, string search, int? CategoryID, int? UserID)
        {

            PagingResult<ContestViewModel> model = ContestHandler.GetContestListing(page_no, per_page_result, sortOrder, sortColumn, search, CategoryID, UserID);
            string view = RenderRazorViewToString("_ContestList", model.List);
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string> { view, model.TotalCount.ToString(), model.Message }
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
