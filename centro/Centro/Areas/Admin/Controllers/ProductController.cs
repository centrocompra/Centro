using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer.Classes;
using BusinessLayer.Handler;
using BusinessLayer.Models.ViewModel;

namespace Centro.Areas.Admin.Controllers
{
    public class ProductController : AdminBaseController
    {
        //
        // GET: /CentroAdmin/Product/

        public ActionResult Index()
        {
            return View();
        }

      

        [AuthenticateUser]
        public ActionResult Category(string id)
        {
            var cat = ProductsHandler.GetCategory().List; ;
            ViewBag.Categories = cat;
            if (id == null || id == "")
            {
                id = cat[0].Name;
            }
            ViewBag.Category = id;
            return View(ProductsHandler.ProductsPagingAdmin(1, 12, "ProductID", "DESC", "", id));
        }

        [AuthenticateUser]
        public JsonResult ProductsPaging(int page_no, int per_page_result, string sortOrder, string sortColumn, string search, string category)
        {
            var list = ProductsHandler.ProductsPagingAdmin(page_no, per_page_result, sortColumn, sortOrder, search, category);
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

        [AuthenticateUser]
        public JsonResult SetAsFeatured(string allProducts, string selectedProducts)
        {
            List<int> AllProducts = new List<int>(allProducts.Split(',').Select(int.Parse));
            List<int> SelectedProducts = !string.IsNullOrEmpty(selectedProducts) ? new List<int>(selectedProducts.Split(',').Select(int.Parse)) : null;
            return Json(ProductsHandler.SetAsFeatured(AllProducts, SelectedProducts), JsonRequestBehavior.AllowGet);
        }

        [AuthenticateUser]
        public JsonResult MoveToCategory(int CategoryID, string Pids)
        {
            List<int> SelectedProducts = !string.IsNullOrEmpty(Pids) ? new List<int>(Pids.Split(',').Select(int.Parse)) : null;
            return Json(ProductsHandler.MoveToCategory(CategoryID, SelectedProducts), JsonRequestBehavior.AllowGet);
        }
    }
}
