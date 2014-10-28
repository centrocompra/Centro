using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Models.DataModel;
using System.Data.Objects;
using BusinessLayer.Classes;

namespace BusinessLayer.Handler
{
    public class CategoriesHandler
    {
        /// <summary>
        /// Add Category
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ActionOutput AddCategory(Category obj, Int32 logged_in_user_id)
        {
            using (var context = new CentroEntities())
            {
                //Category c = context.Categories.Where(m => m.Name.Equals(obj.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                //if (c != null)
                //{
                //    return new ActionOutput { Status = ActionStatus.Error, ID = 0, Message = "Category already exist." };
                //}

                obj.CreatedBy = logged_in_user_id;
                obj.CreatedOn = DateTime.Now;
                context.Categories.AddObject(obj);
                if (context.SaveChanges() > 0)
                {
                    return new ActionOutput
                    {
                        Status = ActionStatus.Successfull,
                        ID = obj.CategoryID,
                        Message = "Category Added Successfully."
                    };
                }
                return new ActionOutput { Status = ActionStatus.Error, ID = 0, Message = "Error Occured." };
            }
        }

        /// <summary>
        /// Update Category
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ActionOutput UpdateCategory(Category obj, Int32 logged_in_user_id)
        {
            using (var context = new CentroEntities())
            {
                Category c = context.Categories.Where(m => m.CategoryID.Equals(obj.CategoryID)).FirstOrDefault();
                if (c == null)
                {
                    return new ActionOutput { Status = ActionStatus.Error, ID = 0, Message = "Category not exist." };
                }
                if (!obj.Name.Equals(c.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    var result = CategoriesHandler.GetCategoryByname(obj.Name);

                    if (result != null)
                    {
                        if (result.CategoryID != c.CategoryID)
                        {
                            return new ActionOutput { Status = ActionStatus.Error, ID = 0, Message = "Category name already exist.Please try another Category name" };
                        }
                    }
                    else
                    {
                        c.Name = obj.Name;
                        c.Description = obj.Description;
                        c.Published = obj.Published;
                        c.UpdatedBy = logged_in_user_id;
                        obj.UpdatedOn = DateTime.Now;

                    }
                }
                else
                {
                    c.Name = obj.Name;
                    c.Description = obj.Description;
                    c.Published = obj.Published;
                    c.UpdatedBy = logged_in_user_id;
                    obj.UpdatedOn = DateTime.Now;

                }




                context.SaveChanges();
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    ID = obj.CategoryID,
                    Message = "Category Updated Successfully."
                };

            }
        }

        /// <summary>
        /// Get Category By Id
        /// </summary>
        /// <param name="category_id"></param>
        /// <returns></returns>
        public static Category GetCategoryByID(Int32 category_id)
        {
            using (var context = new CentroEntities())
            {
                Category c = context.Categories.Where(m => m.CategoryID.Equals(category_id)).FirstOrDefault();
                return c;

            }
        }

