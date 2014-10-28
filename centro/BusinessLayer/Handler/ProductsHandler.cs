using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Classes;
using System.Data.Objects;

namespace BusinessLayer.Handler
{
    public class ProductsHandler
    {
        /// <summary>
        /// Create Product
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ActionOutput CreateProduct(Product obj, int user_id)
        {
            using (var context = new CentroEntities())
            {
                var shop = context.Shops.Where(m => m.UserId == user_id).FirstOrDefault();
                int count = context.Products.Where(m => m.CreatedOn.Value.Month == DateTime.Now.Month && m.CreatedOn.Value.Year == DateTime.Now.Year).Count();

                obj.CreatedBy = user_id;
                obj.CreatedOn = DateTime.Now;
                obj.DeletedBy = null;
                obj.DeletedOn = null;
                obj.IsDeleted = false;
                obj.LastViewedOn = null;
                obj.UpdatedBy = null;
                obj.UpdatedOn = null;
                obj.ProductNumberID = shop.ShopNumberID.Substring(0, 4) + count;

                context.Products.AddObject(obj);
                if (context.SaveChanges() > 0)
                {
                    // Saving tags & materials
                    var masterTagsAll = context.Tags.ToList();
                    var masterProductTags = masterTagsAll.Where(m => m.TagType == (int)TagType.ProductTag).ToList();
                    var masterProductMaterials = masterTagsAll.Where(m => m.TagType == (int)TagType.ProductMaterial).ToList();
                    var existingProductTags = context.ProductTags.Where(m => m.ProductId == obj.ProductID).ToList();
                    List<int> productTags = new List<int>();

                    if (obj.Tags != null && obj.Tags.Length > 0)
                    {
                        string[] tags = obj.Tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        // Save product tags 
                        foreach (var tag in tags)
                        {
                            var existingTag = masterProductTags.Where(t => t.TagText.Equals(tag, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                            if (existingTag != null && !existingProductTags.Where(t => t.TagId == existingTag.TagID).Any())
                            {
                                productTags.Add(existingTag.TagID);
                            }
                            else if (!existingProductTags.Where(t => t.TagId == existingTag.TagID).Any())
                            {
                                // Make entry in master table
                                Tag t = new Tag();
                                t.TagText = tag;
                                t.TagType = (int)TagType.ProductTag;
                                context.Tags.AddObject(t);
                                context.SaveChanges();

                                productTags.Add(t.TagID);
                            }
                        }
                    }
                    if (obj.Materials != null && obj.Materials.Length > 0)
                    {
                        // Save product materials 
                        string[] materials = obj.Materials.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var material in materials)
                        {
                            var existingMaterial = masterProductMaterials.Where(t => t.TagText.Equals(material, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                            if (existingMaterial != null && !existingProductTags.Where(t => t.TagId == existingMaterial.TagID).Any())
                            {
                                productTags.Add(existingMaterial.TagID);
                            }
                            else if (!existingProductTags.Where(t => t.TagId == existingMaterial.TagID).Any())
                            {
                                // Make entry in master table
                                Tag t = new Tag();
                                t.TagText = material;
                                t.TagType = (int)TagType.ProductMaterial;
                                context.Tags.AddObject(t);
                                context.SaveChanges();

                                productTags.Add(t.TagID);
                            }
                        }
                    }
                    if (productTags.Count > 0)
                    {
                        foreach (int id in productTags)
                        {
                            context.ProductTags.AddObject(new ProductTag { TagId = id, ProductId = obj.ProductID });
                        }
                        context.SaveChanges();
                    }



                    return new ActionOutput
                    {
                        Status = ActionStatus.Successfull,
                        ID = obj.ProductID,
                        Message = "Product has been created Successfully.",
                        Results = new List<string> { obj.Title }
                    };
                }
                return new ActionOutput { Status = ActionStatus.Error, ID = 0, Message = "Error Occured." };
            }
        }

        /// <summary>
        /// Get products by shop
        /// </summary>
        /// <param name="shop_id"></param>
        /// <returns></returns>
        public static ActionOutput<Product> ProductsByShopId(int shop_id)
        {
            using (var context = new CentroEntities())
            {
                var list = context.Products.Where(m => m.ShopId == shop_id && !m.IsDeleted && m.Quantity > 0).ToList();

                return new ActionOutput<Product>
                {
                    Status = ActionStatus.Successfull,
                    List = list
                };
            }
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        /// <param name="category_id"></param>
        /// <returns></returns>
        public static ActionOutput<Product> ProductsByCategory(int category_id)
        {
            using (var context = new CentroEntities())
            {
                var list = context.Products.Where(m => m.CategoryId == category_id && !m.IsDeleted && m.Quantity > 0).ToList();

                return new ActionOutput<Product>
                {
                    Status = ActionStatus.Successfull,
                    List = list
                };
            }
        }

        public static ActionOutput<Category> CategoryByProduct(int product_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Category>
                {
                    Object = context.Categories.Join(context.Products,
                                                            c => c.CategoryID,
                                                            p => p.CategoryId,
                                                            (c, p) => new { c, p })
                                                            .Where(p => p.p.ProductID == product_id)
                                                            .Select(m => m.c)
                                                            .FirstOrDefault(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        /// <summary>
        /// Get last viewed products
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static ActionOutput<Product> LastViewedProducts(int count)
        {
            using (var context = new CentroEntities())
            {
                //string qu = context.Products.Join(context.Shops,
                //                                    p => p.ShopId,
                //                                    s => s.ShopID,
                //                                    (p, s) => new { p, s })
                //                             .Join(context.Users,
                //                                    p => p.s.UserId,
                //                                    u => u.UserID,
                //                                    (p, u) => new { p, u })
                //                             .Where(m => !m.p.p.IsDeleted && !m.p.s.IsClosed && m.p.p.Quantity > 0).OrderByDescending(m => m.p.p.LastViewedOn).Take(count)
                //                             .ToTraceString();

                var products = context.Products.Join(context.Shops,
                                                    p => p.ShopId,
                                                    s => s.ShopID,
                                                    (p, s) => new { p, s })
                                             .Join(context.Users,
                                                    p => p.s.UserId,
                                                    u => u.UserID,
                                                    (p, u) => new { p, u })
                                             .Where(m => !m.p.p.IsDeleted && !m.p.s.IsClosed && m.p.p.Quantity > 0 && m.p.p.LastViewedOn.HasValue).OrderByDescending(m => m.p.p.LastViewedOn).Take(count).ToList();

                for (short i = 0; i < products.Count; i++)
                {
                    products[i].p.p.ShopName = products[i].p.s.ShopName;
                    products[i].p.p.ShopOwnerUsername = products[i].u.UserName;
                }
                return new ActionOutput<Product>
                {
                    Status = ActionStatus.Successfull,
                    List = products.Select(m => m.p.p).ToList()
                };
            }
        }

        /// <summary>
        /// Get products by id
        /// </summary>
        /// <param name="product_id"></param>
        /// <returns></returns>
        public static ActionOutput<Product> ProductById(int product_id, bool isDeleted = false)
        {
            using (var context = new CentroEntities())
            {
                var product = context.Products.Where(m => m.ProductID == product_id && m.IsDeleted == isDeleted).FirstOrDefault();
                product.ProductImages = context.Pictures.Join(context.ProductPictures,
                                                                p => p.PictureID,
                                                                pp => pp.PictureId,
                                                                (p, pp) => new { p, pp })
                                                                .Where(m => m.pp.ProductId == product_id)
                                                                .Select(m => m.p).ToList();
                product.ProductShippingDetails = context.ShipingCountries.Where(m => m.ProductId == product_id)
                                                                         .Select(m => new ProductShippingDetails
                                                                         {
                                                                             ShipFromCountryName = context.Countries.Where(c => c.CountryID == m.ShipsFromId).FirstOrDefault().CountryName,
                                                                             ShipToCountryName = context.Countries.Where(c => c.CountryID == m.ShipsTo).FirstOrDefault().CountryName,
                                                                             ShippingCost = m.Cost,
                                                                             ShippingCostAfterFirst = m.CostAfterFirstProduct
                                                                         })
                                                                         .ToList();
                var product_shop = context.Products.Join(context.Shops,
                                                    p => p.ShopId,
                                                    s => s.ShopID,
                                                    (p, s) => new { p, s })
                                             .Join(context.Users,
                                                    p => p.s.UserId,
                                                    u => u.UserID,
                                                    (p, u) => new { p, u })
                                             .Where(m => m.p.p.IsDeleted == isDeleted && m.p.p.ProductID == product_id).FirstOrDefault();
                product.ShopName = product_shop.p.s.ShopName;
                product.ShopOwnerUsername = product_shop.u.UserName;
                return new ActionOutput<Product>
                {
                    Status = ActionStatus.Successfull,
                    Object = product
                };

            }
        }

        public static ActionOutput<Product> ProductByIds(List<int> productIds)
        {
            using (var context = new CentroEntities())
            {
                var products = context.Products.Where(p => productIds.Contains(p.ProductID)).ToList();
                return new ActionOutput<Product> { Status = ActionStatus.Successfull, List = products };
            }
        }

        //public static ActionOutput<Product> ProductById(List<int> product_ids)
        //{
        //    using (var context = new CentroEntities())
        //    {
        //        var products = context.Products.Where(m => product_ids.Contains(m.ProductID) && !m.IsDeleted).ToList();
        //        for (short i = 0; i < products.Count; i++)
        //        {
        //            products[i].ProductImages = context.Pictures.Join(context.ProductPictures,
        //                                                        p => p.PictureID,
        //                                                        pp => pp.PictureId,
        //                                                        (p, pp) => new { p, pp })
        //                                                        .Where(m => m.pp.ProductId == products[i].ProductID)
        //                                                        .Select(m => m.p).ToList();
        //            products[i].ProductShippingDetails = context.ShipingCountries.Where(m => m.ProductId == products[i].ProductID)
        //                                                                     .Select(m => new ProductShippingDetails
        //                                                                     {
        //                                                                         ShipFromCountryName = context.Countries.Where(c => c.CountryID == m.ShipsFromId).FirstOrDefault().CountryName,
        //                                                                         ShipToCountryName = context.Countries.Where(c => c.CountryID == m.ShipsTo).FirstOrDefault().CountryName,
        //                                                                         ShippingCost = m.Cost,
        //                                                                         ShippingCostAfterFirst = m.CostAfterFirstProduct
        //                                                                     })
        //                                                                     .ToList();

        //            var product_shop = context.Products.Join(context.Shops,
        //                                                      p => p.ShopId,
        //                                                      s => s.ShopID,
        //                                                      (p, s) => new { p, s })
        //                                               .Join(context.Users,
        //                                                      p => p.s.UserId,
        //                                                      u => u.UserID,
        //                                                      (p, u) => new { p, u })
        //                                               .Where(m => !m.p.p.IsDeleted && m.p.p.ProductID == products[i].ProductID).FirstOrDefault();
        //            products[i].ShopName = product_shop.p.s.ShopName;
        //            products[i].ShopOwnerUsername = product_shop.u.UserName;
        //        }
        //        return new ActionOutput<Product>
        //        {
        //            Status = ActionStatus.Successfull,
        //            List = products
        //        };
        //    }
        //}

        /// <summary>
        /// Save product images
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static ActionOutput SaveProductPictures(int user_id, int product_id, UserProductTempPictures list)
        {
            using (var context = new CentroEntities())
            {
                foreach (var item in list.UserTempPictures)
                {
                    Picture pic = new Picture();
                    pic.CreatedBy = user_id;
                    pic.CreatedOn = DateTime.Now;
                    pic.DeletedBy = null;
                    pic.DeletedOn = null;
                    pic.DisplayName = item.DisplayName;
                    pic.SavedName = item.SavedName;
                    pic.Thumbnail = item.Thumbnail;
                    pic.IsDeleted = false;
                    pic.MimeType = item.MimeType;
                    pic.SizeInBytes = item.SizeInBytes;
                    pic.SizeInKB = item.SizeInKB;
                    pic.SizeInMB = item.SizeInMB;
                    pic.UpdatedBy = null;
                    pic.UpdatedOn = null;
                    context.Pictures.AddObject(pic);
                    context.SaveChanges();

                    ProductPicture prod_pic = new ProductPicture();
                    prod_pic.DisplayOrder = null;
                    prod_pic.PictureId = pic.PictureID;
                    prod_pic.ProductId = product_id;
                    context.ProductPictures.AddObject(prod_pic);
                    context.SaveChanges();

                    // Move temp files to user's folder
                    Utility.MoveFile("~/Temp/" + item.Username + "/" + item.SavedName, "~/Images/ProductImages/" + item.Username + "/", item.SavedName);
                    Utility.MoveFile("~/Temp/" + item.Username + "/" + item.Thumbnail, "~/Images/ProductImages/" + item.Username + "/", item.Thumbnail);
                }
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Images have been saved." };
            }
        }

        public static ActionOutput SaveProductFile(string username, List<ProductFileViewModel> files, int product_id)
        {
            using (var context = new CentroEntities())
            {
                foreach (ProductFileViewModel file in files)
                {
                    ProductFile p = new ProductFile();
                    p.DisplayName = file.DisplayName;
                    p.MimeType = file.MimeType;
                    p.SavedName = file.SavedName;
                    p.SizeInBytes = file.SizeInBytes;
                    p.SizeInKB = file.SizeInKB;
                    p.SizeInMB = file.SizeInMB;
                    p.ProductID = product_id;
                    context.ProductFiles.AddObject(p);
                }
                context.SaveChanges();

                // Move temp files to user's folder
                foreach (ProductFileViewModel file in files)
                {
                    Utility.MoveFile("~/Temp/ProductFiles/" + username + "/" + file.SavedName, "~/Images/DownloadableFiles/" + username + "/", file.SavedName);
                }
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Files have been saved." };
            }
        }

        /// <summary>
        /// Save product shipping countries
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static ActionOutput SaveShippingCountries(List<ShipingCountry> list)
        {
            using (var context = new CentroEntities())
            {
                foreach (var item in list)
                {
                    context.ShipingCountries.AddObject(item);
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Images have been saved." };
            }
        }

        public static ActionOutput<ProductShippingCountries> ShippingCountryByProductId(int product_id)
        {
            using (var context = new CentroEntities())
            {
                //var q = Context.TableAs.Select(a => new { a.Foo, a.TableB.Bar });
                List<ProductShippingCountries> list = context.ShipingCountries
                                                             .Where(s => s.ProductId == product_id)
                                                             .Select(s => new ProductShippingCountries
                                                                        {
                                                                            CountryID = s.Country1.CountryID == null ? 0 : s.Country1.CountryID,
                                                                            CountryName = s.Country1.CountryName == null ? "Anywhere" : s.Country1.CountryName,
                                                                            ShippingCost = s.Cost,
                                                                            ShippingCostAfterFirst = s.CostAfterFirstProduct
                                                                        })
                                                             .Distinct()
                                                             .OrderByDescending(m => m.CountryID)
                                                             .ToList();
                return new ActionOutput<ProductShippingCountries> { List = list, Status = ActionStatus.Successfull };
            }
        }

        /// <summary>
        /// Change section of selected products
        /// </summary>
        /// <param name="shop_section_id"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public static ActionOutput ChangeSection(int shop_section_id, string[] productIds)
        {
            using (var context = new CentroEntities())
            {
                foreach (string pid in productIds)
                {
                    int id = Convert.ToInt32(pid);
                    var product = context.Products.Where(p => p.ProductID == id).FirstOrDefault();
                    product.ShopSectionId = shop_section_id;
                }
                context.SaveChanges();
                return new ActionOutput { Message = "listings have been moved to selected section", Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput SetLastViewedDate(int product_id)
        {
            using (var context = new CentroEntities())
            {
                var p = context.Products.Where(m => m.ProductID == product_id).FirstOrDefault();
                p.LastViewedOn = DateTime.Now;
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull };
            }
        }

        /// <summary>
        /// Return all categories
        /// </summary>
        public static ActionOutput<Category> GetCategory()
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Category> { Status = ActionStatus.Successfull, List = context.Categories.Where(m => !m.IsDeleted && m.Published).OrderBy(m => m.Name).ToList() };
            }
        }

        /// <summary>
        /// Returne a paged result for products
        /// </summary>
        public static PagingResult<ProductsListing_Result> ProductsPaging(int page_no, int per_page_result, string sortColumn, string sortOrder, string search, string category, string subcat, string type)
        {
            using (var context = new CentroEntities())
            {
                ObjectParameter output = new ObjectParameter("TotalCount", typeof(int));
                var list = context.ProductsListing(page_no, per_page_result, sortColumn, sortOrder, search, category, subcat, type, output).ToList();
                PagingResult<ProductsListing_Result> pagingResult = new PagingResult<ProductsListing_Result>();
                pagingResult.List = list;
                pagingResult.TotalCount = Convert.ToInt32(output.Value);
                pagingResult.Status = ActionStatus.Successfull;
                return pagingResult;
            }
        }

        public static PagingResult<ProductsListing_Result> ExclusiveDeals(int page_no, int per_page_result, string sortColumn, string sortOrder, int dealID, int? dealItemID)
        {
            using (var context = new CentroEntities())
            {
                ObjectParameter output = new ObjectParameter("TotalCount", typeof(int));
                var list = context.ExclusiveDeals(page_no, per_page_result, sortColumn, sortOrder, dealID, dealItemID, output).ToList();
                PagingResult<ProductsListing_Result> pagingResult = new PagingResult<ProductsListing_Result>();
                pagingResult.List = list;
                pagingResult.TotalCount = Convert.ToInt32(output.Value);
                pagingResult.Status = ActionStatus.Successfull;
                return pagingResult;
            }
        }

        /// <summary>
        /// Returne a paged result for products
        /// </summary>
        public static PagingResult<ProductsListingAdmin_Result> ProductsPagingAdmin(int page_no, int per_page_result, string sortColumn, string sortOrder, string search, string category)
        {
            using (var context = new CentroEntities())
            {
                ObjectParameter output = new ObjectParameter("TotalCount", typeof(int));
                var list = context.ProductsListingAdmin(page_no, per_page_result, sortColumn, sortOrder, search, category, output).ToList();
                PagingResult<ProductsListingAdmin_Result> pagingResult = new PagingResult<ProductsListingAdmin_Result>();
                pagingResult.List = list;
                pagingResult.TotalCount = Convert.ToInt32(output.Value);
                pagingResult.Status = ActionStatus.Successfull;
                return pagingResult;
            }
        }

        /// <summary>
        /// Returne a paged result for featured products
        /// </summary>
        public static PagingResult<ProductsListing_Result> FeaturedProductsPaging(int page_no, int per_page_result, string sortColumn, string sortOrder, string search)
        {
            using (var context = new CentroEntities())
            {
                ObjectParameter output = new ObjectParameter("TotalCount", typeof(int));
                var list = context.FeaturedProductsListing(page_no, per_page_result, sortColumn, sortOrder, search, output).ToList();
                PagingResult<ProductsListing_Result> pagingResult = new PagingResult<ProductsListing_Result>();
                pagingResult.List = list;
                pagingResult.TotalCount = Convert.ToInt32(output.Value);
                pagingResult.Status = ActionStatus.Successfull;
                return pagingResult;
            }
        }

        public static ActionOutput SetAsFeatured(List<int> AllProducts, List<int> SelectedProducts)
        {
            using (var context = new CentroEntities())
            {
                var products = context.Products.Where(m => AllProducts.Contains(m.ProductID)).ToList();
                foreach (var item in products)
                {
                    if (SelectedProducts != null && SelectedProducts.Contains(item.ProductID))
                        item.IsFeatured = true;
                    else
                        item.IsFeatured = false;
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Selected products have been set as featured." };
            }
        }

        /// <summary>
        /// Returne a paged result for products by shop id and section id
        /// </summary>
        public static PagingResult<ProductsListing_Result> ProductsByShopPaging(int shop_id, int? shop_section_id, int page_no, int per_page_result, string sortColumn, string sortOrder, int? deleted, int owner_shop_id)
        {
            using (var context = new CentroEntities())
            {
                ObjectParameter output = new ObjectParameter("TotalCount", typeof(int));
                var list = context.GetProductsByShopListing(shop_id, page_no, per_page_result, sortColumn, sortOrder, shop_section_id, deleted, output).ToList();
                PagingResult<ProductsListing_Result> pagingResult = new PagingResult<ProductsListing_Result>();
                pagingResult.List = shop_id == owner_shop_id ? list : list.Where(m => m.Quantity > 0).ToList();
                pagingResult.TotalCount = Convert.ToInt32(output.Value);
                pagingResult.Status = ActionStatus.Successfull;
                return pagingResult;
            }
        }

        public static ActionOutput UpdateProducts(ProductAction obj, Int32 user_id)
        {
            using (var context = new CentroEntities())
            {
                if (obj.ActionID == ActionOnProduct.Activate)
                {
                    foreach (var id in obj.ProductID)
                    {
                        var product = context.Products.Where(p => p.ProductID.Equals(id)).FirstOrDefault();
                        product.IsDeleted = false;
                        product.DeletedBy = null;
                        product.DeletedOn = null;
                        product.UpdatedOn = DateTime.Now;
                        product.UpdatedBy = user_id;

                        context.SaveChanges();
                    }
                    return new ActionOutput { Status = ActionStatus.Successfull, Message = "Products Activated Successfully." };
                }
                else if (obj.ActionID == ActionOnProduct.DeActivate)
                {
                    foreach (var id in obj.ProductID)
                    {
                        var product = context.Products.Where(p => p.ProductID.Equals(id)).FirstOrDefault();
                        product.IsDeleted = true;
                        product.DeletedBy = user_id;
                        product.DeletedOn = DateTime.Now;
                        product.UpdatedOn = DateTime.Now;
                        product.UpdatedBy = user_id;

                        context.SaveChanges();
                    }
                    return new ActionOutput { Status = ActionStatus.Successfull, Message = "Products Deactivated Successfully." };
                }
                else if (obj.ActionID == ActionOnProduct.Delete)
                {
                    foreach (var id in obj.ProductID)
                    {
                        var orderedProduct = context.OrderItems.Where(p => p.ProductID == id).ToList();
                        foreach (var orderItem in orderedProduct)
                        {
                            orderItem.ProductID = null;
                        }

                        var product = context.Products.Where(p => p.ProductID.Equals(id)).FirstOrDefault();
                        context.DeleteObject(product);

                    }
                    context.SaveChanges();
                    return new ActionOutput { Status = ActionStatus.Successfull, Message = "Products Deleted Successfully." };
                }
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Records Updated Successfully." };

            }
        }

        /// <summary>
        /// get product shipping countries
        /// </summary>
        /// <param name="product_id"></param>
        /// <returns></returns>
        public static ActionOutput<ShipingCountry> GetShippingCountriesForProduct(int product_id)
        {
            using (var context = new CentroEntities())
            {

                return new ActionOutput<ShipingCountry> { Status = ActionStatus.Successfull, List = context.ShipingCountries.Where(p => p.ProductId.Equals(product_id)).ToList() };
            }
        }

        /// <summary>
        /// get product images
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static ActionOutput<Picture> GetProductPictures(int product_id)
        {
            using (var context = new CentroEntities())
            {
                var list = context.ProductPictures.Where(p => p.ProductId.Equals(product_id)).Select(p => p.PictureId).ToList();
                List<Picture> picture_list = new List<Picture>();
                foreach (var item in list)
                {
                    Picture pic = context.Pictures.Where(p => p.PictureID.Equals(item)).FirstOrDefault();
                    picture_list.Add(pic);

                }
                return new ActionOutput<Picture> { Status = ActionStatus.Successfull, List = picture_list };
            }
        }


        /// <summary>
        /// update Product
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ActionOutput UpdateProduct(Product obj, int user_id)
        {
            using (var context = new CentroEntities())
            {
                var product = context.Products.Where(p => p.ProductID == obj.ProductID).FirstOrDefault();

                var shop = context.Shops.Where(m => m.UserId == user_id).FirstOrDefault();
                int count = context.Products.Where(m => m.CreatedOn.Value.Month == product.CreatedOn.Value.Month &&
                                                        m.CreatedOn.Value.Year == product.CreatedOn.Value.Year &&
                                                        m.CreatedOn.Value < product.CreatedOn).Count();

                product.UpdatedBy = user_id;
                product.UpdatedOn = DateTime.Now;
                product.Description = obj.Description;
                product.CategoryId = obj.CategoryId;
                product.Condition = obj.Condition;
                product.Materials = obj.Materials;
                product.Tags = obj.Tags;
                product.PrimaryPicture = obj.PrimaryPicture;
                product.UnitPrice = obj.UnitPrice;
                product.IsDownloadable = obj.IsDownloadable;
                product.IsDownloadViaShip = obj.IsDownloadViaShip;
                product.SendDownloadVia = obj.SendDownloadVia;
                product.DownlodableShippingPolicy = obj.DownlodableShippingPolicy;
                // product.ProductFileID = obj.ProductFileID;
                product.ShipFromId = obj.ShipFromId;
                product.Quantity = obj.Quantity;
                product.Title = obj.Title;
                product.ManufacturerID = obj.ManufacturerID;
                product.Manufacturer = obj.Manufacturer;
                product.ProductNumberID = shop.ShopNumberID.Substring(0, 4) + count;
                product.OtherKeywords = obj.OtherKeywords;

                var existingProductTags = context.ProductTags.Where(m => m.ProductId == obj.ProductID).ToList();
                foreach (var existingTag in existingProductTags)
                {
                    context.ProductTags.DeleteObject(existingTag);
                }
                if (obj.IsDownloadable)
                {
                    var shipping_country_list = context.ShipingCountries.Where(s => s.ProductId == obj.ProductID).ToList();
                    foreach (var item in shipping_country_list)
                    {
                        context.ShipingCountries.DeleteObject(item);
                    }
                }
                int status = context.SaveChanges();
                if (status > 0)
                {
                    // Saving tags & materials
                    var masterTagsAll = context.Tags.ToList();
                    var masterProductTags = masterTagsAll.Where(m => m.TagType == (int)TagType.ProductTag).ToList();
                    var masterProductMaterials = masterTagsAll.Where(m => m.TagType == (int)TagType.ProductMaterial).ToList();
                    existingProductTags = context.ProductTags.Where(m => m.ProductId == obj.ProductID).ToList();
                    List<int> productTags = new List<int>();

                    if (obj.Tags != null && obj.Tags.Length > 0)
                    {
                        string[] tags = obj.Tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        // Save product tags 
                        foreach (var tag in tags)
                        {
                            var existingTag = masterProductTags.Where(t => t.TagText.Equals(tag, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                            if (existingTag != null && !existingProductTags.Where(t => t.TagId == existingTag.TagID).Any())
                            {
                                productTags.Add(existingTag.TagID);
                            }
                            else if (!existingProductTags.Where(t => t.TagId == existingTag.TagID).Any())
                            {
                                // Make entry in master table
                                Tag t = new Tag();
                                t.TagText = tag;
                                t.TagType = (int)TagType.ProductTag;
                                context.Tags.AddObject(t);
                                context.SaveChanges();

                                productTags.Add(t.TagID);
                            }
                        }
                    }
                    if (obj.Materials != null && obj.Materials.Length > 0)
                    {
                        // Save product materials 
                        string[] materials = obj.Materials.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var material in materials)
                        {
                            var existingMaterial = masterProductMaterials.Where(t => t.TagText.Equals(material, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                            if (existingMaterial != null && !existingProductTags.Where(t => t.TagId == existingMaterial.TagID).Any())
                            {
                                productTags.Add(existingMaterial.TagID);
                            }
                            else if (!existingProductTags.Where(t => t.TagId == existingMaterial.TagID).Any())
                            {
                                // Make entry in master table
                                Tag t = new Tag();
                                t.TagText = material;
                                t.TagType = (int)TagType.ProductMaterial;
                                context.Tags.AddObject(t);
                                context.SaveChanges();

                                productTags.Add(t.TagID);
                            }
                        }
                    }
                    if (productTags.Count > 0)
                    {
                        foreach (int id in productTags)
                        {
                            context.ProductTags.AddObject(new ProductTag { TagId = id, ProductId = obj.ProductID });
                        }
                        context.SaveChanges();
                    }
                    return new ActionOutput { Status = ActionStatus.Successfull, ID = obj.ProductID, Message = "Product has been Updated Successfully." };
                }
                return new ActionOutput { Status = ActionStatus.Error, ID = 0, Message = "Error Occured." };
            }
        }

        /// <summary>
        /// Update product images
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static ActionOutput UpdateProductPictures(int user_id, int product_id, UserProductTempPictures list)
        {
            using (var context = new CentroEntities())
            {
                var pic_ids = context.ProductPictures.Where(p => p.ProductId == product_id).Select(p => p.PictureId).ToList();
                var current_pic_list = list.UserTempPictures.Where(p => p.PictureId != 0).Select(p => p.PictureId).ToList();
                var pics_to_be_removed = pic_ids.Where(o => !current_pic_list.Contains(o)).ToList();
                foreach (var item in pics_to_be_removed)
                {
                    var picture = context.Pictures.Where(p => p.PictureID == item).FirstOrDefault();
                    context.Pictures.DeleteObject(picture);

                }
                context.SaveChanges();
                foreach (var item in list.UserTempPictures.Where(o => o.PictureId == 0).ToList())
                {
                    Picture pic = new Picture();
                    pic.CreatedBy = user_id;
                    pic.CreatedOn = DateTime.Now;
                    pic.DeletedBy = null;
                    pic.DeletedOn = null;
                    pic.DisplayName = item.DisplayName;
                    pic.SavedName = item.SavedName;
                    pic.Thumbnail = item.Thumbnail;
                    pic.IsDeleted = false;
                    pic.MimeType = item.MimeType;
                    pic.SizeInBytes = item.SizeInBytes;
                    pic.SizeInKB = item.SizeInKB;
                    pic.SizeInMB = item.SizeInMB;
                    pic.UpdatedBy = null;
                    pic.UpdatedOn = null;
                    context.Pictures.AddObject(pic);
                    context.SaveChanges();

                    ProductPicture prod_pic = new ProductPicture();
                    prod_pic.DisplayOrder = null;
                    prod_pic.PictureId = pic.PictureID;
                    prod_pic.ProductId = product_id;
                    context.ProductPictures.AddObject(prod_pic);
                    context.SaveChanges();
                    try
                    {
                        // Move temp files to user's folder
                        Utility.MoveFile("~/Temp/" + item.Username + "/" + item.SavedName, "~/Images/ProductImages/" + item.Username + "/", item.SavedName);
                        Utility.MoveFile("~/Temp/" + item.Username + "/" + item.Thumbnail, "~/Images/ProductImages/" + item.Username + "/", item.Thumbnail);
                    }
                    catch (Exception exc)
                    { }
                }
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Images have been saved." };
            }
        }

        /// <summary>
        /// update product shipping countries
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static ActionOutput UpdateShippingCountries(List<ShipingCountry> list, int product_id)
        {
            using (var context = new CentroEntities())
            {
                var user_shipping_list = context.ShipingCountries.Where(o => o.ProductId == product_id).ToList();
                var shipping_to_be_removed = user_shipping_list.Where(o => !list.Contains(o)).ToList();
                foreach (var item in shipping_to_be_removed)
                {
                    context.ShipingCountries.DeleteObject(item);
                    context.SaveChanges();
                }
                foreach (var item in list)
                {
                    context.ShipingCountries.AddObject(item);
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Images have been saved." };
            }
        }

        /// <summary>
        /// get product files
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static ActionOutput<ProductFile> GetProductFiles(int product_id)
        {
            using (var context = new CentroEntities())
            {
                var list = context.ProductFiles.Where(p => p.ProductID == product_id).ToList();

                return new ActionOutput<ProductFile> { Status = ActionStatus.Successfull, List = list };
            }
        }

        public static ActionOutput UpdateProductFile(string username, List<ProductFileViewModel> files, int product_id)
        {
            using (var context = new CentroEntities())
            {
                var file_ids = context.ProductFiles.Where(p => p.ProductID == product_id).Select(p => p.ProductFileID).ToList();
                var current_file_list = files.Where(p => p.ProductFileId != 0).Select(p => p.ProductFileId).ToList();
                var files_to_be_removed = file_ids.Where(o => !current_file_list.Contains(o)).ToList();
                foreach (var item in files_to_be_removed)
                {
                    var file = context.ProductFiles.Where(p => p.ProductFileID == item).FirstOrDefault();
                    context.ProductFiles.DeleteObject(file);

                }
                foreach (ProductFileViewModel file in files.Where(p => p.ProductFileId == 0).ToList())
                {
                    ProductFile p = new ProductFile();
                    p.DisplayName = file.DisplayName;
                    p.MimeType = file.MimeType;
                    p.SavedName = file.SavedName;
                    p.SizeInBytes = file.SizeInBytes;
                    p.SizeInKB = file.SizeInKB;
                    p.SizeInMB = file.SizeInMB;
                    p.ProductID = product_id;
                    context.ProductFiles.AddObject(p);
                }
                context.SaveChanges();


                // Move temp files to user's folder
                foreach (ProductFileViewModel file in files)
                {
                    try
                    {
                        Utility.MoveFile("~/Temp/ProductFiles/" + username + "/" + file.SavedName, "~/Images/DownloadableFiles/" + username + "/", file.SavedName);
                    }
                    catch (Exception exc)
                    { }
                }
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Files have been saved." };
            }
        }

        public static List<ShopServices> GetServices()
        {
            List<ShopServices> service_list = Enum.GetValues(typeof(Services)).Cast<Services>().Select(v => new ShopServices
            {
                ServiceName = v.ToEnumDescription(),
                ServiceId = v
            }).ToList();
            return service_list;
        }

        public static PagingResult<ProductsListing_Result> ProductsSearch(int page_no, int per_page_result, string sortColumn, string sortOrder, string search, string type)
        {
            using (var context = new CentroEntities())
            {
                ObjectParameter output = new ObjectParameter("TotalCount", typeof(int));
                var list = context.ProductSearch(page_no, per_page_result, sortColumn, sortOrder, search, type, output).ToList();
                PagingResult<ProductsListing_Result> pagingResult = new PagingResult<ProductsListing_Result>();
                pagingResult.List = list;
                pagingResult.TotalCount = Convert.ToInt32(output.Value);
                pagingResult.Status = ActionStatus.Successfull;
                return pagingResult;
            }
        }

        public static ActionOutput<ProductViewModel> SimilarProducts(int category_id)
        {
            using (var context = new CentroEntities())
            {
                var list = context.Products
                                  .Join(context.Shops,
                                        p => p.ShopId,
                                        s => s.ShopID,
                                        (p, s) => new { p, s })
                                  .Join(context.Users,
                                        m => m.s.UserId,
                                        u => u.UserID,
                                        (m, u) => new { m.p, u, m.s })
                                  .Where(m => m.p.CategoryId == category_id && !m.p.IsDeleted && m.p.Quantity > 0)
                                  .Select(m => new ProductViewModel
                                  {
                                      ProductID = m.p.ProductID,
                                      ShopId = m.p.ShopId,
                                      Manufacturer = m.p.Manufacturer,
                                      CategoryId = m.p.CategoryId,
                                      Condition = m.p.Condition,
                                      Quantity = m.p.Quantity,
                                      ShipFromId = m.p.ShipFromId,
                                      Title = m.p.Title,
                                      ShopSectionId = m.p.ShopSectionId,
                                      IsDeleted = m.p.IsDeleted,
                                      Tags = m.p.Tags,
                                      Materials = m.p.Materials,
                                      PrimaryPicture = m.p.PrimaryPicture,
                                      Description = m.p.Description,
                                      UnitPrice = m.p.UnitPrice,
                                      LastViewedOn = m.p.LastViewedOn,
                                      CreatedOn = m.p.CreatedOn,
                                      CreatedBy = m.p.CreatedBy,
                                      UpdatedOn = m.p.UpdatedOn,
                                      UpdatedBy = m.p.UpdatedBy,
                                      DeletedOn = m.p.DeletedOn,
                                      DeletedBy = m.p.DeletedBy,
                                      IsFeatured = m.p.IsFeatured,
                                      IsDownloadable = m.p.IsDownloadable,
                                      ShopName = m.s.ShopName,
                                      ShopOwnerName = m.u.UserName
                                  })
                                  .ToList();

                return new ActionOutput<ProductViewModel>
                {
                    Status = ActionStatus.Successfull,
                    List = list
                };
            }
        }


        public static ActionOutput<ProductViewModel> ProductsToProductsViewModel(List<Product> products)
        {
            return new ActionOutput<ProductViewModel>
            {
                List = products.Select(m => new ProductViewModel
                {
                    ProductID = m.ProductID,
                    ShopId = m.ShopId,
                    Manufacturer = m.Manufacturer,
                    CategoryId = m.CategoryId,
                    Condition = m.Condition,
                    Quantity = m.Quantity,
                    ShipFromId = m.ShipFromId,
                    Title = m.Title,
                    ShopSectionId = m.ShopSectionId,
                    IsDeleted = m.IsDeleted,
                    Tags = m.Tags,
                    Materials = m.Materials,
                    PrimaryPicture = m.PrimaryPicture,
                    Description = m.Description,
                    UnitPrice = m.UnitPrice,
                    LastViewedOn = m.LastViewedOn,
                    CreatedOn = m.CreatedOn,
                    CreatedBy = m.CreatedBy,
                    UpdatedOn = m.UpdatedOn,
                    UpdatedBy = m.UpdatedBy,
                    DeletedOn = m.DeletedOn,
                    DeletedBy = m.DeletedBy,
                    IsFeatured = m.IsFeatured,
                    IsDownloadable = m.IsDownloadable
                }).ToList(),
                Status = ActionStatus.Successfull
            };
        }

        public static ActionOutput<Product> GetProductByProductID(int productID)
        {
            using (var context = new CentroEntities())
            {
                var product = context.Products.Where(m => m.ProductID == productID && m.IsDeleted == false).FirstOrDefault();
                return new ActionOutput<Product>
                {
                    Object = product,
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput MoveToCategory(int CategoryID, List<int> SelectedProducts)
        {
            using (var context = new CentroEntities())
            {
                var products = context.Products.Where(m => SelectedProducts.Contains(m.ProductID)).ToList();
                foreach (var item in products)
                {
                    item.CategoryId = CategoryID;
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Selected products have been moved to selected category." };
            }
        }

        /// <summary>
        /// Get all deals
        /// </summary>
        /// <returns></returns>
        public static List<Deal> GetDeals()
        {
            using (var context = new CentroEntities())
            {
                return context.Deals.ToList();
            }
        }

        /// <summary>
        /// Get all active deals
        /// </summary>
        /// <returns></returns>
        public static List<DealViewModel> GetActiveDeals()
        {
            using (var context = new CentroEntities())
            {

                return context.Deals.Select(m => new DealViewModel
                {
                    DealID = m.DealID,
                    IsActive = m.IsActive,
                    SubTitle = m.SubTitle,
                    Title = m.Title,
                    DealItems = m.DealItems.Select(di => new DealItemViewModel
                    {
                        CategoryID = di.CategoryID,
                        CategoryName = di.Category.Name,
                        DealID = di.DealID,
                        DealItemID = di.DealItemID,
                        IsActive = di.IsActive,
                        PictureName = di.Picture,
                        Title = di.Title
                    }).AsEnumerable()
                })
                .Where(m => m.IsActive)
                .ToList();
            }
        }

        /// <summary>
        /// to get a deal by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static DealViewModel GetDeal(int id)
        {
            using (var context = new CentroEntities())
            {
                return context.Deals.Select(m => new DealViewModel
                {
                    DealID = m.DealID,
                    IsActive = m.IsActive,
                    SubTitle = m.SubTitle,
                    Title = m.Title,
                    DealItems = m.DealItems.Select(di => new DealItemViewModel
                    {
                        CategoryID = di.CategoryID,
                        CategoryName = di.Category.Name,
                        DealID = di.DealID,
                        DealItemID = di.DealItemID,
                        IsActive = di.IsActive,
                        PictureName = di.Picture,
                        Title = di.Title
                    }).AsEnumerable()
                })
                .Where(m => m.DealID == id)
                .FirstOrDefault();
            }
        }

        public static void SaveDeal(DealViewModel model)
        {
            using (var context = new CentroEntities())
            {
                if (model.DealID > 0)
                {
                    var obj = context.Deals.Where(m => m.DealID == model.DealID).FirstOrDefault();
                    context.Deals.DeleteObject(obj);
                }

                Deal deal = new Deal
                {
                    IsActive = true,
                    SubTitle = model.SubTitle,
                    Title = model.Title
                };

                model.DealItemsList.ForEach(m =>
                {
                    var dealItem = new DealItem
                    {
                        CategoryID = m.CategoryID,
                        IsActive = true,
                        Picture = m.PictureName,
                        Title = m.Title
                    };
                    deal.DealItems.Add(dealItem);
                });
                context.Deals.AddObject(deal);
                context.SaveChanges();
            }
        }

        public static void DeleteDeal(int id)
        {
            using (var context = new CentroEntities())
            {
                var obj = context.Deals.Where(m => m.DealID == id).FirstOrDefault();
                context.Deals.DeleteObject(obj);
                context.SaveChanges();
            }
        }

        public static void ActivateDeactivateDeal(int id)
        {
            using (var context = new CentroEntities())
            {
                var obj = context.Deals.Where(m => m.DealID == id).FirstOrDefault();
                obj.IsActive = obj.IsActive ? false : true;
                context.SaveChanges();
            }
        }

        public static List<DealItemProduct> MergeDealProducts(int dealid)
        {
            using (var context = new CentroEntities())
            {
                return context.DealItemProducts.Where(m => m.DealID == dealid).ToList();
            }
        }

        public static List<Product> AllProducts()
        {
            using (var context = new CentroEntities())
            {
                return context.Products.Where(m => !m.IsDeleted && m.Quantity > 0).ToList();
            }
        }

        public static List<DealItem> GetDealItem(int dealid)
        {
            using (var context = new CentroEntities())
            {
                return context.DealItems.Where(m => m.DealID == dealid).ToList();
            }
        }

        public static DealItem GetDealItemByID(int itemid)
        {
            using (var context = new CentroEntities())
            {
                return context.DealItems.Where(m => m.DealItemID == itemid).FirstOrDefault();
            }
        }

        public static ActionOutput MapDealProducts(string data, int dealid)
        {
            using (var context = new CentroEntities())
            {
                List<int> States = new List<int>();
                string[] rows = data.Split('$');
                var old = context.DealItemProducts.Where(m => m.DealID == dealid).ToList();

                foreach (var item in old)
                {
                    context.DealItemProducts.DeleteObject(item);
                }
                foreach (string row in rows)
                {
                    string[] shippings = row.Split(':');
                    int DealItemID = Convert.ToInt32(shippings[0]);
                    //var sippingP = context.ShippingPriceNames.Where(m => m.ID == ShippingPriceNameID).FirstOrDefault();
                    foreach (int id in shippings[1].Split(',').Select(m => Convert.ToInt32(m)).ToList())
                    {
                        context.DealItemProducts.AddObject(new DealItemProduct
                        {
                            DealItemID = DealItemID,
                            DealID = dealid,
                            ProductID = id
                        });
                    }
                }
                context.SaveChanges();
            }
            return new ActionOutput { Status = ActionStatus.Successfull };
        }
    }
}