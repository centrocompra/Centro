using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer.Handler;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Classes;
using BusinessLayer.Models.ViewModel;
using System.Web.Script.Serialization;

namespace Centro.Controllers
{
    public class ShopsController : FrontEndBaseController
    {
        //
        // GET: /Shop/
        [SkipAuthentication]
        public ActionResult Shop(string id)
        {
            id = Utility.HifenToSpace(id);
            ViewBag.IsShopFavorite = false;
            ViewBag.CentroUsers = SiteUserDetails.LoggedInUser != null ? SiteUserDetails : null;
            Shop shop = SellersHandler.ShopByShopName(id).Object;
            if (shop == null || shop.IsClosed)
                return RedirectToAction("Index", "Home");
            ViewBag.Shop = shop;
            ViewBag.ShopOwner = SellersHandler.ShopOwnerByShopName(id).Object;
            //List<ShopSection> shopSection = SellersHandler.ShopSectionsByShopId(shop.ShopID).List;
            ViewBag.ShopSection = SellersHandler.ShopSectionsByShopId(shop.ShopID).List;
            if (SiteUserDetails.LoggedInUser != null)
                ViewBag.UserId = SiteUserDetails.LoggedInUser.Id;
            else
                ViewBag.UserId = 0;
            int owner_shop_id = 0;
            if (SiteUserDetails.LoggedInUser != null)
            {
                Shop ownerShop = SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object;
                if (ownerShop != null)
                {
                    owner_shop_id = ownerShop.ShopID;
                }
                var fav_result = UsersHandler.CheckIsShopFavoriteByUserID(SiteUserDetails.LoggedInUser.Id, shop.ShopID).Object;
                if (fav_result != null)
                {
                    ViewBag.IsShopFavorite = true;
                }

            }
            
            ViewBag.ShopAvailablity = SellersHandler.GetShopAvailablity(shop.ShopID).List;


            return View(ProductsHandler.ProductsByShopPaging(shop.ShopID, null, 1, 12, "CreatedOn", "asc", null, owner_shop_id));
        }

