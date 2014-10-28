using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Classes;
using System.Drawing;
using System.Web.Script.Serialization;
using BusinessLayer.Handler;
using PayPal.AdaptivePayments.Model;
using PayPal.AdaptivePayments;

namespace Centro.Controllers
{
    public class ContestController : FrontEndBaseController
    {

        [SkipAuthentication]
        public ActionResult Details(int id, string name)
        {
            ViewBag.LoggedInUser = SiteUserDetails.LoggedInUser;
            ViewBag.HasShop = SiteUserDetails.LoggedInUser != null ? (SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object != null ? true : false) : false;
            SetPageView(id, Cookies.ViewedPage);
            return View(ContestHandler.GetContest(id).Object);
        }

        public ActionResult Create()
        {
            ViewBag.PageType = "ViewContest";
            ViewBag.UserName = "";
            HttpCookie user_temp_cookie = Request.Cookies[Cookies.UserTempPictures];
            if (user_temp_cookie != null)
            {
                user_temp_cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(user_temp_cookie);
            }
            HttpCookie user_cookie = Request.Cookies[Cookies.TempAttachments];
            if (user_cookie != null)
            {
                user_cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(user_cookie);
            }
            var model = new Contest();
            model.StartDate = DateTime.Now;
            model.EndDate = DateTime.Now.AddDays(1);
            model.Categories = CategoriesHandler.GetCategories().List;
            return View(model);
        }

