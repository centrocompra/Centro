using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Classes;
using BusinessLayer.Handler;
using System.Drawing;
using System.Web.Script.Serialization;
using System.Web.Security;
using Ionic.Zip;
using System.Xml;
using System.IO;

namespace Centro.Controllers
{
    [AuthenticateUser]
    [NoCache]
    public class UserController : FrontEndBaseController
    {
        /// <summary>
        /// To Render Buyer dashboard
        /// </summary>
        public ActionResult Index()
        {
            ViewBag.UserID = SiteUserDetails.LoggedInUser.Id;
            return View(AccountActivityHandler.ActivityFeeds(1, 12, "Desc", "CreatedOn", null, SiteUserDetails.LoggedInUser.Id));
        }

        /// <summary>
        /// To Render user activities
        /// </summary>
        public ActionResult Activities()
        {
            ViewBag.UserID = SiteUserDetails.LoggedInUser.Id;
            return View(AccountActivityHandler.ActivityFeeds(1, 12, "Desc", "CreatedOn", null, SiteUserDetails.LoggedInUser.Id));
        }

        /// <summary>
        /// To Render activities paging
        /// </summary>
        public JsonResult _Activities(int page_no, int per_page_result, string sortOrder, string sortColumn, string search, int UserID)
        {

            PagingResult<AccountActivityViewModel> model = AccountActivityHandler.ActivityFeeds(page_no, per_page_result, sortOrder, sortColumn, null, UserID);
            string view = RenderRazorViewToString("_Activities", model.List);
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string> { view, model.TotalCount.ToString(), model.Message }
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To Render Buyer homepage
        /// </summary>
        //public ActionResult Home()
        //{
        //    ViewBag.LastViewedProducts = ProductsHandler.LastViewedProducts(6).List;
        //    ViewBag.IsFeatured = true;
        //    ViewBag.HubList = HubHandler.GetRandomHubs(2).List;
        //    ViewBag.RandomContest = ContestHandler.GetRandomContest(2).List;
        //    ViewBag.RandomJobs = SellersHandler.GetRandomJobs(3).List;
        //    return View(ProductsHandler.FeaturedProductsPaging(1, 8, "ProductID", "DESC", ""));
        //}
        public ActionResult Home()
        {
            ViewBag.LastViewedProducts = ProductsHandler.LastViewedProducts(6).List;
            ViewBag.IsFeatured = true;
            ViewBag.HubList = HubHandler.GetRandomHubs(2).List;
            return View(ProductsHandler.FeaturedProductsPaging(1, 8, "ProductID", "DESC", ""));
        }

        /// <summary>
        /// To get category by id
        /// </summary>
        public ActionResult Category(string id)
        {
            id = Utility.HifenToSpace(id);
            ViewBag.Category = id;
            return View(ProductsHandler.ProductsPaging(1, 12, "ProductID", "DESC", "", id, "", ""));
        }

        /// <summary>
        /// To check comlpeted steps in shop registration
        /// </summary>
        public JsonResult IsCompleted(int shop_id)
        {
            SellersHandler.SetShopDetailsStepCompleted(shop_id);

            var isCompleted = SellersHandler.GetShopSignUpStepCompleted(shop_id).Object;
            return Json(new ActionOutput { ID = isCompleted != null ? isCompleted.StepCompleted : 0, Status = ActionStatus.Successfull }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To Render shop
        /// </summary>
        public ActionResult Shop()
        {
            #region Selected Tabs
            SelectedTabs.SellerMainTab = SelectedSellerTab.ShopDetails;
            ViewBag.SelectedTabs = SelectedTabs;
            #endregion
            ViewBag.Username = SiteUserDetails.LoggedInUser.Username;
            Shop model = SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object;
            model = model == null ? new Shop() : model;
            if (model == null || model.ShopID == 0)
            {
                ViewBag.ShopServices = SellersHandler.ShopServicesByShopId(null).List;
                ViewBag.Materials = new List<String>();
                ViewBag.ShopSpeciality = SellersHandler.ShopSpecialityByShopId(null).List;
            }
            else
            {
                ViewBag.ShopServices = SellersHandler.ShopServicesByShopId(model.ShopID).List;
                ViewBag.ShopSpeciality = SellersHandler.ShopSpecialityByShopId(model.ShopID, model.ShopSpecialties).List;
                if (!String.IsNullOrEmpty(model.Materials))
                {
                    ViewBag.Materials = model.Materials.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                else
                {
                    ViewBag.Materials = new List<String>();
                }

            }
            SellersHandler.SetShopDetailsStepCompleted(model.ShopID);
            ViewBag.StepCompleted = SellersHandler.GetShopSignUpStepCompleted(model.ShopID).Object;
            ViewBag.ShopId = model.ShopID;
            var user = UsersHandler.UserRegDetail(SiteUserDetails.LoggedInUser.Id).Results;
            ViewBag.UserRegDetail = "Your account was registered on " + user[1] + ".";
            return View(model);
        }

        /// <summary>
        /// To Render shop information partial view
        /// </summary>
        [AjaxOnly]
        public PartialViewResult _ShopInfo()
        {
            Shop model = SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object;
            ViewBag.Countries = SellersHandler.CountryGetAll().List;
            ViewBag.States = SellersHandler.GetStateByCountryId(model == null || model.ContactCountry == 0 ? 0 : model.ContactCountry).List;
            ViewBag.Cities = SellersHandler.GetCityByStateId(model == null || model.ContactState == 0 ? 0 : model.ContactState).List;
            if (model == null || model.ShopID == 0)
            {
                model = new BusinessLayer.Models.DataModel.Shop();
                model.UserId = SiteUserDetails.LoggedInUser.Id;
                ViewBag.ShopServices = SellersHandler.ShopServicesByShopId(null).List;
                ViewBag.Materials = new List<String>();
                ViewBag.ShopSpeciality = SellersHandler.ShopSpecialityByShopId(null).List;
            }
            else
            {
                ViewBag.ShopServices = SellersHandler.ShopServicesByShopId(model.ShopID).List;
                ViewBag.ShopSpeciality = SellersHandler.ShopSpecialityByShopId(model.ShopID, model.ShopSpecialties).List;
                if (!String.IsNullOrEmpty(model.Materials))
                {
                    ViewBag.Materials = model.Materials.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                else
                {
                    ViewBag.Materials = new List<String>();
                }

            }
            ViewBag.User = UsersHandler.GetUserByID(SiteUserDetails.LoggedInUser.Id).Object;
            ViewBag.Username = SiteUserDetails.LoggedInUser.Username;
            return PartialView(model);
        }

        /// <summary>
        /// To Render policies 
        /// </summary>
        [AjaxOnly]
        public PartialViewResult _Policies()
        {
            return PartialView(SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object);
        }

        /// <summary>
        /// To Render shop sections
        /// </summary>
        [AjaxOnly]
        public PartialViewResult _Sections(int shop_id)
        {
            // Getting products for this shop
            ViewBag.Products = ProductsHandler.ProductsByShopId(shop_id).List;
            ViewBag.Username = SiteUserDetails.LoggedInUser.Username;
            Shop shop = SellersHandler.ShopByShopId(shop_id).Object;
            ViewBag.ShopID = shop.ShopID;
            if (string.IsNullOrEmpty(shop.PaymentPolicy))
                return PartialView("_Policies", SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object);
            return PartialView(SellersHandler.ShopSectionsByShopId(shop_id).List);
        }

        /// <summary>
        /// To Render shop availability chart
        /// </summary>
        public PartialViewResult _ShopName()
        {
            Shop shop = SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object;
            ViewBag.ShopAvailablity = SellersHandler.GetShopAvailablity(shop.ShopID).List;
            ViewBag.ShopID = shop.ShopID;
            var section = SellersHandler.ShopSectionsByShopId(shop.ShopID).List;
            if (string.IsNullOrEmpty(shop.PaymentPolicy))
                return PartialView("_Policies", SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object);
            if (section != null && section.Count() == 0)
            {
                ViewBag.Products = ProductsHandler.ProductsByShopId(shop.ShopID).List;
                ViewBag.Username = SiteUserDetails.LoggedInUser.Username;
                return PartialView("_Sections", SellersHandler.ShopSectionsByShopId(shop.ShopID).List);
            }
            return PartialView(shop);
        }

        /// <summary>
        /// To save shop info
        /// </summary>
        [HttpPost]
        public JsonResult ShopInfo(HttpPostedFileBase shopBanner, Shop obj)
        {
            if (shopBanner != null)
            {
                /* Saving banner on the disk */
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(shopBanner.InputStream);
                if (bitmap.Width > 760 || bitmap.Height > 100 || bitmap.Height < 50 || bitmap.Width < 300)
                    return Json(new ActionOutput { Status = ActionStatus.Error, ID = 0, Message = "banner size must be between 300x50 and 760x100" }, JsonRequestBehavior.AllowGet);

                string fileExt = System.IO.Path.GetExtension(shopBanner.FileName).ToLower();
                if (fileExt.ToLower() == ".jpeg" || fileExt.ToLower() == ".jpg" || fileExt.ToLower() == ".gif" || fileExt.ToLower() == ".png")
                {
                    string path = Server.MapPath("~/Images/ShopImages/" + SiteUserDetails.LoggedInUser.Username + "/Banner/");
                    if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
                    obj.ShopBanner = Guid.NewGuid().ToString() + shopBanner.FileName.Substring(shopBanner.FileName.LastIndexOf('.'));
                    shopBanner.SaveAs(path + obj.ShopBanner);
                }
                else
                {
                    return Json(new ActionOutput { Status = ActionStatus.Error, ID = 0, Message = "Invalid file, valid file are *.jpg, *.jpeg, *.gif, *.bmp, *.png" }, JsonRequestBehavior.AllowGet);
                }
            }
            // make entry in database
            obj.UserId = SiteUserDetails.LoggedInUser.Id;
            ActionOutput output = SellersHandler.CreateOrUpdateShop(obj, SiteUserDetails.LoggedInUser.Id);
            //SellersHandler.SetShopDetailsStepCompleted(output.ID);
            if ((Request.Browser).Browser == "IE")
                return Json(output, "text/plain", JsonRequestBehavior.AllowGet);
            else
                return Json(output, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To save shop policies
        /// </summary>
        [HttpPost]
        public JsonResult ShopPolicy(Shop shop)
        {
            var output = SellersHandler.CreateOrUpdateShop(shop, SiteUserDetails.LoggedInUser.Id);
            //SellersHandler.SetShopDetailsStepCompleted(output.ID);
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To save shop sections
        /// </summary>
        [HttpPost]
        public JsonResult ShopSectoion(ShopSection obj)
        {
            var output = SellersHandler.CreateOrUpdateSection(obj);
            //SellersHandler.SetShopDetailsStepCompleted(output.ID);
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To change shop section order
        /// </summary>
        [HttpPost]
        public JsonResult ShopSectionDisplayOrder(int shop_id, string positions)
        {
            string[] items = positions.Split(new char[] { ',' });
            Dictionary<int, int> Positions = new Dictionary<int, int>();
            foreach (var item in items)
            {
                string[] arr = item.Split(new char[] { '=' });
                Positions.Add(Convert.ToInt32(arr[0]), Convert.ToInt32(arr[1]));
            }
            var output = SellersHandler.SetShopSectionDisplayOrder(shop_id, Positions);
            //SellersHandler.SetShopDetailsStepCompleted(shop_id);
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To delete shop section
        /// </summary>
        public JsonResult ShopSectionDelete(int section_id)
        {
            return Json(SellersHandler.DeleteShopSection(section_id), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To update shop section
        /// </summary>
        public JsonResult UpdateSection(int section_id, string section_name)
        {
            return Json(SellersHandler.UpdateSection(section_id, section_name), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To Render List items main page
        /// </summary>
        public ActionResult ListItems(int? product_id)
        {

            #region Selected Tabs
            SelectedTabs.SellerMainTab = SelectedSellerTab.ListItems;
            ViewBag.SelectedTabs = SelectedTabs;
            #endregion

            Shop model = SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object;
            if (model == null)
                return RedirectToAction("Shop", "User");
            model = model == null ? new Shop() : model;
            ViewBag.StepCompleted = SellersHandler.GetShopSignUpStepCompleted(model.ShopID).Object;
            ViewBag.ShopId = model.ShopID;
            ViewBag.IsShopOpen = model == null ? false : !model.IsClosed;
            ViewBag.Countries = SellersHandler.CountryGetAll().List;
            ViewBag.Categories = SellersHandler.CategoriesGetAll().List;
            ViewBag.ShopSections = SellersHandler.ShopSectionsByShopId(model.ShopID).List;
            ViewBag.Manufacturers = Enum.GetValues(typeof(Manufacturer)).Cast<Manufacturer>().Select(v => new SelectListItem
                                                                                                    {
                                                                                                        Text = v.ToEnumDescription(),
                                                                                                        Value = ((int)v).ToString()
                                                                                                    }).ToList();
            ViewBag.ProductCondition = Enum.GetValues(typeof(ProductCondition)).Cast<ProductCondition>().Select(v => new SelectListItem
                                                                                                    {
                                                                                                        Text = v.ToEnumDescription(),
                                                                                                        Value = ((int)v).ToString()
                                                                                                    }).ToList();

            var sendDownloadVia = Enum.GetValues(typeof(SendDownloadVia)).Cast<SendDownloadVia>().Select(v => new SelectListItem
            {
                Text = v.ToEnumWordify(),
                Value = ((int)v).ToString()
            }).ToList();
            sendDownloadVia.Insert(0, new SelectListItem { Text = "Transfer Type", Value = "" });
            ViewBag.SendDownloadVia = sendDownloadVia;

            var user = UsersHandler.UserRegDetail(SiteUserDetails.LoggedInUser.Id).Results;
            ViewBag.UserRegDetail = "Your account was registered on " + user[1] + ".";
            return View(new Product());

        }

        /// <summary>
        /// To Render product add page
        /// </summary>
        [AjaxOnly]
        public PartialViewResult _Item(int shop_id, int? product_id, bool isDeleted = false)
        {
            HttpCookie user_temp_cookie_old = Request.Cookies[Cookies.UserTempPictures];
            HttpCookie file_temp_cookie_old = Request.Cookies[Cookies.ProductTempFile];
            if (user_temp_cookie_old != null) { user_temp_cookie_old.Expires = DateTime.Now.AddDays(-1); Response.Cookies.Add(user_temp_cookie_old); }
            if (file_temp_cookie_old != null) { file_temp_cookie_old.Expires = DateTime.Now.AddDays(-1); Response.Cookies.Add(file_temp_cookie_old); }

            Product model = new Product();
            ViewBag.Countries = SellersHandler.CountryGetAll().List;
            ViewBag.Categories = SellersHandler.CategoriesGetAll().List;
            ViewBag.ShopSections = SellersHandler.ShopSectionsByShopId(shop_id).List;
            ViewBag.Manufacturers = Enum.GetValues(typeof(Manufacturer)).Cast<Manufacturer>().Select(v => new SelectListItem
            {
                Text = v.ToEnumDescription(),
                Value = ((int)v).ToString()
            }).ToList();
            ViewBag.ProductCondition = Enum.GetValues(typeof(ProductCondition)).Cast<ProductCondition>().Select(v => new SelectListItem
            {
                Text = v.ToEnumDescription(),
                Value = ((int)v).ToString()
            }).ToList();

            var sendDownloadVia = Enum.GetValues(typeof(SendDownloadVia)).Cast<SendDownloadVia>().Select(v => new SelectListItem
            {
                Text = v.ToEnumWordify(),
                Value = ((int)v).ToString()
            }).ToList();
            // sendDownloadVia.Insert(0, new SelectListItem { Text = "Transfer Type", Value = "" });
            ViewBag.SendDownloadVia = sendDownloadVia;

            ViewBag.ShopId = shop_id;
            if (product_id.HasValue)
            {
                var result = ProductsHandler.ProductById(product_id.Value, isDeleted);
                result.Object.SendDownloadViaProp = result.Object.SendDownloadVia.HasValue ? result.Object.SendDownloadVia.Value : 0;

                if (result.Object.ShopId != shop_id)
                    return PartialView("_Item", new Product());

                if (result.Object.Manufacturer == SiteUserDetails.LoggedInUser.Username)
                    result.Object.ManufacturerID = 1;
                else
                    result.Object.ManufacturerID = 2;

                String[] tags = result.Object.Tags != null ? result.Object.Tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray() : null;
                ViewBag.Tags = tags;
                String[] material = result.Object.Materials != null ? result.Object.Materials.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray() : null;
                ViewBag.Materials = material;
                var shippingCountry = ProductsHandler.GetShippingCountriesForProduct(product_id.Value);
                List<ShipingCountry> list = shippingCountry.List;
                ViewBag.ShippingTo = list;
                UserProductTempPictures temp_pics = new UserProductTempPictures();
                List<Picture> pic_list = ProductsHandler.GetProductPictures(product_id.Value).List;
                if (pic_list.Count > 0)
                {
                    HttpCookie user_temp_cookie = Request.Cookies[Cookies.UserTempPictures];
                    if (user_temp_cookie != null)
                    {
                        user_temp_cookie.Expires = DateTime.Now.AddDays(-30);
                        Response.Cookies.Add(user_temp_cookie);
                    }
                    user_temp_cookie = new HttpCookie(Cookies.UserTempPictures);

                    temp_pics.UserTempPictures = new List<UserProductPicture>();
                    foreach (var file in pic_list)
                    {
                        temp_pics.UserTempPictures.Add(new UserProductPicture
                        {
                            DisplayName = file.DisplayName,
                            MimeType = file.MimeType,
                            SavedName = file.SavedName,
                            Thumbnail = file.Thumbnail,
                            Username = SiteUserDetails.LoggedInUser.Username,
                            SizeInBytes = file.SizeInBytes,
                            SizeInKB = Convert.ToInt32(file.SizeInKB),
                            SizeInMB = Convert.ToInt32(file.SizeInMB),
                            PictureId = file.PictureID

                        });
                    }
                    // Creating Temp cookie for User product pics
                    CreateCustomCookie(Cookies.UserTempPictures, false, new JavaScriptSerializer().Serialize(temp_pics));
                }
                ViewBag.Pictures = temp_pics.UserTempPictures;
                List<ProductFile> file_list = ProductsHandler.GetProductFiles(product_id.Value).List;
                ProductFiles files = new ProductFiles();
                if (file_list.Count > 0)
                {
                    HttpCookie file_temp_cookie = Request.Cookies[Cookies.ProductTempFile];
                    if (file_temp_cookie != null)
                    {
                        file_temp_cookie.Expires = DateTime.Now.AddDays(-30);
                        Response.Cookies.Add(file_temp_cookie);
                    }
                    file_temp_cookie = new HttpCookie(Cookies.ProductTempFile);
                    files.ProductTempFiles = new List<ProductFileViewModel>();
                    foreach (var file in file_list)
                    {
                        files.ProductTempFiles.Add(new ProductFileViewModel
                        {
                            DisplayName = file.DisplayName,
                            MimeType = file.MimeType,
                            SavedName = file.SavedName,
                            SizeInBytes = file.SizeInBytes,
                            SizeInKB = Convert.ToInt32(file.SizeInKB),
                            SizeInMB = Convert.ToInt32(file.SizeInMB),
                            ProductFileId = file.ProductFileID

                        });
                    }
                    CreateCustomCookie(Cookies.ProductTempFile, false, new JavaScriptSerializer().Serialize(files));
                }
                ViewBag.ProductFiles = files.ProductTempFiles;




                return PartialView(result.Object);
            }
            return PartialView(model);
        }

        /// <summary>
        /// To save new product/item
        /// </summary>
        [HttpPost]
        public JsonResult CreateProduct(Product obj)
        {
            string SubCategory = Request["SubCategory"];
            string Types = Request["Type"];
            obj.OtherKeywords = Request["Keywords"] + "$$$" + SubCategory + ", " + Types;
            var shop = SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object;
            var SalesTax = SellersHandler.GetSalesTax(shop.ShopID).List;
            List<int> SalesTaxCountries = SalesTax.Select(m => m.ToCountryID).ToList();
            SalesTaxCountries.Add(1);// For USA which by default a ship to country as per client.

            // Check for shipping countries
            if (!obj.IsDownloadable)
            {
                // Saving Shipping Countries
                int Shipping_Count = Convert.ToInt32(Request["Shipping_Count"]);
                string[] skip = (Request["Skip_Shipping_Rownum"]).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                List<string> CountryWithoutSalesTax = new List<string>();
                for (int i = 1; i <= Shipping_Count; i++)
                {
                    if (!skip.Contains(i.ToString()))
                    {
                        int ship_to = Convert.ToInt32(Request["ShipTo_" + i]);

                        if (!SalesTaxCountries.Contains(ship_to))
                            CountryWithoutSalesTax.Add(ship_to.ToString());
                    }
                }
                if (CountryWithoutSalesTax.Count() > 0)
                    return Json(new ActionOutput { ID = 0, Status = ActionStatus.Error, Results = new List<string> { string.Join(",", CountryWithoutSalesTax) } }, JsonRequestBehavior.AllowGet);
            }



            if (obj.ProductID > 0)
            {
                obj.Manufacturer = obj.ManufacturerID == 2 ? obj.Manufacturer : SiteUserDetails.LoggedInUser.Username;
                HttpCookie user_temp_cookie = Request.Cookies[Cookies.UserTempPictures];
                HttpCookie file_temp_cookie = Request.Cookies[Cookies.ProductTempFile];
                obj.SendDownloadVia = obj.SendDownloadViaProp;
                if (obj.SendDownloadVia.HasValue && obj.SendDownloadVia.Value > 0)
                    obj.IsDownloadViaShip = true;
                else
                {
                    obj.IsDownloadViaShip = null;
                    obj.SendDownloadVia = null;
                }

                if (user_temp_cookie != null)
                {
                    var temp_pictures = new JavaScriptSerializer().Deserialize<UserProductTempPictures>(user_temp_cookie.Value);
                    obj.PrimaryPicture = temp_pictures.UserTempPictures.Count > 0 ? temp_pictures.UserTempPictures[0].SavedName : null;
                }
                ActionOutput output = ProductsHandler.UpdateProduct(obj, SiteUserDetails.LoggedInUser.Id);
                if (output.Status == ActionStatus.Successfull)
                {
                    AccountActivity objActivity = new AccountActivity();
                    objActivity.ActivityID = output.ID;
                    objActivity.ActivityType = (int)FollowType.Product;
                    objActivity.UserID = SiteUserDetails.LoggedInUser.Id;
                    objActivity.ActivityImage = "/Images/ProductImages/" + SiteUserDetails.LoggedInUser.Username + "/thumb_" + obj.PrimaryPicture;
                    objActivity.ActivityLink = "/Products/" + Utility.SpacesToHifen(shop.ShopName) + "/" + obj.ShopId + "/" + obj.CategoryId + "/" + obj.ProductID;
                    objActivity.ActivityText = ActivityText.ProductUpdate.Replace("{UserName}", SiteUserDetails.LoggedInUser.Username)
                                                           .Replace("{ProductTitle}", obj.Title)
                                                           .Replace("{Link}", objActivity.ActivityLink)
                                                           .Replace("{Date}", DateTime.Now.ToShortTimeString() + " " + DateTime.Now.ToLongDateString());
                    AccountActivityHandler.SaveActivity(objActivity);
                }
                if (file_temp_cookie != null)
                {
                    ProductFiles temp_file = new JavaScriptSerializer().Deserialize<ProductFiles>(file_temp_cookie.Value);
                    // Saving Product files into database
                    ProductsHandler.UpdateProductFile(SiteUserDetails.LoggedInUser.Username, temp_file.ProductTempFiles, output.ID);
                    // Remove temp cookie
                    file_temp_cookie.Expires = DateTime.Now.AddDays(-30);
                    Response.Cookies.Add(file_temp_cookie);
                    // Delete all files from temp folder
                    System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(Server.MapPath("~/Temp/ProductFiles/" + SiteUserDetails.LoggedInUser.Username + "/"));
                    folder.Empty();
                }
                if (user_temp_cookie != null)
                {

                    //FormsAuthenticationTicket pics_ticket = FormsAuthentication.Decrypt(user_temp_cookie.Value);
                    UserProductTempPictures temp_pics = new JavaScriptSerializer().Deserialize<UserProductTempPictures>(user_temp_cookie.Value);
                    // Saving Product Images into database
                    ProductsHandler.UpdateProductPictures(SiteUserDetails.LoggedInUser.Id, output.ID, temp_pics);
                    // Remove temp cookie
                    user_temp_cookie.Expires = DateTime.Now.AddDays(-30);
                    Response.Cookies.Add(user_temp_cookie);
                    // Delete all files from temp folder
                    System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(Server.MapPath("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/"));
                    folder.Empty();
                }
                if (!obj.IsDownloadable)
                {
                    // Saving Shipping Countries
                    
                    int Shipping_Count = Convert.ToInt32(Request["Shipping_Count"]);
                    string[] skip = (Request["Skip_Shipping_Rownum"]).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    List<ShipingCountry> shipping_countries = new List<ShipingCountry>();
                    for (int i = 1; i <= Shipping_Count; i++)
                    {
                        if (!skip.Contains(i.ToString()))
                        {
                            int ship_to = Convert.ToInt32(Request["ShipTo_" + i]);
                            decimal cost = Convert.ToDecimal(Request["Cost_" + i]);
                            decimal cost_with_other = Convert.ToDecimal(Request["CostWithOther_" + i]);
                            ShipingCountry ship = new ShipingCountry();
                            ship.Cost = cost;
                            ship.CostAfterFirstProduct = cost_with_other;
                            ship.ProductId = output.ID;
                            ship.ShipsFromId = obj.ShipFromId;
                            ship.ShipsTo = ship_to;
                            ship.ShopId = obj.ShopId;
                            ship.UserId = SiteUserDetails.LoggedInUser.Id;
                            shipping_countries.Add(ship);
                        }
                    }
                    ProductsHandler.UpdateShippingCountries(shipping_countries, obj.ProductID);
                    
                }
                if (output.Status == ActionStatus.Successfull)
                    output.Results = new List<string> { "Manage Listing" };

                return Json(output, JsonRequestBehavior.AllowGet);
            }
            else
            {
                obj.Manufacturer = obj.ManufacturerID == 2 ? obj.Manufacturer : SiteUserDetails.LoggedInUser.Username;
                HttpCookie user_temp_cookie = Request.Cookies[Cookies.UserTempPictures];
                HttpCookie file_temp_cookie = Request.Cookies[Cookies.ProductTempFile];
                obj.SendDownloadVia = obj.SendDownloadViaProp;
                if (obj.SendDownloadVia.HasValue && obj.SendDownloadVia.Value > 0)
                    obj.IsDownloadViaShip = true;
                else
                {
                    obj.IsDownloadViaShip = null;
                    obj.SendDownloadVia = null;
                }

                if (user_temp_cookie != null)
                {
                    var temp_pictures = new JavaScriptSerializer().Deserialize<UserProductTempPictures>(user_temp_cookie.Value);
                    obj.PrimaryPicture = temp_pictures.UserTempPictures.Count > 0 ? temp_pictures.UserTempPictures[0].SavedName : null;
                }
                ActionOutput output = ProductsHandler.CreateProduct(obj, SiteUserDetails.LoggedInUser.Id);
                if (output.Status == ActionStatus.Successfull)
                {
                    AccountActivity objActivity = new AccountActivity();
                    objActivity.ActivityID = output.ID;
                    objActivity.ActivityType = (int)FollowType.Product;
                    objActivity.UserID = SiteUserDetails.LoggedInUser.Id;
                    objActivity.ActivityImage = "/Images/ProductImages/" + SiteUserDetails.LoggedInUser.Username + "/thumb_" + obj.PrimaryPicture;
                    objActivity.ActivityLink = "/Products/" + Utility.SpacesToHifen(shop.ShopName) + "/" + obj.ShopId + "/" + obj.CategoryId + "/" + obj.ProductID;
                    objActivity.ActivityText = ActivityText.ProductCreate.Replace("{UserName}", SiteUserDetails.LoggedInUser.Username)
                                                                         .Replace("{ProductTitle}", output.Results[0])
                                                                         .Replace("{Link}", objActivity.ActivityLink)
                                                                         .Replace("{ItemCost}", Math.Round(obj.UnitPrice.Value, 2).ToString())
                                                                         .Replace("{Date}", DateTime.Now.ToShortTimeString() + " " + DateTime.Now.ToLongDateString());
                    AccountActivityHandler.SaveActivity(objActivity);
                }

                if (file_temp_cookie != null)
                {
                    ProductFiles temp_file = new JavaScriptSerializer().Deserialize<ProductFiles>(file_temp_cookie.Value);
                    // Saving Product files into database
                    ProductsHandler.SaveProductFile(SiteUserDetails.LoggedInUser.Username, temp_file.ProductTempFiles, output.ID);
                    // Remove temp cookie
                    file_temp_cookie.Expires = DateTime.Now.AddDays(-30);
                    Response.Cookies.Add(file_temp_cookie);
                    // Delete all files from temp folder
                    System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(Server.MapPath("~/Temp/ProductFiles/" + SiteUserDetails.LoggedInUser.Username + "/"));
                    folder.Empty();
                }

                if (user_temp_cookie != null)
                {
                    //FormsAuthenticationTicket pics_ticket = FormsAuthentication.Decrypt(user_temp_cookie.Value);
                    UserProductTempPictures temp_pics = new JavaScriptSerializer().Deserialize<UserProductTempPictures>(user_temp_cookie.Value);
                    // Saving Product Images into database
                    ProductsHandler.SaveProductPictures(SiteUserDetails.LoggedInUser.Id, output.ID, temp_pics);
                    // Remove temp cookie
                    user_temp_cookie.Expires = DateTime.Now.AddDays(-30);
                    Response.Cookies.Add(user_temp_cookie);
                    // Delete all files from temp folder
                    System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(Server.MapPath("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/"));
                    folder.Empty();
                }
                if (!obj.IsDownloadable)
                {
                    // Saving Shipping Countries
                    
                    int Shipping_Count = Convert.ToInt32(Request["Shipping_Count"]);
                    string[] skip = (Request["Skip_Shipping_Rownum"]).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    List<ShipingCountry> shipping_countries = new List<ShipingCountry>();
                    //shipping_countries.Add(new ShipingCountry
                    //{
                    //    Cost = Convert.ToDecimal(Request["EverywhereElse"]),
                    //    CostAfterFirstProduct = Convert.ToDecimal(Request["EverywhereElseWithOther"]),
                    //    ProductId = output.ID,
                    //    ShipsFromId = obj.ShipFromId,
                    //    ShipsTo = null,
                    //    ShopId = obj.ShopId,
                    //    UserId = CentroUsers.LoggedInUser.Id
                    //});
                    for (int i = 1; i <= Shipping_Count; i++)
                    {
                        if (!skip.Contains(i.ToString()))
                        {
                            int ship_to = Convert.ToInt32(Request["ShipTo_" + i]);
                            decimal cost = Convert.ToDecimal(Request["Cost_" + i]);
                            decimal cost_with_other = Convert.ToDecimal(Request["CostWithOther_" + i]);
                            ShipingCountry ship = new ShipingCountry();
                            ship.Cost = cost;
                            ship.CostAfterFirstProduct = cost_with_other;
                            ship.ProductId = output.ID;
                            ship.ShipsFromId = obj.ShipFromId;
                            ship.ShipsTo = ship_to;
                            ship.ShopId = obj.ShopId;
                            ship.UserId = SiteUserDetails.LoggedInUser.Id;
                            shipping_countries.Add(ship);
                        }
                    }
                    ProductsHandler.SaveShippingCountries(shipping_countries);
                    
                }
                if (output.Status == ActionStatus.Successfull)
                    output.Results = new List<string> { "" };
                return Json(output, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// To upload product pictures
        /// </summary>
        public JsonResult UploadProductPicture(HttpPostedFileBase file)
        {
            string newFilename = Guid.NewGuid().ToString() + "." + file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            string fileExt = System.IO.Path.GetExtension(file.FileName).ToLower();
            //if(photo.ContentLength>
            ActionOutput result;
            if (fileExt == ".jpeg" || fileExt == ".jpg" || fileExt == ".gif" || fileExt == ".bmp" || fileExt == ".png")
            {
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(file.InputStream);
                if (bitmap.Height > 1024 || bitmap.Height < 250)
                {
                    if ((Request.Browser).Browser == "IE")
                        return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Image must be between 250x250 to 1024x1024 pixel." }, "text/plain", JsonRequestBehavior.AllowGet);
                    else
                        return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Image must be between 250x250 to 1024x1024 pixel." }, JsonRequestBehavior.AllowGet);
                }
                // Saving Image to Temp Folder
                string temp_folder = Server.MapPath("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/");
                if (!System.IO.Directory.Exists(temp_folder))
                    System.IO.Directory.CreateDirectory(temp_folder);
                file.SaveAs(temp_folder + newFilename);

                // Generating Thumbnail and save it.
                Bitmap thumbnail = new Bitmap(ImageProcessor.ResizeImage(bitmap, new Size(238, 238), false));
                thumbnail.Save(Server.MapPath("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/thumb_") + newFilename);
                #region Temp Cookie
                // Check if cookie already exists
                UserProductTempPictures temp_pics;
                HttpCookie user_temp_cookie = Request.Cookies[Cookies.UserTempPictures];
                if (user_temp_cookie == null)
                {
                    temp_pics = new UserProductTempPictures();
                    temp_pics.UserTempPictures = new List<UserProductPicture>();
                    temp_pics.UserTempPictures.Add(new UserProductPicture
                    {
                        DisplayName = file.FileName,
                        MimeType = file.ContentType,
                        SavedName = newFilename,
                        Thumbnail = "thumb_" + newFilename,
                        Username = SiteUserDetails.LoggedInUser.Username,
                        SizeInBytes = file.ContentLength,
                        SizeInKB = file.ContentLength / 1024,
                        SizeInMB = file.ContentLength / (1024 * 1024)
                    });

                    // Creating Temp cookie for User product pics
                    CreateCustomCookie(Cookies.UserTempPictures, false, new JavaScriptSerializer().Serialize(temp_pics));
                }
                else
                {
                    // update existing temp cookie
                    temp_pics = new JavaScriptSerializer().Deserialize<UserProductTempPictures>(user_temp_cookie.Value);

                    temp_pics.UserTempPictures.Add(new UserProductPicture
                    {
                        DisplayName = file.FileName,
                        MimeType = file.ContentType,
                        SavedName = newFilename,
                        Thumbnail = "thumb_" + newFilename,
                        Username = SiteUserDetails.LoggedInUser.Username,
                        SizeInBytes = file.ContentLength,
                        SizeInKB = file.ContentLength / 1024,
                        SizeInMB = file.ContentLength / (1024 * 1024),
                    });
                    UpdateCustomCookie(Cookies.UserTempPictures, false, new JavaScriptSerializer().Serialize(temp_pics));
                }
                #endregion
                result = new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { SiteUserDetails.LoggedInUser.Username, newFilename, "thumb_" + newFilename } };
            }
            else
            {
                result = new ActionOutput { Status = ActionStatus.Error, Results = new List<string> { SiteUserDetails.LoggedInUser.Username }, Message = "Invalid file, valid file are *.jpg, *.jpeg, *.gif, *.bmp, *.png" };
                //return Json(new ActionOutput { Status = ActionStatus.Error, Message = "" }, JsonRequestBehavior.AllowGet);
            }
            if ((Request.Browser).Browser == "IE")
                return Json(result, "text/plain", JsonRequestBehavior.AllowGet);
            else
                return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To upload product file
        /// </summary>
        public JsonResult UploadProductFile(HttpPostedFileBase productfile)
        {
            if (productfile.ContentLength > 10240000)
            {
                if ((Request.Browser).Browser == "IE")
                    return Json(new ActionOutput { Status = ActionStatus.Error, Message = "File size must be less than 10 MB" }, "text/plain", JsonRequestBehavior.AllowGet);
                else
                    return Json(new ActionOutput { Status = ActionStatus.Error, Message = "File size must be less than 10 MB" }, JsonRequestBehavior.AllowGet);
            }
            string newFilename = Guid.NewGuid().ToString() + "." + productfile.FileName.Substring(productfile.FileName.LastIndexOf(".") + 1);
            string fileExt = System.IO.Path.GetExtension(productfile.FileName).ToLower();
            ActionOutput result;
            if (fileExt == ".stl")
            {
                // Saving Image to Temp Folder
                string temp_folder = Server.MapPath("~/Temp/ProductFiles/" + SiteUserDetails.LoggedInUser.Username + "/");
                if (!System.IO.Directory.Exists(temp_folder))
                    System.IO.Directory.CreateDirectory(temp_folder);
                productfile.SaveAs(temp_folder + newFilename);
                #region Temp Cookie
                // Check if cookie already exists
                ProductFiles files;
                ProductFileViewModel temp_file;
                HttpCookie file_temp_cookie = Request.Cookies[Cookies.ProductTempFile];
                if (file_temp_cookie == null)
                {
                    files = new ProductFiles();
                    temp_file = new ProductFileViewModel();
                    temp_file.DisplayName = productfile.FileName;
                    temp_file.MimeType = productfile.ContentType;
                    temp_file.SavedName = newFilename;
                    temp_file.SizeInBytes = productfile.ContentLength;
                    temp_file.SizeInKB = productfile.ContentLength / 1024;
                    temp_file.SizeInMB = productfile.ContentLength / (1024 * 1024);
                    files.ProductTempFiles = new List<ProductFileViewModel>();
                    files.ProductTempFiles.Add(temp_file);
                    // Creating Temp cookie for User product file
                    CreateCustomCookie(Cookies.ProductTempFile, false, new JavaScriptSerializer().Serialize(files));
                }
                else
                {
                    // update existing temp cookie
                    files = new JavaScriptSerializer().Deserialize<ProductFiles>(file_temp_cookie.Value);
                    files.ProductTempFiles.Add(new ProductFileViewModel
                    {
                        DisplayName = productfile.FileName,
                        MimeType = productfile.ContentType,
                        SavedName = newFilename,
                        SizeInBytes = productfile.ContentLength,
                        SizeInKB = productfile.ContentLength / 1024,
                        SizeInMB = productfile.ContentLength / (1024 * 1024)
                    });
                    UpdateCustomCookie(Cookies.ProductTempFile, false, new JavaScriptSerializer().Serialize(files));
                }
                #endregion
                result = new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { newFilename, productfile.FileName } };
            }
            else
            {
                result = new ActionOutput { Status = ActionStatus.Error, Results = new List<string> { SiteUserDetails.LoggedInUser.Username }, Message = "Invalid file, valid files are *.stl." };
            }
            if ((Request.Browser).Browser == "IE")
                return Json(result, "text/plain", JsonRequestBehavior.AllowGet);
            else
                return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To delete temp products files
        /// </summary>
        public JsonResult DeleteTempProductPicture(string filename, string pic_id)
        {
            // delete from disk
            Utility.DeleteFile(filename);
            // delete from cookie
            HttpCookie user_temp_cookie = Request.Cookies[Cookies.UserTempPictures];
            if (user_temp_cookie != null)
            {
                UserProductTempPictures temp_pics = new JavaScriptSerializer().Deserialize<UserProductTempPictures>(user_temp_cookie.Value);
                if (pic_id == "null" || String.IsNullOrEmpty(pic_id))
                    filename = filename.Split(new char[] { '/' })[3];
                else
                    filename = filename.Split(new char[] { '/' })[4];
                UserProductPicture pic = temp_pics.UserTempPictures.Where(m => m.Thumbnail == filename).FirstOrDefault();
                temp_pics.UserTempPictures.Remove(pic);
                // Creating Temp cookie for User product pics
                CreateCustomCookie(Cookies.UserTempPictures, false, new JavaScriptSerializer().Serialize(temp_pics));
            }
            return Json(new ActionOutput { Status = ActionStatus.Successfull }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To delete temp products files
        /// </summary>
        public JsonResult DeleteTempProductFile(string filename)
        {
            // delete from disk
            Utility.DeleteFile("~/Temp/ProductFiles/" + SiteUserDetails.LoggedInUser.Username + "/" + filename);
            // delete from cookie
            HttpCookie user_temp_cookie = Request.Cookies[Cookies.ProductTempFile];
            if (user_temp_cookie != null)
            {
                ProductFiles temp_files = new JavaScriptSerializer().Deserialize<ProductFiles>(user_temp_cookie.Value);
                //filename = filename.Split(new char[] { '/' })[3];
                ProductFileViewModel pic = temp_files.ProductTempFiles.Where(m => m.SavedName == filename).FirstOrDefault();
                temp_files.ProductTempFiles.Remove(pic);
                // Creating Temp cookie for User product pics
                CreateCustomCookie(Cookies.ProductTempFile, false, new JavaScriptSerializer().Serialize(temp_files));
            }
            return Json(new ActionOutput { Status = ActionStatus.Successfull }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To change shop section
        /// </summary>
        public JsonResult ChangeSection(int shop_section_id, string productIds, int shop_id)
        {
            string[] ids = productIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var result = ProductsHandler.ChangeSection(shop_section_id, ids);
            if (result.Status == ActionStatus.Successfull)
            {
                ViewBag.Products = ProductsHandler.ProductsByShopId(shop_id).List;
                ViewBag.Username = SiteUserDetails.LoggedInUser.Username;
                var list = RenderRazorViewToString("_Sections", SellersHandler.ShopSectionsByShopId(shop_id).List);
                return Json(
                    new ActionOutput
                    {
                        Status = result.Status,
                        Message = result.Message,
                        Results = new List<string> { list }
                    }, JsonRequestBehavior.AllowGet
                    );
            }
            else
            {
                return Json(
                    new ActionOutput
                    {
                        Status = result.Status,
                        Message = result.Message
                    }, JsonRequestBehavior.AllowGet
                    );
            }

        }

        /// <summary>
        /// To delete the shop banner
        /// </summary>
        public JsonResult DeleteBanner(int shop_id)
        {
            var file = SellersHandler.DeleteBanner(shop_id).Message;
            // deleting from disk
            Utility.DeleteFile("~/Images/ShopImages/" + SiteUserDetails.LoggedInUser.Username + "/Banner/" + file);
            return Json(new ActionOutput { Status = ActionStatus.Successfull, Message = "Banner has been deleted." }, JsonRequestBehavior.AllowGet);
        }

        #region Profile Section
        /// <summary>
        /// To Render buyer profile
        /// </summary>
        public ActionResult Profile()
        {
            User user = new User();
            var result = UsersHandler.GetUserByID(SiteUserDetails.LoggedInUser.Id);
            user = result.Object;
            user.UserTagsList = UsersHandler.GetUserTags(user.UserID);

            return View("Profile", user);

        }

        /// <summary>
        /// To update buyer profile
        /// </summary>
        public ActionResult EditProfile()
        {
            User user = new User();
            var result = UsersHandler.GetUserByID(SiteUserDetails.LoggedInUser.Id);
            user = result.Object;
            user.UserTagsList = UsersHandler.GetUserTags(user.UserID);
            user.PasswordRecoveryEmailId = user.EmailId;
            ViewBag.Countries = SellersHandler.CountryGetAll().List;
            ViewBag.States = SellersHandler.GetStateByCountryId(user.CountryId == null ? 0 : user.CountryId.Value).List;
            ViewBag.Cities = SellersHandler.GetCityByStateId(user.StateId == null ? 0 : user.StateId.Value).List;
            ViewBag.Usersettings = UsersHandler.GetUsersetting(SiteUserDetails.LoggedInUser.Id).Object;
            return View("EditProfile", user);

        }

        /// <summary>
        /// To updat buyer profile
        /// </summary>
        [HttpPost]
        public JsonResult EditProfile(User obj)
        {
            bool ShowUsername = Convert.ToBoolean(Request["ShowUsername"]);
            bool ShowFullname = Request["ShowFullname"] == "on" ? true : false;
            bool ShowLocation = Request["ShowLocation"] == "on" ? true : false;
            bool ShowSkills = Request["ShowSkills"] == "on" ? true : false;
            bool AcceptProject = Request["AcceptProject"] == "1" ? true : false;
            string Skills = Request["Skills"];
            string Services = Request["Services"];
            string ShopAvailability = Request["ShopAvailability"];

            if (obj.ShopID > 0)
            {
                SellersHandler.UpdateShopAcceptProject(obj.ShopID, AcceptProject);
            }
            if (!string.IsNullOrEmpty(ShopAvailability))
            {
                List<ShopAvailablity> slots = new List<ShopAvailablity>();
                if (ShopAvailability.Length > 0)
                {
                    string[] templist = ShopAvailability.Split(new char[] { ':' });
                    foreach (string items in templist)
                    {
                        string[] item = items.Split(new char[] { ',' });
                        slots.Add(new ShopAvailablity { ShopID = obj.ShopID, RowNum = Convert.ToInt32(item[0]), ColumnNum = Convert.ToInt32(item[1]) });
                    }
                }
                SellersHandler.SaveShopAvailablity(slots, obj.ShopID, Request["TimeZone"]);
            }
            if (!string.IsNullOrEmpty(Skills))
            {
                SellersHandler.UpdateShopSkills(obj.ShopID, Skills);
            }
            if (!string.IsNullOrEmpty(Services))
            {
                int[] ShopServices = Services.Split(',').Select(m => Convert.ToInt32(m)).ToArray();
                SellersHandler.UpdateShopServices(obj.ShopID, ShopServices);
            }
            else
            {
                SellersHandler.UpdateShopServices(obj.ShopID, null);
            }
            UserSetting setting = new UserSetting();
            setting.ShowFullname = ShowFullname;
            setting.ShowLocation = true; // ShowLocation; All users need to show their location and skill
            setting.ShowSkills = true; // ShowSkills;
            setting.UserID = obj.UserID;
            User user = new User();
            HttpCookie user_temp_cookie = Request.Cookies[Cookies.UserProfilePicture];
            if (user_temp_cookie != null)
            {
                // FormsAuthenticationTicket pics_ticket = FormsAuthentication.Decrypt(user_temp_cookie.Value);
                UserProductPicture user_pic = new JavaScriptSerializer().Deserialize<UserProductPicture>(user_temp_cookie.Value);
                var outPut = UsersHandler.UpdateUserProfilePic(user_pic, SiteUserDetails.LoggedInUser.Id);
                user_temp_cookie.Expires = DateTime.Now.AddDays(-30);
                Response.Cookies.Add(user_temp_cookie);
                // Delete all files from temp folder
                System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(Server.MapPath("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/"));
                folder.Empty();
            }
            UsersHandler.SaveShowUsernameSetting(setting);
            ActionOutput output = UsersHandler.UpdateUserDetails(obj, SiteUserDetails.LoggedInUser, "");
            ActionOutput<User> result = UsersHandler.GetUserByID(SiteUserDetails.LoggedInUser.Id);
            if (result.Object != null)
            {
                user = result.Object;
                var user_det = new UserDetails
                {
                    Email = user.EmailId,
                    Username = user.UserName,
                    Id = user.UserID,
                    UserRole = (UserRole)user.RoleId,
                    Gender = (Gender)user.Gender,
                    ShopDetails = user.ShopDetails,
                    ShopPictures = user.ShopPictures,
                    ProfilePicture = user.ProfilePicUrl,
                    UserLocation = user.UserLocation
                };
                UpdateCustomAuthorisationCookie(user_det);
            }
            return Json(output, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// To upload buyer profile picture
        /// </summary>
        public JsonResult UploadProfilePicture(HttpPostedFileBase file)
        {
            string newFilename = Guid.NewGuid().ToString() + "." + file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            string fileExt = System.IO.Path.GetExtension(file.FileName).ToLower();
            //if(photo.ContentLength>
            ActionOutput result;
            if (fileExt == ".jpeg" || fileExt == ".jpg" || fileExt == ".gif" || fileExt == ".bmp" || fileExt == ".png")
            {
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(file.InputStream);
                if (bitmap.Height > 1024 || bitmap.Height < 300)
                    return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Image must be between 300x300 to 1024x1024 pixel." }, JsonRequestBehavior.AllowGet);
                // Saving Image to Temp Folder
                string temp_folder = Server.MapPath("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/");
                if (!System.IO.Directory.Exists(temp_folder))
                    System.IO.Directory.CreateDirectory(temp_folder);
                file.SaveAs(temp_folder + newFilename);

                // Generating Thumbnail and save it.
                Bitmap thumbnail = new Bitmap(ImageProcessor.ResizeImage(bitmap, new Size(210, 210), false));
                thumbnail.Save(Server.MapPath("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/thumb_") + newFilename);

                #region Temp Cookie
                // Check if cookie already exists
                UserProductPicture temp_pics;
                HttpCookie user_temp_cookie = Request.Cookies[Cookies.UserProfilePicture];
                if (user_temp_cookie == null)
                {
                    temp_pics = new UserProductPicture
                       {
                           DisplayName = file.FileName,
                           MimeType = file.ContentType,
                           SavedName = newFilename,
                           Thumbnail = "thumb_" + newFilename,
                           Username = SiteUserDetails.LoggedInUser.Username,
                           SizeInBytes = file.ContentLength,
                           SizeInKB = file.ContentLength / 1024,
                           SizeInMB = file.ContentLength / (1024 * 1024)
                       };

                    // Creating Temp cookie for User product pics
                    CreateCustomCookie(Cookies.UserProfilePicture, false, new JavaScriptSerializer().Serialize(temp_pics));
                }
                else
                {
                    // update existing temp cookie
                    //FormsAuthenticationTicket pics_ticket = FormsAuthentication.Decrypt(user_temp_cookie.Value);
                    temp_pics = new JavaScriptSerializer().Deserialize<UserProductPicture>(user_temp_cookie.Value);

                    temp_pics = new UserProductPicture
                    {
                        DisplayName = file.FileName,
                        MimeType = file.ContentType,
                        SavedName = newFilename,
                        Thumbnail = "thumb_" + newFilename,
                        Username = SiteUserDetails.LoggedInUser.Username,
                        SizeInBytes = file.ContentLength,
                        SizeInKB = file.ContentLength / 1024,
                        SizeInMB = file.ContentLength / (1024 * 1024),
                    };
                    UpdateCustomCookie(Cookies.UserProfilePicture, false, new JavaScriptSerializer().Serialize(temp_pics));
                }
                #endregion

                result = new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Results = new List<string> 
                                                {
                                                    SiteUserDetails.LoggedInUser.Username,
                                                    newFilename,
                                                    "thumb_"+newFilename
                                                }
                };
            }
            else
            {
                result = new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Results = new List<string> {
                                                    SiteUserDetails.LoggedInUser.Username
                                                },
                    Message = "Invalid file, valid file are *.jpg, *.jpeg, *.gif, *.bmp, *.png"
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            if ((Request.Browser).Browser == "IE")
                return Json(result, "text/plain", JsonRequestBehavior.AllowGet);
            else
                return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To get state lists by country id
        /// </summary>
        public JsonResult GetStateList(Int32 country_id)
        {
            var result = SellersHandler.GetStateByCountryId(country_id);
            JavaScriptSerializer js = new JavaScriptSerializer();
            List<StateProvince> list = result.List;
            var userStates = new SelectList(list, "StateID", "StateName");
            return Json(new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { js.Serialize(userStates) } }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To get city list by state id
        /// </summary>
        public JsonResult GetCityList(Int32 state_id)
        {
            var result = SellersHandler.GetCityByStateId(state_id);
            JavaScriptSerializer js = new JavaScriptSerializer();
            List<City> list = result.List;
            var userCity = new SelectList(list, "CityID", "CityName");
            return Json(new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { js.Serialize(userCity) } }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To get subcategory list by cat id
        /// </summary>
        public JsonResult SubCategoriesGet(Int32 ID)
        {
            var result = SellersHandler.SubCategoriesGet(ID);
            JavaScriptSerializer js = new JavaScriptSerializer();
            List<SubCategory> list = result.List;
            var userCity = new SelectList(list, "SubCategoryID", "Name");
            return Json(new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { js.Serialize(userCity) } }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To get type list by sub cat id
        /// </summary>
        public JsonResult TypesGet(Int32 ID)
        {
            var result = SellersHandler.TypesGet(ID);
            JavaScriptSerializer js = new JavaScriptSerializer();
            List<BusinessLayer.Models.DataModel.Type> list = result.List;
            var userCity = new SelectList(list, "TypeID", "Name");
            return Json(new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { js.Serialize(userCity) } }, JsonRequestBehavior.AllowGet);
        }


        #endregion

        /// <summary>
        /// To Render Get paid section
        /// </summary>
        public ActionResult GetPaid()
        {
            #region Selected Tabs
            SelectedTabs.SellerMainTab = SelectedSellerTab.GetPaid;
            ViewBag.SelectedTabs = SelectedTabs;
            #endregion
            Shop model = SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object;
            if (model == null)
                return RedirectToAction("Shop", "User");

            // Add Venezuela as shipping country with 12% fix tax as per client's requirement.
            var ven = SellersHandler.GetCountryByShortCode("VE").Object;
            SalesTax taxV = new SalesTax { ToCountryID = ven.CountryID, ToStateID = null, Tax = 12, ShopID = model.ShopID, UserID = SiteUserDetails.LoggedInUser.Id };
            List<SalesTax> lst = new List<BusinessLayer.Models.DataModel.SalesTax>();
            lst.Add(taxV);
            SellersHandler.SalesTax(lst, false);
            // Add Venezuela as shipping country with 12% fix tax as per client's requirement.

            
            model = model == null ? new Shop() : model;
            ViewBag.StepCompleted = SellersHandler.GetShopSignUpStepCompleted(model.ShopID).Object;
            ViewBag.ShopId = model.ShopID;
            var user = UsersHandler.UserRegDetail(SiteUserDetails.LoggedInUser.Id).Results;
            ViewBag.UserRegDetail = "Your account was registered on " + user[1] + ".";

            // sales tax 
            var usa = SellersHandler.GetCountryByShortCode("US").Object;
            ViewBag.USID = usa.CountryID;
            ViewBag.States = SellersHandler.StatesByCountry(usa.CountryID).List;
            ViewBag.Countries = SellersHandler.CountryGetAll().List;
            var tax = UsersHandler.GetSalesTaxByUserID(SiteUserDetails.LoggedInUser.Id).List;
            ViewBag.ShopId = model.ShopID;
            ViewBag.ShipToUSOnly = model.ShipToUSOnly;
            IsCompleted(model.ShopID);
            return View(tax);
        }

        /// <summary>
        /// To Render payments section
        /// </summary>
        [AjaxOnly]
        public PartialViewResult _Payments()
        {
            var user = UsersHandler.GetUserByID(SiteUserDetails.LoggedInUser.Id).Object;
            ViewBag.Countries = SellersHandler.CountryGetAll().List;
            ViewBag.States = SellersHandler.GetStateByCountryId(user.CountryId == null ? 0 : user.CountryId.Value).List;
            ViewBag.Cities = SellersHandler.GetCityByStateId(user.StateId == null ? 0 : user.StateId.Value).List;
            return PartialView(user);
        }

        /// <summary>
        /// To save buyer paypal information
        /// </summary>
        public JsonResult Payments(User obj)
        {
            return Json(UsersHandler.UpdateUserPaypalID(obj), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To Render sales tax information
        /// </summary>
        [AjaxOnly]
        public PartialViewResult _SalesTax(int shop_id)
        {
            Shop model = SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object;
            var usa = SellersHandler.GetCountryByShortCode("US").Object;
            ViewBag.USID = usa.CountryID;
            ViewBag.States = SellersHandler.StatesByCountry(usa.CountryID).List;
            ViewBag.Countries = SellersHandler.CountryGetAll().List;
            var tax = UsersHandler.GetSalesTaxByUserID(SiteUserDetails.LoggedInUser.Id).List;
            ViewBag.ShopId = shop_id;
            ViewBag.ShipToUSOnly = model.ShipToUSOnly;
            return PartialView(tax);
        }

        /// <summary>
        /// To get USA sales tax from API
        /// </summary>
        public JsonResult USSalesTax()
        {
            int user_id = SiteUserDetails.LoggedInUser.Id;
            int shop_id = Convert.ToInt32(Request["ShopId"]);
            string[] selectedStates = Request["SelectedStates"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<SalesTax> salesTaxList = new List<SalesTax>();
            foreach (string state in selectedStates)
            {
                salesTaxList.Add(new SalesTax
                {
                    ShopID = shop_id,
                    Tax = Convert.ToDecimal(Request["USTax_" + state]),
                    ToCountryID = Convert.ToInt32(Request["USID"]),
                    ToStateID = Convert.ToInt32(Request["State_" + state]),
                    UserID = SiteUserDetails.LoggedInUser.Id
                });
            }
            return Json(SellersHandler.SalesTax(salesTaxList, true), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To save sales tax
        /// </summary>
        public JsonResult SalesTax()
        {

            int user_id = SiteUserDetails.LoggedInUser.Id;
            int shop_id = Convert.ToInt32(Request["ShopId"]);
            bool ShipToUSOnly = Request["ShipToUSOnly"] != null ? false : true; //Convert.ToBoolean(Request["ShipToUSOnly"]);
            BusinessLayer.Models.DataModel.Shop shop = new BusinessLayer.Models.DataModel.Shop();
            shop.ShopID = shop_id;
            shop.ShipToUSOnly = ShipToUSOnly;
            SellersHandler.ShipToUsaOnly(shop);

            if (ShipToUSOnly)
            {
                return Json(SellersHandler.DeleteSalesTaxByShopId(shop_id), JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (Request["SelectedCountries"].Length <= 0)
                    return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Please add at-least one tax rate!!!" }, JsonRequestBehavior.AllowGet);

                string[] selectedCountries = Request["SelectedCountries"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string[] SelectedStates = Request["SelectedStates"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                List<SalesTax> salesTaxList = new List<SalesTax>();
                string usaTaxs = "";
                foreach (string country in selectedCountries)
                {
                    if (country == "1") usaTaxs = Request["Tax_" + country];
                    salesTaxList.Add(new SalesTax
                    {
                        ShopID = shop_id,
                        Tax = country == "1" ? 0 : Convert.ToDecimal(Request["Tax_" + country]),
                        ToCountryID = country == "1" ? 1 : Convert.ToInt32(Request["Country_" + country]),
                        ToStateID = null,
                        UserID = SiteUserDetails.LoggedInUser.Id
                    });
                }
                string[] taxes = string.IsNullOrWhiteSpace(usaTaxs) ? null : usaTaxs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string state in SelectedStates) // 1,15
                {
                    for (short i = 0; i < salesTaxList.Count(); i++)
                    {
                        if (!salesTaxList[i].ToStateID.HasValue && salesTaxList[i].ToCountryID == 1)
                        {
                            salesTaxList[i].ToStateID = (int?)Convert.ToInt32(state);
                            salesTaxList[i].Tax = Convert.ToDecimal(Request["Tax_" + state]);
                            break;
                        }
                    }
                }

                return Json(SellersHandler.SalesTax(salesTaxList, false), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// To delete sales tax
        /// </summary>
        public JsonResult DeleteSalesTax(int sales_tax_id)
        {
            return Json(SellersHandler.DeleteSalesTax(sales_tax_id), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To Render preview shop page
        /// </summary>
        public ActionResult PreviewShop()
        {
            #region Selected Tabs
            SelectedTabs.SellerMainTab = SelectedSellerTab.PreviewShop;
            ViewBag.SelectedTabs = SelectedTabs;
            #endregion
            Shop model = SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object;
            if (model == null)
                return RedirectToAction("Shop", "User");
            model = model == null ? new Shop() : model;
            ViewBag.StepCompleted = SellersHandler.GetShopSignUpStepCompleted(model.ShopID).Object;
            ViewBag.ShopId = model.ShopID;
            ViewBag.Username = SiteUserDetails.LoggedInUser.Username;
            #region Section Details
            ViewBag.ShopSections = SellersHandler.ShopSectionsByShopId(model.ShopID).List;
            #endregion
            #region Shop Items
            ViewBag.Products = ProductsHandler.ProductsByShopId(model.ShopID).List;
            #endregion
            ViewBag.User = UsersHandler.GetUserByID(SiteUserDetails.LoggedInUser.Id).Object;
            return View(model);
        }

        /// <summary>
        /// To activate the shop 
        /// </summary>
        public JsonResult OpenShop(int shop_id)
        {
            Shop shop = SellersHandler.ShopByShopId(shop_id).Object;
            var output = SellersHandler.OpenShop(shop_id, SiteUserDetails.LoggedInUser.Id);
            if (output.Status == ActionStatus.Successfull)
                output.Results = new List<string> { Url.Action("Shop", "Shops") + "/" + shop.ShopName };
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To Render shop information page
        /// </summary>
        [SkipAuthentication]
        public JsonResult ShopInformation(int shop_id, string tab)
        {
            ViewBag.Action = tab;
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string> 
                { 
                    RenderRazorViewToString("_ShopInformation", SellersHandler.ShopByShopId(shop_id).Object)
                }
            });
        }

        /// <summary>
        /// To Render Signup page
        /// </summary>
        [AjaxOnly]
        public PartialViewResult Listing(int shop_id)
        {
            int owner_shop_id = SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object.ShopID;
            ViewBag.ShopID = shop_id;
            ViewBag.ShowTableFormat = true;
            ViewBag.Inactive = null;
            ViewBag.SelectedTab = SelectedProductListingTab.Active;
            return PartialView(ProductsHandler.ProductsByShopPaging(shop_id, null, 1, 12, "CreatedOn", "asc", null, owner_shop_id));
        }

        /// <summary>
        /// To Render listing page
        /// </summary>
        [AjaxOnly]
        public PartialViewResult _ManageListing(int shop_id)
        {
            ViewBag.ShopID = shop_id;
            ViewBag.ShowTableFormat = true;

            ViewBag.SelectedTab = SelectedProductListingTab.Active;
            int owner_shop_id = SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object.ShopID;
            return PartialView(ProductsHandler.ProductsByShopPaging(shop_id, null, 1, 12, "CreatedOn", "asc", null, owner_shop_id));
        }

        /// <summary>
        /// To Render inactive products
        /// </summary>
        [AjaxOnly]
        public PartialViewResult _InactiveListing(int shop_id)
        {
            ViewBag.ShopID = shop_id;
            ViewBag.ShowTableFormat = true;
            ViewBag.SelectedTab = SelectedProductListingTab.Inactive;
            int owner_shop_id = SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object.ShopID;
            return PartialView(ProductsHandler.ProductsByShopPaging(shop_id, null, 1, 12, "CreatedOn", "asc", 1, owner_shop_id));
        }

        /// <summary>
        /// To Render sold out listing
        /// </summary>
        [AjaxOnly]
        public PartialViewResult _SoldOutListing(int shop_id)
        {
            ViewBag.ShopID = shop_id;
            ViewBag.ShowTableFormat = true;
            ViewBag.SelectedTab = SelectedProductListingTab.SoldOut;
            return PartialView(new PagingResult<ProductsListing_Result>());
        }

        /// <summary>
        /// To update product information
        /// </summary>
        public JsonResult UpdateProducts(ProductAction obj, string selectedTab)
        {
            var result = ProductsHandler.UpdateProducts(obj, SiteUserDetails.LoggedInUser.Id);
            var list = new PagingResult<ProductsListing_Result>();
            int owner_shop_id = SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object.ShopID;
            if (obj.ActionID == ActionOnProduct.Activate)
            {
                ViewBag.SelectedTab = SelectedProductListingTab.Inactive;
                list = ProductsHandler.ProductsByShopPaging(obj.ShopId, null, 1, 12, "CreatedOn", "asc", 1, owner_shop_id);
            }
            else if (obj.ActionID == ActionOnProduct.DeActivate)
            {
                ViewBag.SelectedTab = SelectedProductListingTab.Active;
                list = ProductsHandler.ProductsByShopPaging(obj.ShopId, null, 1, 12, "CreatedOn", "asc", null, owner_shop_id);
            }
            else
            {
                if (Convert.ToInt32(selectedTab) == (int)SelectedProductListingTab.Active)
                {
                    ViewBag.SelectedTab = SelectedProductListingTab.Active;
                    list = ProductsHandler.ProductsByShopPaging(obj.ShopId, null, 1, 12, "CreatedOn", "asc", null, owner_shop_id);

                }
                if (Convert.ToInt32(selectedTab) == (int)SelectedProductListingTab.Inactive)
                {
                    ViewBag.SelectedTab = SelectedProductListingTab.Inactive;
                    list = ProductsHandler.ProductsByShopPaging(obj.ShopId, null, 1, 12, "CreatedOn", "asc", 1, owner_shop_id);
                }

            }

            ViewBag.ShowTableFormat = true;
            ViewBag.ShopID = obj.ShopId;
            string data = RenderRazorViewToString("_Products", list);
            return Json(new ActionOutput
            {
                Status = result.Status,
                Message = result.Message,
                Results = new List<string> 
                { 
                    data, list.TotalCount.ToString() 
                }
            }, JsonRequestBehavior.AllowGet);

        }

        #region ChangePassword
        public ActionResult ChangePassword()
        {
            return View(new ChangePasswordModel());
        }
        [HttpPost]
        public JsonResult ChangePassword(ChangePasswordModel obj)
        {
            //obj.ConfirmPassword = obj.CurrentPassword = Utility.EncryptString(obj.CurrentPassword);
            //obj.NewPassword = Utility.EncryptString(obj.NewPassword);
            var result = UsersHandler.ChangeUserPassword(SiteUserDetails.LoggedInUser.Id, obj);

            return Json(new ActionOutput
            {
                Status = result.Status,
                Message = result.Message,
                Results = new List<string> 
                    { 
                    Url.Action("Profile", "User") 
                    }
            }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        //[SkipAuthentication]
        /// <summary>
        /// To download product file
        /// </summary>
        public ActionResult DownloadFile(int user_id, int product_id)
        {
            // Check for login
            if (SiteUserDetails.LoggedInUser == null)
                return RedirectToAction("Signin", "Home");

            // Check for correct logged in user
            if (SiteUserDetails.LoggedInUser.Id != user_id)
                return RedirectToAction("Home", "User");

            // Fetch product files
            List<ProductFile> files = ProductsHandler.GetProductFiles(product_id).List;
            string shop_owner = SellersHandler.ShopOwnerByProductId(product_id).Object.UserName;
            // Create the zip and put it on response stream for download

            string archiveName = String.Format("archive-{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
            // see http://support.microsoft.com/kb/260519
            //Response.AddHeader("content-disposition", "attachment; filename=" + archiveName);
            using (ZipFile zip = new ZipFile())
            {
                foreach (ProductFile file in files)
                {
                    zip.AddFile(Server.MapPath("~/Images/DownloadableFiles/" + shop_owner + "/" + file.SavedName), @"\");
                }
                //zip.Save(Response.OutputStream);
                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = archiveName,
                    Inline = false,
                };
                Response.AppendHeader("Content-Disposition", cd.ToString());
                var memStream = new MemoryStream();
                zip.Save(memStream);
                memStream.Position = 0; // Else it will try to read starting at the end
                return File(memStream, "application/zip");
            }
        }

        /// <summary>
        /// To save shop availability
        /// </summary>
        public JsonResult SaveShopAvailablity(string slotlist, int shop_id, string TimeZone)
        {
            List<ShopAvailablity> slots = new List<ShopAvailablity>();
            if (slotlist.Length > 0)
            {
                string[] templist = slotlist.Split(new char[] { ':' });
                foreach (string items in templist)
                {
                    string[] item = items.Split(new char[] { ',' });
                    slots.Add(new ShopAvailablity { ShopID = shop_id, RowNum = Convert.ToInt32(item[0]), ColumnNum = Convert.ToInt32(item[1]) });
                }
            }
            return Json(SellersHandler.SaveShopAvailablity(slots, shop_id, TimeZone), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To render favorite products
        /// </summary>
        public ActionResult FavoriteProducts()
        {
            ViewBag.CentroUsers = SiteUserDetails;
            return View(UsersHandler.GetFavoriteProductsByUserId(SiteUserDetails.LoggedInUser.Id).List);
        }

        /// <summary>
        /// To Render favorite products
        /// </summary>
        [AjaxOnly]
        public PartialViewResult _FavProducts()
        {
            return PartialView(UsersHandler.GetFavoriteProductsByUserId(SiteUserDetails.LoggedInUser.Id).List);
        }

        /// <summary>
        /// To Render favorite shops
        /// </summary>
        [AjaxOnly]
        public PartialViewResult _FavShops()
        {
            return PartialView(UsersHandler.GetFavoriteShopByUserIdForView(SiteUserDetails.LoggedInUser.Id).List);
        }

        

        /// <summary>
        /// To get activity notification
        /// </summary>
        public JsonResult NewActivityNotifications(DateTime CurrentTime)
        {
            PagingResult<AccountActivityViewModel> model = AccountActivityHandler.ActivityFeeds(1, 10, "Desc", "CreatedOn", CurrentTime, SiteUserDetails.LoggedInUser.Id);
            string view = RenderRazorViewToString("_ActivityNotifications", model.List);
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string> { view, model.TotalCount.ToString(), model.Message }
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To Render new alerts
        /// </summary>
        public JsonResult NewAlerts()
        {
            var model = AccountActivityHandler.GetAlerts(SiteUserDetails.LoggedInUser.Id, 5);
            string view = RenderRazorViewToString("_AlertItems", model.List);
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string> { view, model.List.Count().ToString() }
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To Render create job page
        /// </summary>
        public ActionResult CreateJob(int? id, string from, string username)
        {
            List<SelectListItem> CategoryList;

            HttpCookie file_temp_cookie = Request.Cookies[Cookies.TempFileAttachments];
            if (file_temp_cookie != null) { file_temp_cookie.Expires = DateTime.Now.AddDays(-1); Response.Cookies.Add(file_temp_cookie); }
            BuyerJob job;
            if (id.HasValue && id.Value > 0)
            {
                job = SellersHandler.GetJob(id.Value).Object;
                CategoryList = Enum.GetValues(typeof(Services)).Cast<Services>().Select(v => new SelectListItem
                {
                    Text = v.ToEnumDescription(),
                    Value = Convert.ToInt32(v).ToString(),
                    Selected = Convert.ToInt32(v) == job.CategoryId ? true : false
                }).ToList();

                if (job.IsPrivate) job.Seller = UsersHandler.GetUserByID(job.JobSentTo.Value).Object.UserName + ";";
                List<BuyerJobAttachment> attachment_list = job.BuyerJobAttachments.ToList();
                RequestAttachmentsTempFile temp_files = new RequestAttachmentsTempFile();
                if (attachment_list.Count > 0)
                {
                    HttpCookie request_temp_cookie = Request.Cookies[Cookies.TempFileAttachments];
                    if (request_temp_cookie != null)
                    {
                        request_temp_cookie.Expires = DateTime.Now.AddDays(-30);
                        Response.Cookies.Add(request_temp_cookie);
                    }
                    request_temp_cookie = new HttpCookie(Cookies.TempFileAttachments);

                    temp_files.RequestAttachments = new List<FileAttachmentViewModel>();
                    foreach (var file in attachment_list)
                    {
                        temp_files.RequestAttachments.Add(new FileAttachmentViewModel
                        {
                            DisplayName = file.DisplayName,
                            MimeType = file.MimeType,
                            SavedName = file.SavedName,
                            SizeInBytes = file.SizeInBytes,
                            SizeInKB = Convert.ToInt32(file.SizeInKb),
                            SizeInMB = Convert.ToInt32(file.SizeInMb),
                            AttachmentID = file.AttachmentID

                        });
                    }
                    // Creating Temp cookie for User product pics
                    CreateCustomCookie(Cookies.TempFileAttachments, false, new JavaScriptSerializer().Serialize(temp_files));
                    ViewBag.RequestAttachments = temp_files.RequestAttachments;
                }
            }
            else
            {
                job = new BuyerJob();
                if (!string.IsNullOrEmpty(username))
                {
                    job.IsPrivate = true;
                    job.Seller = username;
                }
                CategoryList = Enum.GetValues(typeof(Services)).Cast<Services>().Select(v => new SelectListItem
                {
                    Text = v.ToEnumDescription(),
                    Value = Convert.ToInt32(v).ToString()
                }).ToList();
            }
            ViewBag.CategoryList = CategoryList;
            ViewBag.From = from;
            return View(job);
        }

        /// <summary>
        /// To save/update a job
        /// </summary>
        public JsonResult SaveOrUpdateJob(BuyerJob obj)
        {
            bool Create = obj.JobID > 0 ? false : true;
            obj.BuyerID = SiteUserDetails.LoggedInUser.Id;
            HttpCookie file_temp_cookie = Request.Cookies[Cookies.TempFileAttachments];
            bool MyJob = Request["From"] == "Repost" ? true : false;

            var output = SellersHandler.SaveOrUpdateJob(obj, SiteUserDetails.LoggedInUser.Id, MyJob);

            if (Create)
            {
                AccountActivity objActivity = new AccountActivity();
                objActivity.ActivityID = output.ID;
                objActivity.ActivityType = (int)FollowType.Jobs;
                objActivity.UserID = SiteUserDetails.LoggedInUser.Id;
                objActivity.ActivityImage = "";
                objActivity.ActivityLink = Url.Action("Job", "User", new { id = output.ID, title = Utility.SpacesToHifen(obj.JobTitle) });
                objActivity.ActivityText = ActivityText.JobCreate.Replace("{UserName}", SiteUserDetails.LoggedInUser.Username)
                                                                     .Replace("{JobTitle}", obj.JobTitle)
                                                                     .Replace("{Link}", objActivity.ActivityLink)
                                                                     .Replace("{Price}", Math.Round(obj.MinBudget, 2).ToString())
                                                                     .Replace("{Date}", DateTime.Now.ToShortTimeString() + " " + DateTime.Now.ToLongDateString());
                AccountActivityHandler.SaveActivity(objActivity);
            }
            else
            {
                AccountActivity objActivity = new AccountActivity();
                objActivity.ActivityID = output.ID;
                objActivity.ActivityType = (int)FollowType.Jobs;
                objActivity.UserID = SiteUserDetails.LoggedInUser.Id;
                objActivity.ActivityImage = "";
                objActivity.ActivityLink = Url.Action("Job", "User", new { id = output.ID, title = Utility.SpacesToHifen(obj.JobTitle) });
                objActivity.ActivityText = ActivityText.JobUpdate.Replace("{UserName}", SiteUserDetails.LoggedInUser.Username)
                                                                     .Replace("{JobTitle}", obj.JobTitle)
                                                                     .Replace("{Link}", objActivity.ActivityLink)
                                                                     .Replace("{Date}", DateTime.Now.ToShortTimeString() + " " + DateTime.Now.ToLongDateString());
                AccountActivityHandler.SaveActivity(objActivity);
            }
            if (obj.IsPrivate && output.Status == ActionStatus.Successfull)
            {
                // log alert into database
                AccountActivityHandler.SaveAlert(new Alert
                {
                    AlertForID = obj.JobID,
                    AlertLink = "/User/Job/" + obj.JobID + "/" + Utility.SpacesToHifen(obj.JobTitle),
                    AlertText = SiteUserDetails.LoggedInUser.Username + " has posted a private job \"" + obj.JobTitle + "\" for you.",
                    UserID = obj.JobSentTo.Value
                });
            }
            if (file_temp_cookie != null && output.Status == ActionStatus.Successfull)
            {
                RequestAttachmentsTempFile temp_file = new JavaScriptSerializer().Deserialize<RequestAttachmentsTempFile>(file_temp_cookie.Value);
                // Saving Product files into database
                SellersHandler.UpdateJobAttachments(SiteUserDetails.LoggedInUser.Username, temp_file.RequestAttachments, Convert.ToInt32(output.Results[0]));
                // Remove temp cookie
                file_temp_cookie.Expires = DateTime.Now.AddDays(-30);
                Response.Cookies.Add(file_temp_cookie);
                // Delete all files from temp folder
                System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(Server.MapPath("~/Temp/RequestAttachments/" + SiteUserDetails.LoggedInUser.Username + "/"));
                folder.Empty();
            }
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To Render jobs
        /// </summary>
        /// 
        [SkipAuthentication]
        public ActionResult Jobs()
        {
            HttpCookie cookie = Request.Cookies[Cookies.JobsFilter];
            if (cookie != null) { cookie.Expires = DateTime.Now.AddDays(-1); Response.Cookies.Add(cookie); }
            ViewBag.UserID = null;
            ViewBag.LoggedInUser = SiteUserDetails;
            ViewBag.LoggedInUsername = SiteUserDetails.LoggedInUser != null ? SiteUserDetails.LoggedInUser.Username : "";
            ViewBag.LoggedInUserID = SiteUserDetails.LoggedInUser != null ? SiteUserDetails.LoggedInUser.Id : 0;
            ViewBag.From = "Jobs";
            return View(SellersHandler.GetJobs(1, 6, "CreatedOn", "Desc", null, null));
        }

        /// <summary>
        /// To Render apply job popup
        /// </summary>
        public JsonResult ApplyForJobPopup(int id)
        {
            if (SiteUserDetails.LoggedInUser != null)
            {
                var job = SellersHandler.GetJob(id).Object;
                ViewBag.MinBudget = job.MinBudget;
                return Json(new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Results = new List<string>
                    {
                        RenderRazorViewToString("_ApplyForJobPopup", id)
                    }
                });
            }
            else
            {
                return Json(new ActionOutput { Status = ActionStatus.Error }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// To Render no shop popup
        /// </summary>
        public JsonResult NoShop()
        {
            if (SiteUserDetails.LoggedInUser != null)
            {
                return Json(new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Results = new List<string>
                    {
                        RenderRazorViewToString("_NoShopPopup", null)
                    }
                });
            }
            else
            {
                return Json(new ActionOutput { Status = ActionStatus.Error }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// To apply on a job
        /// </summary>
        public JsonResult ApplyForJob()
        {
            int JobID = Convert.ToInt32(Request["JobID"]);
            decimal BidAmount = Convert.ToDecimal(Request["BidAmount"]);
            return Json(SellersHandler.ApplyForJob(JobID, BidAmount, SiteUserDetails.LoggedInUser.Id, SiteUserDetails.LoggedInUser.Username), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To Render apply for a job without a shop
        /// </summary>
        public JsonResult ApplyForJobPopupNoShop()
        {
            if (SiteUserDetails.LoggedInUser != null)
            {
                return Json(new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Results = new List<string>
                    {
                        RenderRazorViewToString("_ApplyForJobPopupNoShop", null)
                    }
                });
            }
            else
            {
                return Json(new ActionOutput { Status = ActionStatus.Error }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// To withdraw job application
        /// </summary>
        public JsonResult WithdrawApplication(int JobID)
        {
            return Json(SellersHandler.WithdrawApplication(JobID, SiteUserDetails.LoggedInUser.Id), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To list jobs
        /// </summary>
        public JsonResult _Jobs(int page_no, int per_page_result, string sortOrder, string sortColumn, string search, int? UserID, bool IsFindJobs = false)
        {
            ViewBag.LoggedInUsername = SiteUserDetails.LoggedInUser.Username;
            ViewBag.LoggedInUserID = SiteUserDetails.LoggedInUser.Id;
            JobsFilter filter = null;
            HttpCookie cookie = Request.Cookies[Cookies.JobsFilter];
            if (cookie != null)
                filter = new JavaScriptSerializer().Deserialize<JobsFilter>(cookie.Value);
            PagingResult<JobsViewModel> model = SellersHandler.GetJobs(page_no, per_page_result, sortColumn, sortOrder, UserID, filter);
            string view = RenderRazorViewToString(IsFindJobs ? "_FindJobListing" : "_JobListing", model.List);
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string> { view, model.TotalCount.ToString(), model.Message }
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To Render my jobs
        /// </summary>
        public ActionResult MyJobs()
        {
            HttpCookie cookie = Request.Cookies[Cookies.JobsFilter];
            if (cookie != null) { cookie.Expires = DateTime.Now.AddDays(-1); Response.Cookies.Add(cookie); }
            ViewBag.Title = "";
            ViewBag.UserID = SiteUserDetails.LoggedInUser.Id;
            ViewBag.LoggedInUserID = SiteUserDetails.LoggedInUser.Id;
            ViewBag.LoggedInUsername = SiteUserDetails.LoggedInUser.Username;
            ViewBag.From = "MyJobs";
            return View("Jobs", SellersHandler.GetJobs(1, 6, "CreatedOn", "Desc", SiteUserDetails.LoggedInUser.Id, null, "MyJobs"));
        }

        /// <summary>
        /// To Render a job
        /// </summary>
        //[SkipAuthentication]
        public ActionResult Job(int id, string title)
        {
            ViewBag.JobApplicants = SellersHandler.GetJobApplicants(id).List;
            //ViewBag.LoggedInUserId = CentroUsers.LoggedInUser.Id;
            return View(SellersHandler.GetJob(id).Object);
        }

        /// <summary>
        /// To Render my job
        /// </summary>
        public ActionResult MyJob(int id, string title)
        {
            ViewBag.JobApplicants = SellersHandler.GetJobApplicants(id).List;
            var model = SellersHandler.GetJob(id).Object;
            ViewBag.LoggedInUserId = SiteUserDetails.LoggedInUser.Id;
            //if (model.BuyerID == CentroUsers.LoggedInUser.Id)
            return View(model);
            // return RedirectToAction("Jobs", "User");
        }

        /// <summary>
        /// To download job file.
        /// </summary>
        public FileResult DownloadJobFile(int id, string username)
        {
            BuyerJobAttachment file = SellersHandler.GetJobAttachment(id).Object;
            var filedata = Utility.ReadFileBytes("~/Images/Attachments/" + username + "/" + file.SavedName);
            return File(filedata, file.MimeType);
        }

        /// <summary>
        /// To set sent to
        /// </summary>
        public JsonResult SetSentTo(int JobID, int UserID)
        {
            return Json(SellersHandler.SetSentTo(JobID, UserID), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To award the job
        /// </summary>
        public JsonResult AwardJobTo(int JobID, int UserID, int ShopID)
        {
            return Json(SellersHandler.AwardJobTo(JobID, UserID, ShopID, SiteUserDetails.LoggedInUser.Username), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To Render Signup page
        /// </summary>
        public JsonResult DeleteJobEntry(int JobApplicationID)
        {
            return Json(SellersHandler.DeleteJobEntry(JobApplicationID), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To delete a job
        /// </summary>
        public JsonResult DeleteJob(int JobID)
        {
            return Json(SellersHandler.DeleteJob(JobID), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To activate/deactivae a job
        /// </summary>
        public JsonResult ActivateDeactivateJob(int JobID, bool IsActive)
        {
            return Json(SellersHandler.ActivateDeactivateJob(JobID, IsActive), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To get my contracts
        /// </summary>
        public ActionResult MyContracts()
        {
            ViewBag.UserID = null;
            ViewBag.LoggedInUsername = SiteUserDetails.LoggedInUser.Username;
            ViewBag.LoggedInUserID = SiteUserDetails.LoggedInUser.Id;
            ViewBag.JobSentToMe = SellersHandler.GetJobs(1, 6, "CreatedOn", "Desc", null, SiteUserDetails.LoggedInUser.Id, false);
            return View();
        }

        /// <summary>
        /// To Render my contracts partial
        /// </summary>
        public PartialViewResult _MyActiveContracts()
        {
            return PartialView("_Contracts", "Active");
        }

        /// <summary>
        /// To get my client's contracts
        /// </summary>
        public PartialViewResult _MyActiveClients()
        {
            return PartialView("_MyClients", "Active");
        }

        /// <summary>
        /// To render past clients
        /// </summary>
        public PartialViewResult _MyPastClients()
        {
            return PartialView("_MyClients", "Past");
        }

        public PartialViewResult _MyActiveContractors()
        {
            return PartialView("_MyContractors", "Active");
        }

        public PartialViewResult _MyPastContractors()
        {
            return PartialView("_MyContractors", "Past");
        }

        public JsonResult MoveTo(string Ids, string MoveTo)
        {
            List<long> RequestIds = Ids.Split(',').Select(m => Convert.ToInt64(m)).ToList();
            return Json(SellersHandler.MoveTo(RequestIds, MoveTo), JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult _JobSentToMe(int page_no, int per_page_result, string sortOrder, string sortColumn)
        {
            ViewBag.LoggedInUsername = SiteUserDetails.LoggedInUser.Username;
            ViewBag.LoggedInUserID = SiteUserDetails.LoggedInUser.Id;
            ViewBag.Title = "Potential Jobs";
            return PartialView("_JobListing", SellersHandler.GetJobs(page_no, per_page_result, sortColumn, sortOrder, null, SiteUserDetails.LoggedInUser.Id, false).List);
        }

        public PartialViewResult _JobsApplied(int page_no, int per_page_result)
        {
            ViewBag.LoggedInUsername = SiteUserDetails.LoggedInUser.Username;
            ViewBag.LoggedInUserID = SiteUserDetails.LoggedInUser.Id;
            ViewBag.Title = "";
            return PartialView("_JobListing", SellersHandler.JobsApplied(page_no, per_page_result, SiteUserDetails.LoggedInUser.Id).List);
        }

        public JsonResult FilterJobs()
        {
            string Keyword = Request["Job-Keyword"].ToString();
            decimal MinBudget = Request["MinBudget"] != null ? Convert.ToDecimal(Request["MinBudget"]) : 10;
            // decimal MaxBudget = Request["MaxBudget"] != null ? Convert.ToDecimal(Request["MaxBudget"]) : 9999999;
            string SearchType = Request["searchType"];

            JobsFilter filter = new JobsFilter
            {
                Keyword = Keyword.ToLower(),
                LoggedInUserID = SiteUserDetails.LoggedInUser.Id,
                MinBudget = MinBudget,
                // MaxBudget = MaxBudget,
                SearchType = SearchType
            };
            CreateCustomCookie(Cookies.JobsFilter, false, new JavaScriptSerializer().Serialize(filter), 5);
            PagingResult<JobsViewModel> model = SellersHandler.GetJobs(1, 6, "CreatedOn", "Desc", null, filter);
            ViewBag.LoggedInUsername = SiteUserDetails.LoggedInUser.Username;
            ViewBag.LoggedInUserID = SiteUserDetails.LoggedInUser.Id;
            return Json(
                new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Results = new List<string> { RenderRazorViewToString("_FindJobListing", model.List), model.TotalCount.ToString() }
                }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Recruit()
        {
            ViewBag.LoggedInUsername = SiteUserDetails.LoggedInUser.Username;
            ViewBag.LoggedInUserID = SiteUserDetails.LoggedInUser.Id;
            ViewBag.JobSentToMe = SellersHandler.GetJobs(1, 6, "CreatedOn", "Desc", null, SiteUserDetails.LoggedInUser.Id, false);
            ViewBag.JobsApplied = SellersHandler.JobsApplied(1, 6, SiteUserDetails.LoggedInUser.Id);
            JobsFilter filter = new JobsFilter();
            filter.IsAwarded = false;
            return View(SellersHandler.GetMyJobs(1, 10, "CreatedOn", "Desc", SiteUserDetails.LoggedInUser.Id, filter, "MyJobs"));
        }

        public ActionResult _MyJobs(int page_no, int per_page_result, string sortOrder, string sortColumn, bool IsAwarded, bool? isPaging = null)
        {
            ViewBag.LoggedInUsername = SiteUserDetails.LoggedInUser.Username;
            ViewBag.LoggedInUserID = SiteUserDetails.LoggedInUser.Id;
            JobsFilter filter = new JobsFilter();
            filter.IsAwarded = IsAwarded;
            PagingResult<JobsViewModel> model = SellersHandler.GetMyJobs(page_no, per_page_result, sortColumn, sortOrder, SiteUserDetails.LoggedInUser.Id, filter, "MyJobs");
            string view = RenderRazorViewToString("_MyJobsListing", model);
            ViewBag.isAwared = IsAwarded;
            if (isPaging == null)
            {
                return PartialView("_MyJobsListing", SellersHandler.GetMyJobs(page_no, per_page_result, sortColumn, sortOrder, SiteUserDetails.LoggedInUser.Id, filter, "MyJobs"));
            }
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string> { view, model.TotalCount.ToString(), model.Message }
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Alerts()
        {
            return View(AccountActivityHandler.GetAlerts(SiteUserDetails.LoggedInUser.Id, 1, 20, "Desc", "CreatedOn"));
        }

        public JsonResult _AlertsMain(int page_no, int per_page_result, string sortOrder, string sortColumn)
        {
            var model = AccountActivityHandler.GetAlerts(SiteUserDetails.LoggedInUser.Id, page_no, per_page_result, sortOrder, sortColumn);
            string view = RenderRazorViewToString("_AlertsMain", model.List);
            return Json(new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { view, model.TotalCount.ToString() } }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateNextStep(long ID, int NextStep)
        {
            return Json(SellersHandler.UpdateNextStep(ID, NextStep), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteAllActivities()
        {
            AccountActivityHandler.DeleteAllActivities(SiteUserDetails.LoggedInUser.Id);
            return RedirectToAction("Activities", "User");
        }
    }
}