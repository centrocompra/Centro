using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer.Classes;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Handler;
using System.Web.Script.Serialization;

namespace Centro.Controllers
{
    [AuthenticateUser]
    public class MessageController : FrontEndBaseController
    {
        private void ClearTempAttachmentCookie()
        {
            HttpCookie file_temp_cookie = Request.Cookies[Cookies.TempAttachments];
            if (file_temp_cookie != null)
            {
                // Remove temp cookie
                file_temp_cookie.Expires = DateTime.Now.AddDays(-30);
                Response.Cookies.Add(file_temp_cookie);
                // Delete all files from temp folder
                System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(Server.MapPath("~/Temp/Attachments/" + SiteUserDetails.LoggedInUser.Username + "/"));
                folder.Empty();
            }
        }

        public ActionResult Index()
        {
            //PagingResult<MessagesListing_Result> model = MessageHandler.GetMessageListing(1, 20, "Desc", "CreatedOn", "", CentroUsers.LoggedInUser.Id, "Inbox", null, false);
            PagingResult<MessagesListing_Result> model = new PagingResult<MessagesListing_Result>();
            model.List = null;
            model.TotalCount = 0;
            return View(model);
        }

        public ActionResult Compose(string id)
        {
            ClearTempAttachmentCookie();
            Message model = new Message();
            if (!string.IsNullOrEmpty(id))
                model.Receiver = id + ";";
            return View(model);
        }