        [SkipAuthentication]
        [AjaxOnly]
        public JsonResult ProductsByShopPaging(int shop_id, int? shop_section_id, int page_no, int per_page_result, string sortColumn, string sortOrder, bool showProductTemplate, int? inactive)
        {
            int owner_shop_id = SiteUserDetails.LoggedInUser != null ? SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object.ShopID : 0;
            var list = ProductsHandler.ProductsByShopPaging(shop_id, shop_section_id, page_no, per_page_result, "CreatedOn", "asc", inactive, owner_shop_id);
            if (showProductTemplate)
            {
                string data = RenderRazorViewToString("_ShopProducts", list);
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
            else
            {
                ViewBag.ShowTableFormat = true;
                ViewBag.ShopID = shop_id;
                ViewBag.SelectedTab = inactive.Value == 1 ? SelectedProductListingTab.Inactive : SelectedProductListingTab.Active;
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
        }

        [SkipAuthentication]
        public ActionResult Services(string ServiceName)
        {
            ServiceName = Utility.HifenToSpace(ServiceName);
            ViewBag.CentroUsers = SiteUserDetails.LoggedInUser != null ? SiteUserDetails : null;
            int UserId = SiteUserDetails.LoggedInUser != null ? SiteUserDetails.LoggedInUser.Id : 0;
            int Id = (int)Enum.GetValues(typeof(Services)).Cast<Services>().Where(v => v.ToEnumDescription().ToLower() == ServiceName.ToLower()).Select(v => v).FirstOrDefault();
            ViewBag.ServiceID = Id;
            ViewBag.ServiceName = ServiceName;
            PagingResult<GetShopListingByServiceId_Result> listing = SellersHandler.GetShopListingByServiceID(Id, 1, 8, "CreatedOn", "Desc");
            if (UserId != 0)
            {
                var list = listing.List.Where(x => x.UserId != UserId).ToList();
                int counter = listing.List.Where(x => x.UserId == UserId).Count();
                listing.TotalCount = listing.TotalCount - counter;
                listing.List = list;
            }
            return View("ShopByServices", listing);
        }

        [SkipAuthentication]
        [AjaxOnly]
        public JsonResult ShopsByServicesPaging(int service_id, int page_no, int per_page_result, string sortColumn, string sortOrder)
        {
            var list = SellersHandler.GetShopListingByServiceID(service_id, page_no, per_page_result, sortColumn, sortOrder);
            int UserId = SiteUserDetails.LoggedInUser != null ? SiteUserDetails.LoggedInUser.Id : 0;
            if (UserId != 0)
            {
                var tempList = list.List.Where(x => x.UserId != UserId).ToList();
                int counter = list.List.Where(x => x.UserId == UserId).Count();
                list.TotalCount = list.TotalCount - counter;
                list.List = tempList;
            }
            string data = RenderRazorViewToString("_ShopsListing", list);
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
        public ActionResult SendCustomRequest(string id, long? request_id)
        {
            List<SelectListItem> CategoryList;
            id = Utility.HifenToSpace(id);
            Shop shop = SellersHandler.ShopByShopName(id).Object;
            if (SiteUserDetails.LoggedInUser == null)
            {
                CreateCustomCookie(Cookies.ReturnUrlCookie, false, "/Shops/SendCustomRequest/" + Utility.SpacesToHifen(shop.ShopName), 20);
                return RedirectToAction("Signin", "Home");
            }
            else if (shop.UserId == SiteUserDetails.LoggedInUser.Id)
            {
                return RedirectToAction("Home", "User");
            }
            // Check if contract is already present between buyer and seller
            var existingContract= SellersHandler.GetCountractByBuyerAndSeller(SiteUserDetails.LoggedInUser.Id, shop.UserId).Object;
            if (existingContract != null)// return to existing contract page
                return RedirectToAction("BuyerCustomOrder", "Message", new { id = existingContract.RequestID });

            ViewBag.ShopOwner = SellersHandler.ShopOwnerByShopName(shop.ShopName).Object;
            ViewBag.Shop = shop;
            PrototypeRequest model = new PrototypeRequest();

            if (request_id.HasValue)
            {
                model = SellersHandler.GetRequestByRequestId(request_id.Value, SiteUserDetails.LoggedInUser.Id).Object;
                CategoryList = Enum.GetValues(typeof(Services)).Cast<Services>().Select(v => new SelectListItem
                {
                    Text = v.ToEnumDescription(),
                    Value = Convert.ToInt32(v).ToString(),
                    Selected = Convert.ToInt32(v) == model.CategoryId ? true : false
                }).ToList();
                if (model != null && model.RequestStatus != (int)CustomRequestStatus.Draft)
                {
                    model = new PrototypeRequest();
                    model.ShopId = shop.ShopID;
                    model.SellerId = shop.UserId;
                    ViewBag.CategoryList = CategoryList;
                    return View("CustomRequest", model);
                }
                List<RequestAttachment> attachment_list = SellersHandler.GetRequestAttachments(request_id.Value).List;
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

                model.ShopId = shop.ShopID;
                model.SellerId = shop.UserId;
                CategoryList = Enum.GetValues(typeof(Services)).Cast<Services>().Select(v => new SelectListItem
                {
                    Text = v.ToEnumDescription(),
                    Value = Convert.ToInt32(v).ToString()
                }).ToList();
            }
            ViewBag.CategoryList = CategoryList;
            return View("CustomRequest", model);
        }

        [HttpPost]
        public JsonResult SendCustomRequest(PrototypeRequest obj)
        {
            obj.BuyerId = SiteUserDetails.LoggedInUser.Id;
            HttpCookie file_temp_cookie = Request.Cookies[Cookies.TempFileAttachments];
            var output = SellersHandler.SendOrUpdateCustomRequest(obj, SiteUserDetails.LoggedInUser.Id);
            
            // log alert into database
            AccountActivityHandler.SaveAlert(new Alert
            {
                AlertForID = output.ID,
                AlertLink = "/Message/ViewRequest/" + output.ID,
                AlertText = "You have received a custom order from " + SiteUserDetails.LoggedInUser.Username,
                UserID = obj.SellerId.Value
            });
            
            if (file_temp_cookie != null)
            {
                RequestAttachmentsTempFile temp_file = new JavaScriptSerializer().Deserialize<RequestAttachmentsTempFile>(file_temp_cookie.Value);
                // Saving Product files into database
                User RequestCreatedBy = SellersHandler.RequestCreatedBy(Convert.ToInt64(output.Results[0])).Object;
                SellersHandler.UpdateRequestAttachments(SiteUserDetails.LoggedInUser.Username, temp_file.RequestAttachments, Convert.ToInt64(output.Results[0]), RequestCreatedBy.UserName);
                // Remove temp cookie
                file_temp_cookie.Expires = DateTime.Now.AddDays(-30);
                Response.Cookies.Add(file_temp_cookie);
                // Delete all files from temp folder
                System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(Server.MapPath("~/Temp/RequestAttachments/" + SiteUserDetails.LoggedInUser.Username + "/"));
                folder.Empty();
            }
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public JsonResult UploadRequestAttachment(HttpPostedFileBase requestAttachment)
        {
            if (requestAttachment.ContentLength > 10240000)
            {
                if ((Request.Browser).Browser == "IE")
                    return Json(new ActionOutput { Status = ActionStatus.Error, Message = "File size must be less than 10 MB" }, "text/plain", JsonRequestBehavior.AllowGet);
                else
                    return Json(new ActionOutput { Status = ActionStatus.Error, Message = "File size must be less than 10 MB" }, JsonRequestBehavior.AllowGet);
            }
            string newFilename = Guid.NewGuid().ToString() + "." + requestAttachment.FileName.Substring(requestAttachment.FileName.LastIndexOf(".") + 1);
            string fileExt = System.IO.Path.GetExtension(requestAttachment.FileName).ToLower();
            ActionOutput result;
            // Saving Image to Temp Folder
            string temp_folder = Server.MapPath("~/Temp/RequestAttachments/" + SiteUserDetails.LoggedInUser.Username + "/");
            if (!System.IO.Directory.Exists(temp_folder))
                System.IO.Directory.CreateDirectory(temp_folder);
            requestAttachment.SaveAs(temp_folder + newFilename);
            #region Temp Cookie
            // Check if cookie already exists
            RequestAttachmentsTempFile files;
            FileAttachmentViewModel temp_file;
            HttpCookie attachment_temp_cookie = Request.Cookies[Cookies.TempFileAttachments];
            if (attachment_temp_cookie == null)
            {
                files = new RequestAttachmentsTempFile();
                temp_file = new FileAttachmentViewModel();
                temp_file.DisplayName = requestAttachment.FileName;
                temp_file.MimeType = requestAttachment.ContentType;
                temp_file.SavedName = newFilename;
                temp_file.SizeInBytes = requestAttachment.ContentLength;
                temp_file.SizeInKB = requestAttachment.ContentLength / 1024;
                temp_file.SizeInMB = requestAttachment.ContentLength / (1024 * 1024);
                files.RequestAttachments = new List<FileAttachmentViewModel>();
                files.RequestAttachments.Add(temp_file);
                // Creating Temp cookie for User product file
                CreateCustomCookie(Cookies.TempFileAttachments, false, new JavaScriptSerializer().Serialize(files));
            }
            else
            {
                // update existing temp cookie
                files = new JavaScriptSerializer().Deserialize<RequestAttachmentsTempFile>(attachment_temp_cookie.Value);
                files.RequestAttachments.Add(new FileAttachmentViewModel
                {
                    DisplayName = requestAttachment.FileName,
                    MimeType = requestAttachment.ContentType,
                    SavedName = newFilename,
                    SizeInBytes = requestAttachment.ContentLength,
                    SizeInKB = requestAttachment.ContentLength / 1024,
                    SizeInMB = requestAttachment.ContentLength / (1024 * 1024)
                });
                UpdateCustomCookie(Cookies.TempFileAttachments, false, new JavaScriptSerializer().Serialize(files));
            }
            #endregion
            result = new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { newFilename, requestAttachment.FileName } };

            if ((Request.Browser).Browser == "IE")
                return Json(result, "text/plain", JsonRequestBehavior.AllowGet);
            else
                return Json(result, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public JsonResult DeleteTempFileAttachment(string filename)
        {
            // delete from disk
            Utility.DeleteFile("~/Temp/RequestAttachments/" + SiteUserDetails.LoggedInUser.Username + "/" + filename);
            // delete from cookie
            HttpCookie user_temp_cookie = Request.Cookies[Cookies.TempFileAttachments];
            if (user_temp_cookie != null)
            {
                RequestAttachmentsTempFile temp_files = new JavaScriptSerializer().Deserialize<RequestAttachmentsTempFile>(user_temp_cookie.Value);
                //filename = filename.Split(new char[] { '/' })[3];
                FileAttachmentViewModel pic = temp_files.RequestAttachments.Where(m => m.SavedName == filename).FirstOrDefault();
                temp_files.RequestAttachments.Remove(pic);
                // Creating Temp cookie for User product pics
                UpdateCustomCookie(Cookies.TempFileAttachments, false, new JavaScriptSerializer().Serialize(temp_files));
            }
            return Json(new ActionOutput { Status = ActionStatus.Successfull }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ManageRequests()
        {
            Shop shop = SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object;
            var data = new PagingResult<GetRequestListingByBuyerOrSellerID_Result>();
            if (shop == null || shop.IsClosed)
            {
                ViewBag.MyRequest = false;
                ViewBag.Buyer = "true";
                data = SellersHandler.GetRequestListingByBuyerOrSellerIDPaging(SiteUserDetails.LoggedInUser.Id, null, 1, 12, "UpdatedOn", "desc");

            }
            else
            {
                ViewBag.MyRequest = true;
                ViewBag.Buyer = "false";
                data = SellersHandler.GetRequestListingByBuyerOrSellerIDPaging(null, SiteUserDetails.LoggedInUser.Id, 1, 12, "UpdatedOn", "desc");
            }
            ViewBag.Draft = "false";


            return View(data);
        }

        [AjaxOnly]
        public PartialViewResult _RequestSent(bool draft = false)
        {
            ViewBag.Buyer = "true";
            PagingResult<GetRequestListingByBuyerOrSellerID_Result> data = new PagingResult<GetRequestListingByBuyerOrSellerID_Result>();

            if (draft)
            {
                ViewBag.Draft = "true";
                data = SellersHandler.GetRequestListingByBuyerOrSellerIDPaging(SiteUserDetails.LoggedInUser.Id, null, 1, 12, "UpdatedOn", "desc", true);
            }
            else
            {
                ViewBag.Draft = "false";
                data = SellersHandler.GetRequestListingByBuyerOrSellerIDPaging(SiteUserDetails.LoggedInUser.Id, null, 1, 12, "UpdatedOn", "desc");
            }
            return PartialView(data);
        }

        [AjaxOnly]
        public PartialViewResult _RequestRecieved()
        {
            ViewBag.Buyer = "false";
            ViewBag.Draft = "false";
            return PartialView(SellersHandler.GetRequestListingByBuyerOrSellerIDPaging(null, SiteUserDetails.LoggedInUser.Id, 1, 12, "UpdatedOn", "desc"));
        }

        [HttpPost]
        [AjaxOnly]
        public JsonResult RequestListingByBuyerOrSellerIDPaging(int page_no, int per_page_result, string sortColumn, string sortOrder, string buyer, string draft)
        {
            var list = new PagingResult<GetRequestListingByBuyerOrSellerID_Result>();
            String data = "";
            if (buyer == "true")
            {
                ViewBag.Buyer = "true";
                if (draft == "true")
                {
                    ViewBag.Draft = "true";
                    list = SellersHandler.GetRequestListingByBuyerOrSellerIDPaging(SiteUserDetails.LoggedInUser.Id, null, page_no, per_page_result, sortColumn, sortOrder, true);

                }
                else
                {
                    ViewBag.Draft = "false";
                    list = SellersHandler.GetRequestListingByBuyerOrSellerIDPaging(SiteUserDetails.LoggedInUser.Id, null, page_no, per_page_result, sortColumn, sortOrder);

                }
                data = RenderRazorViewToString("_RequestSent", list);
            }
            else
            {
                ViewBag.Buyer = "false";
                ViewBag.Draft = "false";
                list = SellersHandler.GetRequestListingByBuyerOrSellerIDPaging(null, SiteUserDetails.LoggedInUser.Id, page_no, per_page_result, sortColumn, sortOrder);
                data = RenderRazorViewToString("_RequestRecieved", list);
            }


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


        public ActionResult ViewRequest(long id)
        {
            SellersHandler.MarkRequestAsReadUnread(SiteUserDetails.LoggedInUser.Id, id, true);
            GetRequestInfoByRequestId_Result request = SellersHandler.GetRequestInformation(id);
            if (request.SellerId != SiteUserDetails.LoggedInUser.Id)
                return RedirectToAction("ManageRequests");
            List<RequestAttachment> attachment_list = SellersHandler.GetRequestAttachments(id).List;
            ViewBag.Request = request;
            ViewBag.Attachments = attachment_list;
            return View();
        }

        public ActionResult DownloadFile(int attachment_id, long request_id)
        {
            GetRequestInfoByRequestId_Result request = SellersHandler.GetRequestInformation(request_id);

            if (request == null)
                return RedirectToAction("ManageRequests", "Shops");

            // Fetch Attachment file
            RequestAttachment file = SellersHandler.GetRequestAttachments(request_id).List.Where(o => o.AttachmentID == attachment_id).FirstOrDefault();
            byte[] file_bytes = Utility.ReadFileBytes("~/Images/Attachments/" + request.BuyerUserName + "/" + file.SavedName);

            return File(file_bytes, file.MimeType, file.DisplayName);
        }

        [HttpPost]
        public JsonResult UpdateRequestStatus(long request_id, CustomRequestStatus request_status)
        {
            var result = SellersHandler.UpdateRequestStatus(request_id, request_status, SiteUserDetails.LoggedInUser.Id, SiteUserDetails.LoggedInUser.Username);
            
            return Json(new ActionOutput { Status = result.Status, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteRequest(long request_id)
        {
            var result = SellersHandler.DeleteRequest(request_id, SiteUserDetails.LoggedInUser.Id);
            return Json(new ActionOutput { Status = result.Status, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult MyCustomOrder(long id)
        {
            SellersHandler.MarkRequestAsReadUnread(SiteUserDetails.LoggedInUser.Id, id, true);
            GetRequestInfoByRequestId_Result request = SellersHandler.GetRequestInformation(id);
            if (request.SellerId != SiteUserDetails.LoggedInUser.Id)
                return RedirectToAction("ManageRequests");
            List<RequestAttachment> attachment_list = SellersHandler.GetRequestAttachments(id).List;
            if (request.RequestStatus != (int)CustomRequestStatus.Working && request.RequestStatus != (int)CustomRequestStatus.OnHold && request.RequestStatus != (int)CustomRequestStatus.Completed && request.RequestStatus != (int)CustomRequestStatus.WaitingToStart)
            {
                request.RequestStatus = (int)CustomRequestStatus.Working;
            }
            List<SelectListItem> list = Enum.GetValues(typeof(CustomRequestStatus)).Cast<CustomRequestStatus>().Where(z => z.Equals(CustomRequestStatus.Working) || z.Equals(CustomRequestStatus.OnHold) || z.Equals(CustomRequestStatus.Completed) || z.Equals(CustomRequestStatus.WaitingToStart)).Select(v => new SelectListItem
            {
                Text = v.ToEnumDescription(),
                Value = Convert.ToInt32(v).ToString(),
                Selected = Convert.ToInt32(v) == request.RequestStatus.Value ? true : false
            }).ToList();

            HttpCookie file_temp_cookie = Request.Cookies[Cookies.TempFileAttachments];
            if (file_temp_cookie != null)
            {
                file_temp_cookie.Expires = DateTime.Now.AddDays(-30);
                Response.Cookies.Add(file_temp_cookie);
                System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(Server.MapPath("~/Temp/RequestAttachments/" + SiteUserDetails.LoggedInUser.Username + "/"));
                folder.Empty();
            }
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
            }
            ViewBag.Request = request;
            ViewBag.Attachments = attachment_list;
            ViewBag.StatusList = list;
            ViewBag.Invoices = InvoiceHandler.GetInvoiceByRequestId(id).List;
            return View();
        }

        public ActionResult BuyerCustomOrder(long id)
        {
            GetRequestInfoByRequestId_Result request = SellersHandler.GetRequestInformation(id);
            if (request.BuyerId != SiteUserDetails.LoggedInUser.Id)
                return RedirectToAction("ManageRequests");
            List<RequestAttachment> attachment_list = SellersHandler.GetRequestAttachments(id).List;
            if (request.RequestStatus != (int)CustomRequestStatus.Working && request.RequestStatus != (int)CustomRequestStatus.OnHold && request.RequestStatus != (int)CustomRequestStatus.Completed && request.RequestStatus != (int)CustomRequestStatus.WaitingToStart)
            {
                request.RequestStatus = (int)CustomRequestStatus.Working;
            }
            List<SelectListItem> list = Enum.GetValues(typeof(CustomRequestStatus)).Cast<CustomRequestStatus>().Where(z => z.Equals(CustomRequestStatus.Working) || z.Equals(CustomRequestStatus.OnHold) || z.Equals(CustomRequestStatus.Completed) || z.Equals(CustomRequestStatus.WaitingToStart)).Select(v => new SelectListItem
            {
                Text = v.ToEnumDescription(),
                Value = Convert.ToInt32(v).ToString(),
                Selected = Convert.ToInt32(v) == request.RequestStatus.Value ? true : false
            }).ToList();

            HttpCookie file_temp_cookie = Request.Cookies[Cookies.TempFileAttachments];
            if (file_temp_cookie != null)
            {
                file_temp_cookie.Expires = DateTime.Now.AddDays(-30);
                Response.Cookies.Add(file_temp_cookie);
                System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(Server.MapPath("~/Temp/RequestAttachments/" + SiteUserDetails.LoggedInUser.Username + "/"));
                folder.Empty();
            }
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
            }
            ViewBag.Request = request;
            ViewBag.Attachments = attachment_list;
            ViewBag.StatusList = list;
            ViewBag.Invoices = InvoiceHandler.GetInvoiceByRequestId(id).List;
            return View();
        }

        [HttpPost]
        public JsonResult UpdateCustomOrderRequest()
        {
            HttpCookie file_temp_cookie = Request.Cookies[Cookies.TempFileAttachments];
            long request_id = Convert.ToInt64(Request["RequestID"]);
            CustomRequestStatus request_status = (CustomRequestStatus)(Convert.ToInt32(Request["RequestStatus"]));
            var result = SellersHandler.UpdateRequestStatus(request_id, request_status, SiteUserDetails.LoggedInUser.Id, SiteUserDetails.LoggedInUser.Username);
            if (file_temp_cookie != null)
            {
                RequestAttachmentsTempFile temp_file = new JavaScriptSerializer().Deserialize<RequestAttachmentsTempFile>(file_temp_cookie.Value);
                User RequestCreatedBy = SellersHandler.RequestCreatedBy(request_id).Object;
                SellersHandler.UpdateRequestAttachments(SiteUserDetails.LoggedInUser.Username, temp_file.RequestAttachments, request_id, RequestCreatedBy.UserName);

                file_temp_cookie.Expires = DateTime.Now.AddDays(-30);
                Response.Cookies.Add(file_temp_cookie);

                System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(Server.MapPath("~/Temp/RequestAttachments/" + SiteUserDetails.LoggedInUser.Username + "/"));
                folder.Empty();
            }

            return Json(new ActionOutput { Status = result.Status, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ManageOrders(string id)
        {
            PagingResult<OrderViewModel> model;
            if (id.ToLower() == "s")
            {
                Shop shop = SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object;
                ViewBag.Status = (int)OrderStatus.Pending;
                if (shop == null || shop.IsClosed)
                {
                    model = new PagingResult<OrderViewModel> { List = new List<OrderViewModel>(), Message = "No Order Found", Status = ActionStatus.Successfull };
                    //return RedirectToAction("ManageOrders", "Shops", new { id = id });
                }
                else
                    model = SellersHandler.GetSellerOrderViewModel(shop.ShopID, OrderStatus.Pending, 1, 20, id);
                ViewBag.Id = shop.ShopID;
            }
            else
            {
                User buyer = UsersHandler.GetUserByID(SiteUserDetails.LoggedInUser.Id).Object;
                ViewBag.Status = (int)OrderStatus.Pending;
                model = SellersHandler.GetBuyerOrderViewModel(buyer.UserID, OrderStatus.Pending, 1, 20, id);
                ViewBag.Id = buyer.UserID;
            }
            ViewBag.LoggedInUsername = SiteUserDetails.LoggedInUser.Username;
            ViewBag.Type = id;
            return View(model);
        }

        public JsonResult CancelOrder(int OrderId)
        {
            return Json(SellersHandler.CancelOrder(OrderId), JsonRequestBehavior.AllowGet);
        }

        [SkipAuthentication]
        public ActionResult ShopAvailability(string ShopName,int ShopID)
        {
            ShopName = Utility.HifenToSpace(ShopName);
            ViewBag.IsShopFavorite = false;
            ViewBag.CentroUsers = SiteUserDetails.LoggedInUser != null ? SiteUserDetails : null;
            Shop shop = SellersHandler.ShopByShopId(ShopID).Object;
            ViewBag.Shop = shop;
            if (shop == null || shop.IsClosed)
                return RedirectToAction("Index", "Home");
            //ViewBag.Shop = shop;
            ViewBag.ShopOwner = SellersHandler.ShopOwnerByShopId(ShopID).Object;
            //List<ShopSection> shopSection = SellersHandler.ShopSectionsByShopId(shop.ShopID).List;
            ViewBag.ShopSection = SellersHandler.ShopSectionsByShopId(shop.ShopID).List;
            if (SiteUserDetails.LoggedInUser != null)
                ViewBag.UserId = SiteUserDetails.LoggedInUser.Id;
            else
                ViewBag.UserId = 0;
            int owner_shop_id = 0;
            if (SiteUserDetails.LoggedInUser != null)
            {
                Shop ownerShop = SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object;
                if (ownerShop != null)
                {
                    owner_shop_id = ownerShop.ShopID;
                }
                var fav_result = UsersHandler.CheckIsShopFavoriteByUserID(SiteUserDetails.LoggedInUser.Id, shop.ShopID).Object;
                if (fav_result != null)
                {
                    ViewBag.IsShopFavorite = true;
                }

            }

            ViewBag.ShopAvailablity = SellersHandler.GetShopAvailablity(shop.ShopID).List;
            return View(shop);
        }

        [AjaxOnly]
        public PartialViewResult _OrderListing(int id, int status, string type)
        {
            ViewBag.LoggedInUsername = SiteUserDetails.LoggedInUser.Username;
            var list = new PagingResult<OrderViewModel>();
            if (type.ToLower() == "s")
                list = SellersHandler.GetSellerOrderViewModel(id, (OrderStatus)status, 1, 20, type);
            else
                list = SellersHandler.GetBuyerOrderViewModel(id, (OrderStatus)status, 1, 20, type);
            ViewBag.Type = type;
            return PartialView(list);
        }

        [HttpPost]
        [AjaxOnly]
        public JsonResult OrderListingPaging(int page_no, int per_page_result, string sortColumn, string sortOrder, OrderStatus orderStatus, int id, string type)
        {
            ViewBag.Type = type;
            var list = new PagingResult<OrderViewModel>();
            String data = "";
            ViewBag.Status = (int)orderStatus;
            ViewBag.Id = id;
            ViewBag.Type = type;
            if (type.ToLower() == "s")
                list = SellersHandler.GetSellerOrderViewModel(id, orderStatus, page_no, per_page_result, type);
            else
                list = SellersHandler.GetBuyerOrderViewModel(id, orderStatus, page_no, per_page_result, type);
            data = RenderRazorViewToString("_OrderListing", list);
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Message = "",
                Results = new List<string> 
                { 
                    data, list.TotalCount.ToString() 
                }
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult OrderDetail(int id, string type)
        {
            ViewBag.Type = type;
            ViewBag.LoggedInUserID = SiteUserDetails.LoggedInUser.Id;
            var model = SellersHandler.OrderById(id).Object;
            ViewBag.Buyer = UsersHandler.GetUserByID(model.UserID).Object;
            ViewBag.ShippingDetails = model.ShippingAddressId.HasValue ? UsersHandler.GetShippingAddressById(model.ShippingAddressId.Value).Object : null;
            ViewBag.BillingDetails = UsersHandler.GetBillingAddressByID(model.UserID, model.BillingAddressId);
            if(model.OrderStatusId==(int)OrderStatus.Pending)
            ViewBag.OrderStatusList = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>()
                                                                        //.Where(m => m != OrderStatus.Canceled)
                                                                        .Select(v => new SelectListItem
                                                                        {
                                                                            Text = v.ToEnumWordify(),
                                                                            Value = Convert.ToInt32(v).ToString(),
                                                                            Selected = Convert.ToInt32(v) == model.OrderStatusId ? true : false
                                                                        }).ToList();
            else
            ViewBag.OrderStatusList = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>()
                                                                        .Where(m => m != OrderStatus.Canceled)
                                                                        .Select(v => new SelectListItem
                                                                        {
                                                                            Text = v.ToEnumWordify(),
                                                                            Value = Convert.ToInt32(v).ToString(),
                                                                            Selected = Convert.ToInt32(v) == model.OrderStatusId ? true : false
                                                                        }).ToList();

            ViewBag.ShippingStatusList = Enum.GetValues(typeof(ShippingStatus)).Cast<ShippingStatus>().Select(v => new SelectListItem
                                                                                                        {
                                                                                                            Text = v.ToEnumWordify(),
                                                                                                            Value = Convert.ToInt32(v).ToString(),
                                                                                                            Selected = Convert.ToInt32(v) == model.ShippingStatusId ? true : false
                                                                                                        }).ToList();
            return View(model);
        }

        public JsonResult UpdateOrder(int OrderID, int OrderStatus, int ShippingStatus, string TrackingID)
        {
            return Json(SellersHandler.UpdateOrder(OrderID, OrderStatus, ShippingStatus, TrackingID), JsonRequestBehavior.AllowGet);
        }

        [SkipAuthentication]
        public ActionResult Hubs(string id)
        {
            id = Utility.HifenToSpace(id);
            ViewBag.CentroUsers = SiteUserDetails.LoggedInUser != null ? SiteUserDetails : null;
            Shop shop = SellersHandler.ShopByShopName(id).Object;
            if (shop == null || shop.IsClosed)
                return RedirectToAction("Index", "Home");
            ViewBag.Shop = shop;
            ViewBag.ShopOwner = SellersHandler.ShopOwnerByShopName(id).Object;
            //List<ShopSection> shopSection = SellersHandler.ShopSectionsByShopId(shop.ShopID).List;
            ViewBag.ShopSection = SellersHandler.ShopSectionsByShopId(shop.ShopID).List;
            if (SiteUserDetails.LoggedInUser != null)
                ViewBag.UserId = SiteUserDetails.LoggedInUser.Id;
            else
                ViewBag.UserId = 0;
            int owner_shop_id = 0;
            if (SiteUserDetails.LoggedInUser != null)
            {
                Shop ownerShop = SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object;
                if (ownerShop != null)
                {
                    owner_shop_id = ownerShop.ShopID;
                }

            }
            ViewBag.ShopAvailablity = SellersHandler.GetShopAvailablity(shop.ShopID).List;

            return View(HubHandler.GetRandomHubs(12).List);
        }
    }
}
