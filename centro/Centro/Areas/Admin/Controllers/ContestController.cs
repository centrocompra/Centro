using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Handler;
using BusinessLayer.Classes;
using System.Web.Script.Serialization;
using BusinessLayer.Models.ViewModel;
using System.Drawing;

namespace Centro.Areas.Admin.Controllers
{
    public class ContestController : AdminBaseController
    {
        //
        // GET: /Admin/Contest/

        public ActionResult ManageContest()
        {
            var model = ContestHandler.GetContestListing(1, 20, "Desc", "CreatedOn", null, null, null);
            return View(model);
        }

        public ActionResult ManageContestRequest()
        {
            return View();
        }

        public ActionResult AddContest()
        {
            var model = new Contest();
            model.StartDate = DateTime.Now;
            model.EndDate = DateTime.Now.AddDays(1);
            model.Categories = CategoriesHandler.GetCategories().List;
            return View(model);
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

        public JsonResult Save(Contest model)
        {
            HttpCookie pic = Request.Cookies[Cookies.UserTempPictures];
            if (pic != null)
            {
                var temp_pics = new JavaScriptSerializer().Deserialize<UserProductTempPictures>(pic.Value);
                model.ContestImage = temp_pics.UserTempPictures[0].SavedName;
                Utility.MoveFile("~/Temp/" + CentroUsers.LoggedInUser.Username + "/" + temp_pics.UserTempPictures[0].SavedName, "~/Images/Contest/" + CentroUsers.LoggedInUser.Username + "/", temp_pics.UserTempPictures[0].SavedName);
                Utility.MoveFile("~/Temp/" + CentroUsers.LoggedInUser.Username + "/thumb_" + temp_pics.UserTempPictures[0].SavedName, "~/Images/Contest/" + CentroUsers.LoggedInUser.Username + "/thumb_", temp_pics.UserTempPictures[0].SavedName);
            }
            model.UserID = CentroUsers.LoggedInUser.Id;
            model.Username = CentroUsers.LoggedInUser.Username;
            var output = ContestHandler.Save(model);
            List<string> contest_detail = output.Results;
            if (output.Status == ActionStatus.Successfull)
            {
                AccountActivity objActivity = new AccountActivity();
                objActivity.ActivityID = output.ID;
                objActivity.ActivityType = (int)FollowType.Contest;
                objActivity.UserID = CentroUsers.LoggedInUser.Id;
                objActivity.ActivityImage = model.ContestImage != null ? "/Images/Contest/" + CentroUsers.LoggedInUser.Username + "/" + model.ContestImage : "/Content/Images/Contest.png";
                objActivity.ActivityLink = "/Contest/Entries/" + output.ID;
                objActivity.ActivityText = (ActivityText.ContestCreate.Replace("{UserName}", CentroUsers.LoggedInUser.Username)
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
                    Utility.MoveFile("~/Temp/Attachments/" + CentroUsers.LoggedInUser.Username + "/" + file.SavedName, "~/Images/Contest/" + CentroUsers.LoggedInUser.Username + "/", file.SavedName);
                }
            }
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _Contest(int page_no, int per_page_result, string sortOrder, string sortColumn, string search, int? CategoryID, int? UserID)
        {

            PagingResult<ContestViewModel> model = ContestHandler.GetContestListing(page_no, per_page_result, sortOrder, sortColumn, search, CategoryID, UserID);
            string view = RenderRazorViewToString("_Contests", model);
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string> { view, model.TotalCount.ToString(), model.Message }
            }, JsonRequestBehavior.AllowGet);
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
                string temp_folder = Server.MapPath("~/Temp/" + CentroUsers.LoggedInUser.Username + "/");
                if (!System.IO.Directory.Exists(temp_folder))
                    System.IO.Directory.CreateDirectory(temp_folder);
                file.SaveAs(temp_folder + newFilename);

                // Generating Thumbnail and save it.
                Bitmap thumbnail = new Bitmap(ImageProcessor.ResizeImage(bitmap, new Size(170, 170), false));
                thumbnail.Save(Server.MapPath("~/Temp/" + CentroUsers.LoggedInUser.Username + "/thumb_") + newFilename);
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
                        Username = CentroUsers.LoggedInUser.Username,
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
                        Username = CentroUsers.LoggedInUser.Username,
                        SizeInBytes = file.ContentLength,
                        SizeInKB = file.ContentLength / 1024,
                        SizeInMB = file.ContentLength / (1024 * 1024),
                    });
                    UpdateCustomCookie(Cookies.UserTempPictures, false, new JavaScriptSerializer().Serialize(temp_pics));
                }
                #endregion
                result = new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { CentroUsers.LoggedInUser.Username, newFilename, "thumb_" + newFilename } };
            }
            else
            {
                result = new ActionOutput { Status = ActionStatus.Error, Results = new List<string> { CentroUsers.LoggedInUser.Username }, Message = "Invalid file, valid file are *.jpg, *.jpeg, *.gif, *.bmp, *.png" };
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
            string temp_folder = Server.MapPath("~/Temp/Attachments/" + CentroUsers.LoggedInUser.Username + "/");
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

        public JsonResult DeactivateContest(string Ids)
        {
            List<int> ContestIds = Ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(m => int.Parse(m)).ToList();
            return Json(ContestHandler.DeactivateContest(ContestIds), JsonRequestBehavior.AllowGet);
        }

        public JsonResult ActivateContest(string Ids)
        {
            List<int> ContestIds = Ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(m => int.Parse(m)).ToList();
            return Json(ContestHandler.ActivateContest(ContestIds), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Entries(int id, string name)
        {
            ViewBag.ContestName = name;
            ViewBag.ContestID = id;
            return View(ContestHandler.GetContestParticipants(1, 20, "Desc", "CreatedOn", "", id));
        }

        public JsonResult _Entries(int page_no, int per_page_result, string sortOrder, string sortColumn, string search, int ContestID)
        {

            PagingResult<ContestParticipantsViewModel> model = ContestHandler.GetContestParticipants(page_no, per_page_result, sortOrder, sortColumn, search, ContestID);
            string view = RenderRazorViewToString("_Entries", model);
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string> { view, model.TotalCount.ToString(), model.Message }
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetWinners(int ContestID, int First, int? Second, int? Third)
        {
            return Json(ContestHandler.SetWinners(ContestID, First, Second, Third), JsonRequestBehavior.AllowGet);
        }
    }
}
