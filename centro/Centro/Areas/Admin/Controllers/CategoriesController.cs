using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer.Classes;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Handler;
using BusinessLayer.Models.DataModel;

namespace Centro.Areas.Admin.Controllers
{
    [AuthenticateUser]
    public class CategoriesController : AdminBaseController
    {

        public ActionResult ManageCategories()
        {
            ViewBag.CategoryActionList = CommonMethods.GetListFromEnum(typeof(ActionOnCategory), "Select Option");
            var list = CategoriesHandler.CategoryPagingAdmin(1, 20, "Name", "Asc", "");
            return View(list);
        }

        public JsonResult CategoryPaging(int page_no, int per_page_result, string sortOrder, string sortColumn, string name)
        {
            var list = CategoriesHandler.CategoryPagingAdmin(page_no, per_page_result, sortColumn, sortOrder, name);
            string data = RenderRazorViewToString("_Categories", list);
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

        public ActionResult AddCategory()
        {
            ViewBag.EditCategory = false;
            return View(new Category());
        }

        public ActionResult EditCategory(Int32 id)
        {
            Category category = CategoriesHandler.GetCategoryByID(id);
            ViewBag.EditCategory = true;
            category.UpdatedCategoryName = category.Name;
            return View("AddCategory", category);
        }

        [HttpPost]
        public JsonResult AddEditCategory(Category obj)
        {
            if (obj.CategoryID == 0)
            {

                if (ModelState.IsValid)
                {
                    var result = CategoriesHandler.AddCategory(obj, CentroUsers.LoggedInUser.Id);
                    return Json(new ActionOutput
                    {
                        Status = result.Status,
                        Message = result.Message,
                        Results = new List<string> 
                            { 
                                Url.Action("ManageCategories", "Categories") 
                            }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    return Json(new ActionOutput
                    {
                        Status = ActionStatus.Error,
                        Message = "Some Error Occured Please try later."
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                obj.Name = obj.UpdatedCategoryName;
                var result = CategoriesHandler.UpdateCategory(obj, CentroUsers.LoggedInUser.Id);
                return Json(new ActionOutput
                {
                    Status = result.Status,
                    Message = result.Message,
                    Results = new List<string> 
                            { 
                                Url.Action("ManageCategories", "Categories") 
                            }
                }, JsonRequestBehavior.AllowGet);
            }


        }

        public JsonResult UpdateCategories(CategoryAction obj)
        {
            var result = CategoriesHandler.UpdateCategories(obj, CentroUsers.LoggedInUser.Id);
            return Json(new ActionOutput
            {
                Status = result.Status,
                Message = result.Message,
                Results = new List<string> 
                { 
                    Url.Action("ManageCategories"),
                    result.Results !=null&& result.Results.Count>0? result.Results[0]:""
                }
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [SkipAuthentication]
        public JsonResult UniqueCategoryname(string Name)
        {
            return Json(Utility.UniqueCategoryname(Name));
        }

        public ActionResult SubCategories()
        {
            var list = CategoriesHandler.SubCategoryPagingAdmin(1, 20, "Name", "Asc", "");
            return View(list);
        }

        public JsonResult SubCategoryPaging(int page_no, int per_page_result, string sortOrder, string sortColumn, string name)
        {
            var list = CategoriesHandler.SubCategoryPagingAdmin(page_no, per_page_result, sortColumn, sortOrder, name);
            string data = RenderRazorViewToString("_SubCategories", list);
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

        public ActionResult AddSubCategory(int? id)
        {
            SubCategory subcategory = id.HasValue ? CategoriesHandler.GetSubCategory(id.Value) : new SubCategory();                        
            return View(subcategory);
        }

        public JsonResult AddEditSubCategory(SubCategory model)
        {
            return Json(CategoriesHandler.AddEditSubCategory(model), JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteSubCat(string Ids)
        {
            List<int> SubCatIds = Ids.Split(',').Select(m => int.Parse(m)).ToList();
            return Json(CategoriesHandler.DeleteSubCat(SubCatIds), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Types()
        {
            var list = CategoriesHandler.TypesPagingAdmin(1, 20, "Name", "Asc", "");
            return View(list);
        }

        public JsonResult TypesPaging(int page_no, int per_page_result, string sortOrder, string sortColumn, string name)
        {
            var list = CategoriesHandler.TypesPagingAdmin(page_no, per_page_result, sortColumn, sortOrder, name);
            string data = RenderRazorViewToString("_Types", list);
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

        public ActionResult AddType(int? id)
        {
            BusinessLayer.Models.DataModel.Type type = id.HasValue ? CategoriesHandler.GetType(id.Value) : new BusinessLayer.Models.DataModel.Type();
            return View(type);
        }

        public JsonResult AddEditType(BusinessLayer.Models.DataModel.Type model)
        {
            return Json(CategoriesHandler.AddEditType(model), JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteType(string Ids)
        {
            List<int> TypeIds = Ids.Split(',').Select(m => int.Parse(m)).ToList();
            return Json(CategoriesHandler.DeleteType(TypeIds), JsonRequestBehavior.AllowGet);
        }
    }
}