        /// <summary>
        /// Get Category By Name
        /// </summary>
        /// <param name="category_id"></param>
        /// <returns></returns>
        public static Category GetCategoryByname(String category_name)
        {
            using (var context = new CentroEntities())
            {
                Category c = context.Categories.Where(m => m.Name.Equals(category_name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                return c;

            }
        }

        /// <summary>
        /// Returne a paged result for the categories
        /// </summary>
        public static PagingResult<CategoryListing_Result> CategoryPaging(int page_no, int per_page_result, string sortColumn, string sortOrder, string name)
        {
            using (var context = new CentroEntities())
            {
                ObjectParameter output = new ObjectParameter("TotalCount", typeof(int));
                var list = context.CategoryListing(page_no, per_page_result, sortColumn, sortOrder, name, output).ToList();
                PagingResult<CategoryListing_Result> pagingResult = new PagingResult<CategoryListing_Result>();
                pagingResult.List = list;
                pagingResult.TotalCount = Convert.ToInt32(output.Value);
                pagingResult.Status = ActionStatus.Successfull;
                return pagingResult;
            }
        }

        /// <summary>
        /// Returne a paged result for the categories
        /// </summary>
        public static PagingResult<CategoryListingAdmin_Result> CategoryPagingAdmin(int page_no, int per_page_result, string sortColumn, string sortOrder, string name)
        {
            using (var context = new CentroEntities())
            {
                ObjectParameter output = new ObjectParameter("TotalCount", typeof(int));
                var list = context.CategoryListingAdmin(page_no, per_page_result, sortColumn, sortOrder, name, output).ToList();
                PagingResult<CategoryListingAdmin_Result> pagingResult = new PagingResult<CategoryListingAdmin_Result>();
                pagingResult.List = list;
                pagingResult.TotalCount = Convert.ToInt32(output.Value);
                pagingResult.Status = ActionStatus.Successfull;
                return pagingResult;
            }
        }

        public static PagingResult<SubCategory> SubCategoryPagingAdmin(int page_no, int per_page_result, string sortColumn, string sortOrder, string name)
        {
            using (var context = new CentroEntities())
            {
                var query = context.SubCategories.Include("Category").Where("1=1");
                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where("it.Name like '" + name + "%'");

                PagingResult<SubCategory> pagingResult = new PagingResult<SubCategory>();
                pagingResult.List = query.OrderBy("it." + sortColumn + " " + sortOrder).Skip(page_no - 1)
                                        .Take(per_page_result)
                                        .ToList();
                pagingResult.TotalCount = query.Count();
                pagingResult.Status = ActionStatus.Successfull;
                return pagingResult;
            }
        }

        public static PagingResult<BusinessLayer.Models.DataModel.Type> TypesPagingAdmin(int page_no, int per_page_result, string sortColumn, string sortOrder, string name)
        {
            using (var context = new CentroEntities())
            {
                var query = context.Types.Include("SubCategory").Where("1=1");
                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where("it.Name like '" + name + "%'");

                PagingResult<BusinessLayer.Models.DataModel.Type> pagingResult = new PagingResult<BusinessLayer.Models.DataModel.Type>();
                pagingResult.List = query.OrderBy("it." + sortColumn + " " + sortOrder).Skip(page_no - 1)
                                        .Take(per_page_result)
                                        .ToList();
                pagingResult.TotalCount = query.Count();
                pagingResult.Status = ActionStatus.Successfull;
                return pagingResult;
            }
        }

        /// <summary>
        /// This will be used to bulk update from admin
        /// </summary>
        /// <param name="action_obj"></param>
        /// <param name="logged_in_user_id"></param>
        /// <returns></returns>
        public static ActionOutput UpdateCategories(CategoryAction action_obj, Int32 logged_in_user_id)
        {
            using (var context = new CentroEntities())
            {
                List<Product> products = new List<Product>();
                if (action_obj.ActionID == ActionOnCategory.Publish)
                {
                    foreach (var id in action_obj.CategoryID)
                    {
                        var category = context.Categories.Where(u => u.CategoryID.Equals(id)).FirstOrDefault();
                        category.Published = true;
                        category.UpdatedOn = DateTime.Now;
                        category.UpdatedBy = logged_in_user_id;

                        context.SaveChanges();
                    }
                }
                else if (action_obj.ActionID == ActionOnCategory.UnPublish)
                {
                    foreach (var id in action_obj.CategoryID)
                    {
                        var category = context.Categories.Where(u => u.CategoryID.Equals(id)).FirstOrDefault();
                        category.Published = false;
                        category.UpdatedOn = DateTime.Now;
                        category.UpdatedBy = logged_in_user_id;

                        context.SaveChanges();
                    }
                }
                else if (action_obj.ActionID == ActionOnCategory.Delete)
                {
                    foreach (var id in action_obj.CategoryID)
                    {
                        var category = context.Categories.Where(u => u.CategoryID.Equals(id)).FirstOrDefault();
                        var list=context.Products.Where(m => m.CategoryId == category.CategoryID).ToList();
                        products.AddRange(list);
                        if (list.Count <= 0)
                        {
                            context.DeleteObject(category);
                        }
                    }
                    context.SaveChanges();
                }
                if (products.Count > 0)
                {
                    string ids = string.Join(",", products.Select(m => m.CategoryId).Distinct().ToList());
                    return new ActionOutput { Status = ActionStatus.Error, Message = "All Red background categories has their association with some products.", Results = new List<string> { ids } };
                }
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Records Updated Successfully." };
            }            
        }

        public static ActionOutput<Category> GetCategories()
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Category> { List = context.Categories.Where(m => !m.IsDeleted).ToList(), Status = ActionStatus.Successfull };
            }
        }

        public static SubCategory GetSubCategory(int id)
        {
            using (var context = new CentroEntities())
            {
                return context.SubCategories.Where(m => m.SubCategoryID == id).FirstOrDefault();
            }
        }

        public static List<SubCategory> GetSubCategory()
        {
            using (var context = new CentroEntities())
            {
                return context.SubCategories.ToList();
            }
        }

        public static ActionOutput AddEditSubCategory(SubCategory model)
        {
            using (var context = new CentroEntities())
            {
                string msg = "";
                if (model.SubCategoryID > 0)
                {
                    var sub = context.SubCategories.Where(m => m.SubCategoryID == model.SubCategoryID).FirstOrDefault();
                    sub.Name = model.Name;
                    sub.CategoryID = model.CategoryID;
                    msg = "Sub category updated.";
                }
                else
                {
                    context.SubCategories.AddObject(model);
                    msg = "Sub category added.";
                }
                context.SaveChanges();
                return new ActionOutput { Message = msg, Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput DeleteSubCat(List<int> SubCatIds)
        {
            using (var context = new CentroEntities())
            {
                var subs = context.SubCategories.Where(m => SubCatIds.Contains(m.SubCategoryID)).ToList();
                var types = context.Types.Where(m => SubCatIds.Contains(m.SubCategoryID.Value)).ToList();
                foreach (var type in types)
                {
                    context.Types.DeleteObject(type);
                }

                foreach (var sub in subs)
                {
                    context.SubCategories.DeleteObject(sub);
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Selected sub categories deleted." };
            }
        }

        public static BusinessLayer.Models.DataModel.Type GetType(int id)
        {
            using (var context = new CentroEntities())
            {
                return context.Types.Where(m => m.TypeID== id).FirstOrDefault();
            }
        }

        public static ActionOutput AddEditType(BusinessLayer.Models.DataModel.Type model)
        {
            using (var context = new CentroEntities())
            {
                string msg = "";
                if (model.TypeID > 0)
                {
                    var sub = context.Types.Where(m => m.TypeID == model.TypeID).FirstOrDefault();
                    sub.Name = model.Name;
                    sub.SubCategoryID = model.SubCategoryID;
                    msg = "Type updated.";
                }
                else
                {
                    context.Types.AddObject(model);
                    msg = "Type added.";
                }
                context.SaveChanges();
                return new ActionOutput { Message = msg, Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput DeleteType(List<int> SubCatIds)
        {
            using (var context = new CentroEntities())
            {
                var types = context.Types.Where(m => SubCatIds.Contains(m.TypeID)).ToList();
                foreach (var type in types)
                {
                    context.Types.DeleteObject(type);
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Selected types deleted." };
            }
        }
    }
}