        public JsonResult UploadPicture(HttpPostedFileBase file)
        {
            string newFilename = Guid.NewGuid().ToString() + "." + file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            string fileExt = System.IO.Path.GetExtension(file.FileName).ToLower();
            //if(photo.ContentLength>
            ActionOutput result;
            if (fileExt == ".jpeg" || fileExt == ".jpg" || fileExt == ".gif" || fileExt == ".bmp" || fileExt == ".png")
            {
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(file.InputStream);
                if (bitmap.Height > 600 || bitmap.Height < 200)
                {
                    if ((Request.Browser).Browser == "IE")
                        return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Image must be between 200x200 to 600x600 pixel." }, "text/plain", JsonRequestBehavior.AllowGet);
                    else
                        return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Image must be between 200x200 to 600x600 pixel." }, JsonRequestBehavior.AllowGet);
                }
                // Saving Image to Temp Folder
                string temp_folder = Server.MapPath("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/");
                if (!System.IO.Directory.Exists(temp_folder))
                    System.IO.Directory.CreateDirectory(temp_folder);
                file.SaveAs(temp_folder + newFilename);

                // Generating Thumbnail and save it.
                Bitmap thumbnail = new Bitmap(ImageProcessor.ResizeImage(bitmap, new Size(170, 170), false));
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
                    temp_pics.UserTempPictures.Clear();
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

        public JsonResult Save(Contest model)
        {
            HttpCookie pic = Request.Cookies[Cookies.UserTempPictures];
            if (pic != null)
            {
                var temp_pics = new JavaScriptSerializer().Deserialize<UserProductTempPictures>(pic.Value);
                model.ContestImage = temp_pics.UserTempPictures[0].SavedName;
                Utility.MoveFile("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/" + temp_pics.UserTempPictures[0].SavedName, "~/Images/Contest/" + SiteUserDetails.LoggedInUser.Username + "/", temp_pics.UserTempPictures[0].SavedName);
                Utility.MoveFile("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/thumb_" + temp_pics.UserTempPictures[0].SavedName, "~/Images/Contest/" + SiteUserDetails.LoggedInUser.Username + "/thumb_", temp_pics.UserTempPictures[0].SavedName);
            }
            model.UserID = SiteUserDetails.LoggedInUser.Id;
            model.Username = SiteUserDetails.LoggedInUser.Username;
            var output = ContestHandler.Save(model);
            List<string> contest_detail = output.Results;
            if (output.Status == ActionStatus.Successfull)
            {
                AccountActivity objActivity = new AccountActivity();
                objActivity.ActivityID = output.ID;
                objActivity.ActivityType = (int)FollowType.Contest;
                objActivity.UserID = SiteUserDetails.LoggedInUser.Id;
                objActivity.ActivityImage = model.ContestImage != null ? "/Images/Contest/" + SiteUserDetails.LoggedInUser.Username + "/" + model.ContestImage : "/Content/Images/Contest.png";
                objActivity.ActivityLink = "/Contest/Entries/" + output.ID;
                objActivity.ActivityText = (ActivityText.ContestCreate.Replace("{UserName}", SiteUserDetails.LoggedInUser.Username)
                                                                      .Replace("{ContestTitle}", contest_detail[1])
                                                                      .Replace("{Link}", objActivity.ActivityLink)
                                                                      .Replace("{Date}", DateTime.Now.ToShortTimeString() + " " + DateTime.Now.ToLongDateString()));
                AccountActivityHandler.SaveActivity(objActivity);
            }

            HttpCookie attachments = Request.Cookies[Cookies.TempAttachments];
            if (attachments != null)
            {
                var temp_attachments = new JavaScriptSerializer().Deserialize<ProductFiles>(attachments.Value);
                ContestHandler.SaveAttachments(temp_attachments.ProductTempFiles, output.ID);
                foreach (var file in temp_attachments.ProductTempFiles)
                {
                    Utility.MoveFile("~/Temp/Attachments/" + SiteUserDetails.LoggedInUser.Username + "/" + file.SavedName, "~/Images/Contest/" + SiteUserDetails.LoggedInUser.Username + "/", file.SavedName);
                }
            }
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        [SkipAuthentication]
        public ActionResult ViewContest(string UserName, int? id, string CategoryName)
        {
            UserName = !string.IsNullOrEmpty(UserName) ? Utility.HifenToSpace(UserName) : "";
            User user = UsersHandler.UserByUsername(UserName).Object;
            ViewBag.UserID = user != null ? (int?)user.UserID : null;
            ViewBag.IsLoggedIn = SiteUserDetails.LoggedInUser != null ? true : false;
            ViewBag.CategoryID = id;
            ViewBag.UserID = user != null ? (int?)user.UserID : null;
            ViewBag.UserName = UserName;
            return View("ViewContest", ContestHandler.GetContestListing(1, 9, "Desc", "CreatedOn", null, id, user != null ? (int?)user.UserID : null));
        }

        //[SkipAuthentication]
        //public ActionResult UserContest(int? id, string UserName)
        //{
        //    UserName = Utility.HifenToSpace(UserName);
        //    ViewBag.IsLoggedIn = CentroUsers.LoggedInUser != null ? true : false;
        //    User user = UsersHandler.UserByUsername(UserName).Object;
        //    ViewBag.UserID = user != null ? (int?)user.UserID : null;
        //    ViewBag.CategoryID = id;
        //    ViewBag.MyContest = false;
        //    ViewBag.PageType = "UserContest";
        //    ViewBag.UserName = UserName;
        //    // if(id.HasValue && id.Value>0)
        //    return View("ViewContest", ContestHandler.GetContestListing(1, 9, "Desc", "TotalViews", null, id, user.UserID));
        //    //return View("ViewContest",ContestHandler.GetContest().List);
        //}

        [SkipAuthentication]
        public FileResult Download(int id)
        {
            var file = ContestHandler.GetContestAttachment(id).Object;
            var fileData = Utility.ReadFileBytes("~/Images/Contest/" + UsersHandler.GetUserByID(file.Contest.UserID).Object.UserName + "/" + file.SavedName);
            return File(fileData, file.MimeType);
        }

        [SkipAuthentication]
        public ActionResult Participate(int id)
        {
            if (SiteUserDetails.LoggedInUser == null)
            {
                CreateCustomCookie(Cookies.ReturnUrlCookie, false, "/Contest/Participate/" + id, 20);
                //return Json(new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { Url.Action("Signin", "Home") } }, JsonRequestBehavior.AllowGet);
                return RedirectToAction("Signin", "Home");
            }
            var shop = SiteUserDetails.LoggedInUser != null ? SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object : null;
            if (shop == null)
            {
                TempData["ErrorMessage"] = "Please create a shop first to participate.";
                return RedirectToAction("ViewContest", "Contest");
            }
            var model = new ContestParticipant();
            model.ContestID = id;
            model.UserID = SiteUserDetails.LoggedInUser.Id;
            model.ShopID = shop.ShopID;
            model.Contest = ContestHandler.GetContest(id).Object;
            ViewBag.IsLoggedIn = true;
            ViewBag.Username = SiteUserDetails.LoggedInUser.Username;
            ViewBag.Products = ProductsHandler.ProductsToProductsViewModel(ProductsHandler.ProductsByShopId(shop.ShopID).List).List;
            return View(model);
        }

        public JsonResult SaveParticipate(ContestParticipant model)
        {
            return Json(ContestHandler.SaveParticipate(model), JsonRequestBehavior.AllowGet);
        }

        [SkipAuthentication]
        public ActionResult Entries(int id)
        {
            ViewBag.LoggedInUser = SiteUserDetails.LoggedInUser;
            ViewBag.HasShop = SiteUserDetails.LoggedInUser != null ? (SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object != null ? true : false) : false;
            ViewBag.Contest = ContestHandler.GetContest(id).Object;
            List<ContestParticipantsViewModel> model = ContestHandler.GetContestParticipants(id).List;
            ViewBag.LoggedInUserId = SiteUserDetails.LoggedInUser != null ? SiteUserDetails.LoggedInUser.Id : 0;
            return View(model);
        }

        public JsonResult ParticipantVoteUp(int ContestparticipantID, int ContestID)
        {
            return Json(ContestHandler.ParticipantVoteUp(SiteUserDetails.LoggedInUser.Id, ContestparticipantID, ContestID), JsonRequestBehavior.AllowGet);
        }

        public JsonResult ParticipantVoteDown(int ContestparticipantID, int ContestID)
        {
            return Json(ContestHandler.ParticipantVoteDown(SiteUserDetails.LoggedInUser.Id, ContestparticipantID, ContestID), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Entry(int id)
        {
            var model = ContestHandler.GetContestParticipant(id).Object;
            Product pro = ProductsHandler.ProductById(model.ContestParticipant.ProductID).Object;
            ViewBag.Product = pro;
            ViewBag.ShopOwner = SellersHandler.ShopOwnerByProductId(pro.ProductID).Object;
            ViewBag.UserId = SiteUserDetails.LoggedInUser.Id;
            ViewBag.Contest = ContestHandler.GetContest(model.ContestParticipant.ContestID).Object;
            ViewBag.Shop = SellersHandler.ShopByShopId(pro.ShopId).Object;
            ViewBag.ShopProducts = ProductsHandler.ProductsByShopId(pro.ShopId).List;
            ViewBag.Comments = ContestHandler.GetComments(id).List;
            // Set/Update Total Views of this entry
            ContestHandler.UpdateTotalViews(model.ContestParticipant.ContestparticipantID, Request.ServerVariables["REMOTE_ADDR"], SiteUserDetails.LoggedInUser.Id);
            return View(model);
        }

        public JsonResult SaveComment(ContestComment obj)
        {
            string contest_name = Request["ContestName"].ToString();
            string contest_id = Request["ContestID"].ToString();
            string contest_image = Request["ContestImage"].ToString();
            obj.UserID = SiteUserDetails.LoggedInUser.Id;
            obj.Username = SiteUserDetails.LoggedInUser.Username;
            var result = ContestHandler.SaveComment(obj);
            if (result.Status == ActionStatus.Successfull)
            {
                AccountActivity objActivity = new AccountActivity();
                objActivity.ActivityID = result.ID;
                objActivity.ActivityType = (int)FollowType.Contest;
                objActivity.UserID = SiteUserDetails.LoggedInUser.Id;
                objActivity.ActivityImage = "/Images/Contest/" + SiteUserDetails.LoggedInUser.Username + "/" + contest_image;
                objActivity.ActivityLink = "/Contest/Entry/" + contest_id;
                objActivity.ActivityText = (ActivityText.ContestComment.Replace("{UserName}", SiteUserDetails.LoggedInUser.Username)
                                                                       .Replace("{ContestTitle}", contest_name)
                                                                       .Replace("{Link}", objActivity.ActivityLink)
                                                                       .Replace("{Date}", DateTime.Now.ToShortTimeString() + " " + DateTime.Now.ToLongDateString()));
                AccountActivityHandler.SaveActivity(objActivity);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Winner(int id)
        {
            var model = ContestHandler.GetContest(id).Object;
            ViewBag.Winner = UsersHandler.GetUserByID(model.WinnerID.Value).Object;
            ViewBag.WinnerContestParticipant = ContestHandler.GetWinnerContestParticipant(model.ContestID).Object;
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

        public JsonResult MakeDonation(int ContestID, decimal Amount)
        {
            string ReturnUrl = "http://" + Request.Url.Authority + Url.Action("ReturnFromDonation", "Contest");
            string CancelUrl = "http://" + Request.Url.Authority + Url.Action("Entries", "Contest", new { id = ContestID });

            var output = PaymentHandler.MakeDonationUsingPaypal(Amount, ReturnUrl, CancelUrl);
            if (output.Status != ActionStatus.Successfull)
                return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Something went wrong, please try again later." }, JsonRequestBehavior.AllowGet);
            // Saving paykey to database
            PaymentHandler.SaveDonationTransaction(new DonationTransaction { UserID = SiteUserDetails.LoggedInUser.Id, Paykey = output.Results[1], Amount = Amount, ContestID = ContestID });
            // Saving paykey to Cookie
            Donation donation = new Donation();
            donation.Amount = Amount;
            donation.ContestID = ContestID;
            donation.PayKey = output.Results[1];
            donation.UserID = SiteUserDetails.LoggedInUser.Id;
            CreateCustomCookie(Cookies.DonationPaykey, false, new JavaScriptSerializer().Serialize(donation));
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReturnFromDonation()
        {
            // Check payment status
            int contestid = 0;
            HttpCookie cookie = Request.Cookies[Cookies.DonationPaykey];
            if (cookie != null)
            {
                Donation donation = new JavaScriptSerializer().Deserialize<Donation>(cookie.Value);
                PaymentDetailsRequest req = new PaymentDetailsRequest(new RequestEnvelope("en_US"));
                req.payKey = donation.PayKey;
                contestid = donation.ContestID;
                // All set. Fire the request            
                AdaptivePaymentsService service = new AdaptivePaymentsService();
                PaymentDetailsResponse resp = null;

                resp = service.PaymentDetails(req);

                // Display response values. 
                Dictionary<string, string> keyResponseParams = new Dictionary<string, string>();
                if (!(resp.responseEnvelope.ack == AckCode.FAILURE) && !(resp.responseEnvelope.ack == AckCode.FAILUREWITHWARNING))
                {
                    keyResponseParams.Add("Pay key", resp.payKey);
                    keyResponseParams.Add("Payment execution status", resp.status);
                    keyResponseParams.Add("Sender email", resp.senderEmail);

                    //Selenium Test Case
                    keyResponseParams.Add("Acknowledgement", resp.responseEnvelope.ack.ToString());
                    keyResponseParams.Add("Action Type", resp.actionType);

                    string[] ResponseArray = service.getLastResponse().Split(new char[] { '&' });
                    Dictionary<string, string> ResponseDictionary = new Dictionary<string, string>();
                    foreach (string resp_item in ResponseArray)
                    {
                        string[] arr = resp_item.Split(new char[] { '=' });
                        ResponseDictionary.Add(arr[0].Trim(), arr[1].Trim());
                    }

                    // Updating UserPaykey db table based on Paykey
                    PaymentHandler.UpdateDonationTransaction(new DonationTransaction
                    {
                        Paykey = req.payKey,
                        Message = ResponseDictionary["paymentInfoList.paymentInfo(0).transactionStatus"],
                        Status = ResponseDictionary["status"],
                        TransactionID = ResponseDictionary["paymentInfoList.paymentInfo(0).transactionId"],
                        Amount = donation.Amount,
                        ContestID = donation.ContestID
                    });

                    // Remove Donation Cookie
                    cookie.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(cookie);
                }
            }
            return RedirectToAction("Entries", "Contest", new { id = contestid });
        }

        public ActionResult MyContest(int? id, string name)
        {
            ViewBag.IsLoggedIn = SiteUserDetails.LoggedInUser != null ? true : false;
            ViewBag.CategoryID = id;
            ViewBag.MyContest = true;
            ViewBag.UserID = SiteUserDetails.LoggedInUser.Id;
            ViewBag.PageType = "MyContest";
            ViewBag.UserName = SiteUserDetails.LoggedInUser.Username;
            // if(id.HasValue && id.Value>0)
            return View("ViewContest", ContestHandler.GetContestListing(1, 9, "Desc", "TotalViews", null, id, SiteUserDetails.LoggedInUser.Id));
            //return View("ViewContest",ContestHandler.GetContest().List);
        }

        public JsonResult VoteUp(int ID)
        {
            return Json(ContestHandler.VoteUP(ID, SiteUserDetails.LoggedInUser.Id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult RequestChallenge(ContestRequest model)
        {
            model.Date = DateTime.Now;
            model = ContestHandler.AddContestRequest(model);            
            if (model.ContestRequestId == 0)
            {
                return Json(new ActionOutput { Status = ActionStatus.Error}, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new ActionOutput { Status = ActionStatus.Successfull, ID = model.ContestRequestId, Message="Request has been sent." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult OpenRequestChallenge()
        {
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string>
                {
                    RenderRazorViewToString("_OpenRequestChallenge",new ContestRequest())
                }
            });
        }
    }
}