        [HttpPost]
        public JsonResult Compose(Message model)
        {
            model.Receivers = Request["Receiver"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            HttpCookie file_temp_cookie = Request.Cookies[Cookies.TempAttachments];
            model.AuthorID = SiteUserDetails.LoggedInUser.Id;
            var output = MessageHandler.PostMessage(model);
            if (file_temp_cookie != null && output.Status == ActionStatus.Successfull)
            {
                ProductFiles temp_file = new JavaScriptSerializer().Deserialize<ProductFiles>(file_temp_cookie.Value);
                // Saving Product files into database
                MessageHandler.AddAttachments(SiteUserDetails.LoggedInUser.Username, temp_file.ProductTempFiles, output.ID);
                // Remove temp cookie
                file_temp_cookie.Expires = DateTime.Now.AddDays(-30);
                Response.Cookies.Add(file_temp_cookie);
                // Delete all files from temp folder
                System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(Server.MapPath("~/Temp/Attachments/" + SiteUserDetails.LoggedInUser.Username + "/"));
                folder.Empty();
            }
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public JsonResult _Message(int page_no, int per_page_result, string sortOrder, string sortColumn, string search, string message_type, bool? IsRead, bool IsArchived)
        {
            int place_holder = (int)MessagePlaceHolder.Inbox;
            if (message_type == "Archived") place_holder = (int)MessagePlaceHolder.Archive;
            else if (message_type == "Sent") place_holder = (int)MessagePlaceHolder.Sent;
            ViewBag.PlaceHolder = place_holder;
            PagingResult<MessagesListing_Result> model = MessageHandler.GetMessageListing(page_no, per_page_result, sortOrder, sortColumn, search, SiteUserDetails.LoggedInUser.Id, place_holder, IsRead, IsArchived);
            ViewBag.MessageType = place_holder;
            string view = RenderRazorViewToString("_Messages", model);
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string> { view, model.TotalCount.ToString(), model.Message }
            }, JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public JsonResult _Usernames(string startswith)
        {
            string[] except = startswith.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            startswith = startswith.IndexOf(';') > 0 ? startswith.Substring(startswith.LastIndexOf(';') + 1).Trim() : startswith.Trim();
            List<string> users;
            if (startswith.Trim().Length > 0)
                users = UsersHandler.UsersStartsWith(startswith, except).List;
            else
                users = new List<string>();
            string json = new JavaScriptSerializer().Serialize(users);
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string> { json },
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteTempAttachment(string filename)
        {
            // delete from disk
            Utility.DeleteFile("~/Temp/Attachments/" + SiteUserDetails.LoggedInUser.Username + "/" + filename);
            // delete from cookie
            HttpCookie user_temp_cookie = Request.Cookies[Cookies.TempAttachments];
            if (user_temp_cookie != null)
            {
                ProductFiles temp_files = new JavaScriptSerializer().Deserialize<ProductFiles>(user_temp_cookie.Value);
                //filename = filename.Split(new char[] { '/' })[3];
                ProductFileViewModel pic = temp_files.ProductTempFiles.Where(m => m.SavedName == filename).FirstOrDefault();
                temp_files.ProductTempFiles.Remove(pic);
                // Creating Temp cookie for User product pics
                UpdateCustomCookie(Cookies.TempAttachments, false, new JavaScriptSerializer().Serialize(temp_files));
            }
            return Json(new ActionOutput { Status = ActionStatus.Successfull }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UploadAttachments(HttpPostedFileBase Attachments)
        {
            string count = Request["Count"];

            if (Attachments.ContentLength > 10240000)
            {
                if ((Request.Browser).Browser == "IE")
                    return Json(new ActionOutput { Status = ActionStatus.Error, Message = "File size must be less than 10 MB", Results = new List<string> { "", "", count } }, "text/plain", JsonRequestBehavior.AllowGet);
                else
                    return Json(new ActionOutput { Status = ActionStatus.Error, Message = "File size must be less than 10 MB", Results = new List<string> { "", "", count } }, JsonRequestBehavior.AllowGet);
            }
            string newFilename = Guid.NewGuid().ToString() + "." + Attachments.FileName.Substring(Attachments.FileName.LastIndexOf(".") + 1);
            string fileExt = System.IO.Path.GetExtension(Attachments.FileName).ToLower();
            ActionOutput result;

            // Saving Attachments to Temp Folder
            string temp_folder = Server.MapPath("~/Temp/Attachments/" + SiteUserDetails.LoggedInUser.Username + "/");
            if (!System.IO.Directory.Exists(temp_folder))
                System.IO.Directory.CreateDirectory(temp_folder);
            Attachments.SaveAs(temp_folder + newFilename);
            #region Temp Cookie
            // Check if cookie already exists
            ProductFiles files;
            ProductFileViewModel temp_file;
            HttpCookie file_temp_cookie = Request.Cookies[Cookies.TempAttachments];
            if (file_temp_cookie == null)
            {
                files = new ProductFiles();
                temp_file = new ProductFileViewModel();
                temp_file.DisplayName = Attachments.FileName;
                temp_file.MimeType = Attachments.ContentType;
                temp_file.SavedName = newFilename;
                temp_file.SizeInBytes = Attachments.ContentLength;
                temp_file.SizeInKB = Attachments.ContentLength / 1024;
                temp_file.SizeInMB = Attachments.ContentLength / (1024 * 1024);
                files.ProductTempFiles = new List<ProductFileViewModel>();
                files.ProductTempFiles.Add(temp_file);
                // Creating Temp cookie for User product file
                CreateCustomCookie(Cookies.TempAttachments, false, new JavaScriptSerializer().Serialize(files));
            }
            else
            {
                // update existing temp cookie
                files = new JavaScriptSerializer().Deserialize<ProductFiles>(file_temp_cookie.Value);
                files.ProductTempFiles.Add(new ProductFileViewModel
                {
                    DisplayName = Attachments.FileName,
                    MimeType = Attachments.ContentType,
                    SavedName = newFilename,
                    SizeInBytes = Attachments.ContentLength,
                    SizeInKB = Attachments.ContentLength / 1024,
                    SizeInMB = Attachments.ContentLength / (1024 * 1024)
                });
                UpdateCustomCookie(Cookies.TempAttachments, false, new JavaScriptSerializer().Serialize(files));
            }
            #endregion
            result = new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { newFilename, Attachments.FileName, count } };

            if ((Request.Browser).Browser == "IE")
                return Json(result, "text/plain", JsonRequestBehavior.AllowGet);
            else
                return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MyMessage(int id)
        {
            ClearTempAttachmentCookie();
            MessageHandler.MarkAsReadUnread(SiteUserDetails.LoggedInUser.Id, id, true);
            int? msg_id = MessageHandler.ReplyMessageIdByMessageId(id).Object;
            var model = MessageHandler.MessageById(id, msg_id).List;
            ViewBag.UsersMessages = MessageHandler.UsersMessagesByMessageId(id).List;
            ViewBag.MessageID = msg_id.HasValue ? msg_id.Value : id;
            ViewBag.LoggedInUserID = SiteUserDetails.LoggedInUser.Id;
            return View(model);
        }

        [HttpPost]
        public JsonResult ReplyToAll()
        {
            Message model = new Message();
            model.AuthorID = SiteUserDetails.LoggedInUser.Id;
            model.ReplyMessageID = Convert.ToInt32(Request["MessageID"]);
            model.Subject = Request["Message1"];
            model.Body = Request["Body"];

            List<int> Receivers = MessageHandler.MessageReceiversByMessageId(Convert.ToInt32(Request["MessageID"])).List.Select(m => m.UserID).ToList();

            var output = MessageHandler.ReplyToAll(model, Receivers, SiteUserDetails.LoggedInUser.Id);

            HttpCookie file_temp_cookie = Request.Cookies[Cookies.TempAttachments];
            //MessageHandler.MarkAsReadUnread( model.MessageID, false);
            if (file_temp_cookie != null)
            {
                ProductFiles temp_file = new JavaScriptSerializer().Deserialize<ProductFiles>(file_temp_cookie.Value);
                // Saving Product files into database
                MessageHandler.AddAttachments(SiteUserDetails.LoggedInUser.Username, temp_file.ProductTempFiles, output.ID);
                // Remove temp cookie
                file_temp_cookie.Expires = DateTime.Now.AddDays(-30);
                Response.Cookies.Add(file_temp_cookie);
                // Delete all files from temp folder
                System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(Server.MapPath("~/Temp/Attachments/" + SiteUserDetails.LoggedInUser.Username + "/"));
                folder.Empty();
            }
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DownloadFile(int attachment_id, int message_id)
        {
            // Check for correct receivers/sender
            List<int> Users = MessageHandler.MessageReceiversByMessageID(message_id).List.Select(m => m.UserID).ToList();

            if (!Users.Where(m => m == SiteUserDetails.LoggedInUser.Id).Any())
                return RedirectToAction("Index", "Message");

            // Fetch Attachment file
            Attachment file = MessageHandler.AttachmentById(attachment_id).Object;
            byte[] file_bytes = Utility.ReadFileBytes("~/Images/Attachments/" + MessageHandler.MessageAuthorByMessageId(message_id).Object + "/" + file.SavedName);

            return File(file_bytes, file.MimeType, file.DisplayName);
        }

        public JsonResult DeleteMessages(string Ids)
        {
            List<int> MessageIds = Ids.Split(',').Select(i => int.Parse(i)).ToList();
            return Json(MessageHandler.DeleteMessages(MessageIds, SiteUserDetails.LoggedInUser.Id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult MarkAsArchive(string Ids)
        {
            List<int> MessageIds = Ids.Split(',').Select(i => int.Parse(i)).ToList();
            return Json(MessageHandler.MarkAsArchive(MessageIds, SiteUserDetails.LoggedInUser.Id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult TotalUnreadMessage()
        {
            return Json(MessageHandler.TotalUnreadMessage(SiteUserDetails.LoggedInUser.Id), JsonRequestBehavior.AllowGet);

        }

        public JsonResult UpdateCustomRequestTitle(long RequestID, string Title)
        {
            return Json(SellersHandler.UpdateCustomRequestTitle(RequestID, Title), JsonRequestBehavior.AllowGet);
        }

        public ActionResult MyCustomOrder(long id, string from)
        {
            ViewBag.From = from;
            SellersHandler.MarkRequestAsReadUnread(SiteUserDetails.LoggedInUser.Id, id, true);
            GetRequestInfoByRequestId_Result request = SellersHandler.GetRequestInformation(id);
            if (request.SellerId != SiteUserDetails.LoggedInUser.Id)
                //return RedirectToAction("ManageRequests");
                return RedirectToAction("EditProfile", "User");
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

        public ActionResult ViewRequest(long id)
        {
            GetRequestInfoByRequestId_Result request = SellersHandler.GetRequestInformation(id);
            if (request != null && (request.SellerId == SiteUserDetails.LoggedInUser.Id || request.BuyerId == SiteUserDetails.LoggedInUser.Id))
            {
                SellersHandler.MarkRequestAsReadUnread(SiteUserDetails.LoggedInUser.Id, id, true);
                List<RequestAttachment> attachment_list = SellersHandler.GetRequestAttachments(id).List;
                ViewBag.Request = request;
                ViewBag.Attachments = attachment_list;
                return View();
            }
            return RedirectToAction("Index", "Message");
        }

        public ActionResult BuyerCustomOrder(long id, string from)
        {
            HttpCookie temp = Request.Cookies["temp"];
            if (temp != null)
            {
                temp.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(temp);
            }
            ViewBag.From = from;
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

        //[HttpPost]
        //public JsonResult UpdateCustomOrderRequest()
        //{
        //    HttpCookie file_temp_cookie = Request.Cookies[Cookies.TempFileAttachments];
        //    long request_id = Convert.ToInt64(Request["RequestID"]);
        //    string[] hdnFiles = Convert.ToString(Request["hdnFiles"]).Split(',');

        //    CustomRequestStatus request_status = (CustomRequestStatus)(Convert.ToInt32(Request["RequestStatus"]));
        //    var result = SellersHandler.UpdateRequestStatus(request_id, request_status, CentroUsers.LoggedInUser.Id);
        //    if (file_temp_cookie != null)
        //    {
        //        RequestAttachmentsTempFile temp_file = new JavaScriptSerializer().Deserialize<RequestAttachmentsTempFile>(file_temp_cookie.Value);
        //        User RequestCreatedBy = SellersHandler.RequestCreatedBy(request_id).Object;

        //        foreach (string item in hdnFiles)
        //        {
        //            string oldF = item.Split(':')[1];
        //            string newF = item.Split(':')[0];
        //            var current=temp_file.RequestAttachments.Where(m=>m.SavedName==oldF).FirstOrDefault();
        //            if(current!=null)

        //        }

        //        SellersHandler.UpdateRequestAttachments(CentroUsers.LoggedInUser.Username, temp_file.RequestAttachments, request_id, RequestCreatedBy.UserName);

        //        file_temp_cookie.Expires = DateTime.Now.AddDays(-30);
        //        Response.Cookies.Add(file_temp_cookie);

        //        System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(Server.MapPath("~/Temp/RequestAttachments/" + CentroUsers.LoggedInUser.Username + "/"));
        //        folder.Empty();
        //    }

        //    return Json(new ActionOutput { Status = result.Status, Message = result.Message }, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public JsonResult UpdateCustomOrderRequest()
        {
            HttpCookie file_temp_cookie = Request.Cookies[Cookies.TempFileAttachments];
            long request_id = Convert.ToInt64(Request["RequestID"]);
            string file_names = Convert.ToString(Request["hdnFiles"]);


            CustomRequestStatus request_status = (CustomRequestStatus)(Convert.ToInt32(Request["RequestStatus"]));
            var result = SellersHandler.UpdateRequestStatus(request_id, request_status, SiteUserDetails.LoggedInUser.Id, SiteUserDetails.LoggedInUser.Username);
            if (file_temp_cookie != null)
            {
                RequestAttachmentsTempFile temp_file = new JavaScriptSerializer().Deserialize<RequestAttachmentsTempFile>(file_temp_cookie.Value);
                if (file_names != "")
                {
                    string[] fileName_arr = file_names.Split(',');
                    if (fileName_arr.Count() > 0)
                    {
                        for (int i = 0; i < temp_file.RequestAttachments.Count; i++)
                        {
                            for (int j = 0; j < fileName_arr.Count(); j++)
                            {
                                if (temp_file.RequestAttachments[i].SavedName == fileName_arr[j].Split(':')[1])
                                {
                                    temp_file.RequestAttachments[i].DisplayName = fileName_arr[j].Split(':')[0];
                                }

                            }
                        }
                    }
                }
                User RequestCreatedBy = SellersHandler.RequestCreatedBy(request_id).Object;
                SellersHandler.UpdateRequestAttachments(SiteUserDetails.LoggedInUser.Username, temp_file.RequestAttachments, request_id, RequestCreatedBy.UserName);

                file_temp_cookie.Expires = DateTime.Now.AddDays(-30);
                Response.Cookies.Add(file_temp_cookie);

                System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(Server.MapPath("~/Temp/RequestAttachments/" + SiteUserDetails.LoggedInUser.Username + "/"));
                folder.Empty();
            }

            return Json(new ActionOutput { Status = result.Status, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }


        [AjaxOnly]
        public ViewResult _Contract(string type)
        {
            ViewBag.Buyer = "false";
            ViewBag.Draft = "false";
            ViewBag.Type = type;
            return View(SellersHandler.GetServiceContracts(1, 12, "CreatedOn", "Desc", type, SiteUserDetails.LoggedInUser.Id));
        }

        [AjaxOnly]
        public PartialViewResult _Hired(string type)
        {
            ViewBag.Buyer = "false";
            ViewBag.Draft = "false";
            ViewBag.Type = type;
            ViewBag.AsSeller = SellersHandler.GetServiceContracts(1, 12, "CreatedOn", "Desc", "SellerID", SiteUserDetails.LoggedInUser.Id);
            ViewBag.AsBuyer = SellersHandler.GetServiceContracts(1, 12, "CreatedOn", "Desc", "BuyerID", SiteUserDetails.LoggedInUser.Id);
            return PartialView(/*SellersHandler.GetServiceContracts(1, 12, "CreatedOn", "Desc", type, CentroUsers.LoggedInUser.Id)*/);
        }

        [AjaxOnly]
        public PartialViewResult _PastHired(string type)
        {
            ViewBag.Buyer = "false";
            ViewBag.Draft = "false";
            ViewBag.Type = type;
            ViewBag.AsSeller = SellersHandler.GetPastServiceContracts(1, 12, "CreatedOn", "Desc", "SellerID", SiteUserDetails.LoggedInUser.Id);
            ViewBag.AsBuyer = SellersHandler.GetPastServiceContracts(1, 12, "CreatedOn", "Desc", "BuyerID", SiteUserDetails.LoggedInUser.Id);
            return PartialView("_Hired"/*SellersHandler.GetServiceContracts(1, 12, "CreatedOn", "Desc", type, CentroUsers.LoggedInUser.Id)*/);
        }

        [HttpPost]
        [AjaxOnly]
        public JsonResult GetServiceContracts(int page_no, int per_page_result, string sortColumn, string sortOrder, string type)
        {
            ViewBag.Type = type;
            var list = new PagingResult<ServiceContracts_Result>();
            String data = "";
            ViewBag.Buyer = "false";
            ViewBag.Draft = "false";
            list = SellersHandler.GetServiceContracts(page_no, per_page_result, "CreatedOn", "Desc", type, SiteUserDetails.LoggedInUser.Id);
            data = RenderRazorViewToString("_Contract", list);
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

        [HttpPost]
        [AjaxOnly]
        public JsonResult DeleteInvoice(int invoiceId)
        {
            var result = InvoiceHandler.DeleteInvoice(invoiceId, SiteUserDetails.LoggedInUser.Username);
            return Json(new ActionOutput { Status = result.Status, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Invoices(long ID, int MessageType)
        {
            return View("Invoices", InvoiceHandler.GetInvoiceByUserAndType(ID, SiteUserDetails.LoggedInUser.Id, (MessagePlaceHolder)MessageType).List);
        }

        public JsonResult setDocStatus(int ID, int Type)
        {
            return Json(MessageHandler.setDocStatus(ID, Type, SiteUserDetails.LoggedInUser.Id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteContractDoc(int FileID)
        {
            return Json(MessageHandler.DeleteContractDoc(FileID, SiteUserDetails.LoggedInUser.Id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult _TermsAndConditions(int InvoiceID, int RequestID)
        {
            ViewBag.InvoiceID = InvoiceID;
            ViewBag.RequestID = RequestID;
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string>
                    {
                        RenderRazorViewToString("_TermsAndConditions", null)
                    }
            });
        }
    }
}
