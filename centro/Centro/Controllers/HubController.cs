using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Handler;
using BusinessLayer.Classes;
using BusinessLayer.Models.ViewModel;
using System.Drawing;
using System.Web.Script.Serialization;


namespace Centro.Controllers
{
    [NoCache]
    public class HubController : FrontEndBaseController
    {
        public ActionResult Index()
        {
            ViewBag.CentroUsers = SiteUserDetails;
            ViewBag.Shop = SellersHandler.ShopByUserId(SiteUserDetails.LoggedInUser.Id).Object;
            ViewBag.UserID = SiteUserDetails.LoggedInUser.Id;
            return View(HubHandler.GetHubListing(1, 9, "desc", "CreatedOn", "", null, SiteUserDetails.LoggedInUser.Id, true));
        }

        public ActionResult Create()
        {
            System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(Server.MapPath("~/Temp/" + SiteUserDetails.LoggedInUser.Username));
            d.Empty();
            // delete item data from cookie
            HttpCookie user_temp_cookie = Request.Cookies[Cookies.UserTempPictures];
            if (user_temp_cookie != null)
            {
                user_temp_cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(user_temp_cookie);
            }
            HttpCookie user_temp_hub_cookie = Request.Cookies[Cookies.UserTempHubPictures];
            if (user_temp_hub_cookie != null)
            {
                user_temp_hub_cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(user_temp_hub_cookie);
            }
            Hub model = new Hub();
            model.HubTopics = HubHandler.GetHubTopics().List;
            model.HubTemplateID = (int)HubTemplate.Template1;
            ViewBag.UserName = SiteUserDetails.LoggedInUser.Username;
            return View(model);
        }

        public ActionResult EditHub(int HubID)
        {
            Hub model = new Hub();
            model = HubHandler.GetHubById(HubID, SiteUserDetails.LoggedInUser.Id).Object;
            model.HubTopics = HubHandler.GetHubTopics().List;
            ViewBag.UserName = SiteUserDetails.LoggedInUser.Username;
            return View(model);
        }

        public JsonResult _LoadTemplate(int HubTemplateID)
        {
            string view = string.Empty;
            view = HubTemplateID == (int)HubTemplate.Template1 ? RenderRazorViewToString("_Template1", null) : RenderRazorViewToString("_Template2", null);

            //System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(Server.MapPath("~/Temp/" + CentroUsers.LoggedInUser.Username));
            //d.Empty();
            // delete item data from cookie
            HttpCookie user_temp_cookie = Request.Cookies[Cookies.UserTempPictures];
            if (user_temp_cookie != null)
            {
                user_temp_cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(user_temp_cookie);
            }

            return Json(new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { view } }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UploadPicture(HttpPostedFileBase file)
        {
            string Counter = Request["Counter"];
            string newFilename = Guid.NewGuid().ToString() + "." + file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            string fileExt = System.IO.Path.GetExtension(file.FileName).ToLower();
            //if(photo.ContentLength>
            ActionOutput result;
            if (fileExt == ".jpeg" || fileExt == ".jpg" || fileExt == ".gif" || fileExt == ".bmp" || fileExt == ".png")
            {
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(file.InputStream);
                if (bitmap.Height > 1024 || bitmap.Height < 100)
                {
                    if ((Request.Browser).Browser == "IE")
                        return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Image must be between 100x100 to 1024x1024 pixel." }, "text/plain", JsonRequestBehavior.AllowGet);
                    else
                        return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Image must be between 100x100 to 1024x1024 pixel." }, JsonRequestBehavior.AllowGet);
                }
                // Saving Image to Temp Folder
                string temp_folder = Server.MapPath("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/");
                if (!System.IO.Directory.Exists(temp_folder))
                    System.IO.Directory.CreateDirectory(temp_folder);
                file.SaveAs(temp_folder + newFilename);

                // Generating Thumbnail and save it.
                Bitmap thumbnail = new Bitmap(ImageProcessor.ResizeImage(bitmap, new Size(165, 165), false));
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
                        Counter = Counter,
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
                        Counter = Counter,
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

        public JsonResult UploadHubPicture(HttpPostedFileBase file)
        {
            string Counter = Request["Counter"];
            string newFilename = Guid.NewGuid().ToString() + "." + file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            string fileExt = System.IO.Path.GetExtension(file.FileName).ToLower();
            //if(photo.ContentLength>
            ActionOutput result;
            if (fileExt == ".jpeg" || fileExt == ".jpg" || fileExt == ".gif" || fileExt == ".bmp" || fileExt == ".png")
            {
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(file.InputStream);
                if (bitmap.Height > 1024 || bitmap.Height < 100)
                {
                    if ((Request.Browser).Browser == "IE")
                        return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Image must be between 100x100 to 1024x1024 pixel." }, "text/plain", JsonRequestBehavior.AllowGet);
                    else
                        return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Image must be between 100x100 to 1024x1024 pixel." }, JsonRequestBehavior.AllowGet);
                }
                // Saving Image to Temp Folder
                string temp_folder = Server.MapPath("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/");
                if (!System.IO.Directory.Exists(temp_folder))
                    System.IO.Directory.CreateDirectory(temp_folder);
                file.SaveAs(temp_folder + newFilename);

                // Generating Thumbnail and save it.
                Bitmap thumbnail = new Bitmap(ImageProcessor.ResizeImage(bitmap, new Size(165, 165), false));
                thumbnail.Save(Server.MapPath("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/thumb_") + newFilename);
                #region Temp Cookie
                // Check if cookie already exists
                UserProductTempPictures temp_pics;
                HttpCookie user_temp_cookie = Request.Cookies[Cookies.UserTempHubPictures];
                if (user_temp_cookie == null)
                {
                    temp_pics = new UserProductTempPictures();
                    temp_pics.UserTempPictures = new List<UserProductPicture>();
                    temp_pics.UserTempPictures.Add(new UserProductPicture
                    {
                        Counter = Counter,
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
                    CreateCustomCookie(Cookies.UserTempHubPictures, false, new JavaScriptSerializer().Serialize(temp_pics));
                }
                else
                {
                    // update existing temp cookie
                    temp_pics = new JavaScriptSerializer().Deserialize<UserProductTempPictures>(user_temp_cookie.Value);
                    temp_pics.UserTempPictures.RemoveAt(0);
                    temp_pics.UserTempPictures.Add(new UserProductPicture
                    {
                        Counter = Counter,
                        DisplayName = file.FileName,
                        MimeType = file.ContentType,
                        SavedName = newFilename,
                        Thumbnail = "thumb_" + newFilename,
                        Username = SiteUserDetails.LoggedInUser.Username,
                        SizeInBytes = file.ContentLength,
                        SizeInKB = file.ContentLength / 1024,
                        SizeInMB = file.ContentLength / (1024 * 1024),
                    });
                    UpdateCustomCookie(Cookies.UserTempHubPictures, false, new JavaScriptSerializer().Serialize(temp_pics));
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

        public JsonResult DeleteTempPicture(string filename, string pic_id)
        {
            // delete from disk
            Utility.DeleteFile(filename);
            Utility.DeleteFile(filename.Replace("thumb_", ""));
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
                temp_pics.UserTempPictures.Remove(pic);
                // Creating Temp cookie for User product pics
                CreateCustomCookie(Cookies.UserTempPictures, false, new JavaScriptSerializer().Serialize(temp_pics));
            }
            return Json(new ActionOutput { Status = ActionStatus.Successfull }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteTemp2Picture(string count)
        {
            // delete from cookie
            HttpCookie user_temp_cookie = Request.Cookies[Cookies.UserTempPictures];
            if (user_temp_cookie != null)
            {
                UserProductTempPictures temp_pics = new JavaScriptSerializer().Deserialize<UserProductTempPictures>(user_temp_cookie.Value);
                UserProductPicture pic = temp_pics.UserTempPictures.Where(m => m.Counter == count).FirstOrDefault();
                if (pic != null)
                {
                    temp_pics.UserTempPictures.Remove(pic);
                    // Creating Temp cookie for User product pics
                    CreateCustomCookie(Cookies.UserTempPictures, false, new JavaScriptSerializer().Serialize(temp_pics));
                    // delete from disk
                    Utility.DeleteFile("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/" + pic.SavedName); ;
                    Utility.DeleteFile("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/thumb_" + pic.SavedName); ;
                }
            }
            return Json(new ActionOutput { Status = ActionStatus.Successfull }, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        public ActionResult SaveHub(Hub obj)
        {
            obj.HubStatus = (int)HubStatus.Active;
            short[] Rows = !string.IsNullOrEmpty(Request["Rows"]) ? Request["Rows"].Split(new char[] { ',' }).Select(m => Convert.ToInt16(m)).ToArray() : new short[0];
            short[] Except = !string.IsNullOrEmpty(Request["Except"]) ? Request["Except"].Split(new char[] { ',' }).Select(m => Convert.ToInt16(m)).ToArray() : new short[0];

            List<HubContent> HubContent = new List<HubContent>();
            HttpCookie user_temp_cookie = Request.Cookies[Cookies.UserTempPictures];
            HttpCookie user_temp_hub_cookie = Request.Cookies[Cookies.UserTempHubPictures];

            UserProductTempPictures temp_pics = null;
            UserProductTempPictures temp_hub_pic = null;
            if (user_temp_hub_cookie != null)
            {
                temp_hub_pic = new JavaScriptSerializer().Deserialize<UserProductTempPictures>(user_temp_hub_cookie.Value);
                var hubpic = temp_hub_pic.UserTempPictures.Where(m => m.Counter == null).FirstOrDefault();
                if (hubpic != null)
                    obj.HubPicture = hubpic.SavedName;
            }

            List<string> hubdetail = HubHandler.SaveHub(obj, SiteUserDetails.LoggedInUser.Id).Results;
            int hubid = Convert.ToInt32(hubdetail[0]);
            string hub_title = hubdetail[1];

            if (temp_hub_pic != null)
            {
                var file = temp_hub_pic.UserTempPictures[0];
                Utility.MoveFile("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/" + file.SavedName, "~/Images/" + SiteUserDetails.LoggedInUser.Username + "/Hubs/", file.SavedName);
                Utility.MoveFile("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/thumb_" + file.SavedName, "~/Images/" + SiteUserDetails.LoggedInUser.Username + "/Hubs/thumb_", file.SavedName);
            }

            if (user_temp_cookie != null)
                temp_pics = new JavaScriptSerializer().Deserialize<UserProductTempPictures>(user_temp_cookie.Value);
            foreach (short i in Rows.Where(m => !Except.Contains(m)).ToArray())
            {
                var pic = temp_pics != null ? temp_pics.UserTempPictures.Where(m => m.Counter == i.ToString()).FirstOrDefault() : null;
                if (obj.HubTemplateID == (int)HubTemplate.Template1)
                {
                    HubContent.Add(new HubContent
                    {
                        ContentText = Request["Count_" + i],
                        HubID = hubid,
                        DisplayName = pic != null ? pic.DisplayName : null,
                        MimeType = pic != null ? pic.MimeType : null,
                        SavedName = pic != null ? pic.SavedName : null,
                        SizeInBytes = pic != null ? (int?)pic.SizeInBytes : null,
                        SizeInKB = pic != null ? (decimal?)pic.SizeInKB : null,
                        SizeInMB = pic != null ? (decimal?)pic.SizeInMB : null,
                        Thumbnail = pic != null ? pic.Thumbnail : null,
                    });
                }
                else
                {
                    HubContent.Add(new HubContent
                    {
                        ContentText = obj.Count[i],
                        HubID = hubid,
                        DisplayName = pic != null ? pic.DisplayName : null,
                        MimeType = pic != null ? pic.MimeType : null,
                        SavedName = pic != null ? pic.SavedName : null,
                        SizeInBytes = pic != null ? (int?)pic.SizeInBytes : null,
                        SizeInKB = pic != null ? (decimal?)pic.SizeInKB : null,
                        SizeInMB = pic != null ? (decimal?)pic.SizeInMB : null,
                        Thumbnail = pic != null ? pic.Thumbnail : null,
                    });
                }
            }

            var result = HubHandler.SaveHubContents(HubContent);
            // delete cookie and move temp files
            if (temp_pics != null)
            {
                foreach (var file in temp_pics.UserTempPictures)
                {
                    Utility.MoveFile("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/" + file.SavedName, "~/Images/" + SiteUserDetails.LoggedInUser.Username + "/Hubs/", file.SavedName);
                    Utility.MoveFile("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/thumb_" + file.SavedName, "~/Images/" + SiteUserDetails.LoggedInUser.Username + "/Hubs/", file.Thumbnail);
                }
                user_temp_cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(user_temp_cookie);
            }
            HubTopic topic = HubHandler.GetHubTopicByTopicID(obj.HubTopicID).Object;
            if (hubid > 0)
            {
                AccountActivity objActivity = new AccountActivity();
                objActivity.ActivityID = hubid;
                objActivity.ActivityType = (int)FollowType.Hub;                
                objActivity.UserID = SiteUserDetails.LoggedInUser.Id;
                objActivity.ActivityImage = "/Images/" + SiteUserDetails.LoggedInUser.Username + "/Hubs/" + obj.HubPicture;
                objActivity.ActivityLink = "/Hubs/" + SiteUserDetails.LoggedInUser.Username + "/" + Utility.SpacesToHifen(topic.HubTopic1) + "/" + obj.HubID + "/" + Utility.SpacesToHifen(obj.Title);
                objActivity.ActivityText = (ActivityText.HubCreate.Replace("{UserName}", SiteUserDetails.LoggedInUser.Username)
                                                                  .Replace("{HubTitle}", hub_title)
                                                                  .Replace("{Link}", objActivity.ActivityLink)
                                                                  .Replace("{Date}", DateTime.Now.ToShortTimeString()+" "+DateTime.Now.ToLongDateString()));
                AccountActivityHandler.SaveActivity(objActivity);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        public ActionResult UpdateHub(Hub obj)
        {
            short[] Rows = !string.IsNullOrEmpty(Request["RowsEdit"]) ? Request["RowsEdit"].Split(new char[] { ',' }).Select(m => Convert.ToInt16(m)).ToArray() : new short[0];
            short[] Except = !string.IsNullOrEmpty(Request["ExceptEdit"]) ? Request["ExceptEdit"].Split(new char[] { ',' }).Select(m => Convert.ToInt16(m)).ToArray() : new short[0];
            //List<HubContent> HubContent = new List<HubContent>();
            HttpCookie user_temp_cookie = Request.Cookies[Cookies.UserTempPictures];
            HttpCookie user_temp_hub_cookie = Request.Cookies[Cookies.UserTempHubPictures];
            List<HubContent> HubContentToAdd = new List<HubContent>();
            List<HubContent> HubContentToEdit = new List<HubContent>();
            UserProductTempPictures temp_pics = null;
            UserProductTempPictures temp_hub_pic = null;
            if (user_temp_hub_cookie != null)
            {
                temp_hub_pic = new JavaScriptSerializer().Deserialize<UserProductTempPictures>(user_temp_hub_cookie.Value);
                var hubpic = temp_hub_pic.UserTempPictures.Where(m => m.Counter == null).FirstOrDefault();
                if (hubpic != null)
                    obj.HubPicture = hubpic.SavedName;
            }
            int is_updated = (int)HubHandler.UpdateHub(obj, SiteUserDetails.LoggedInUser.Id).Status;
            if (temp_hub_pic != null)
            {
                var file = temp_hub_pic.UserTempPictures[0];
                Utility.MoveFile("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/" + file.SavedName, "~/Images/" + SiteUserDetails.LoggedInUser.Username + "/Hubs/", file.SavedName);
                Utility.MoveFile("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/thumb_" + file.SavedName, "~/Images/" + SiteUserDetails.LoggedInUser.Username + "/Hubs/thumb_", file.SavedName);
            }
            if (user_temp_hub_cookie != null)
            {
                user_temp_hub_cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(user_temp_hub_cookie);
            }

            foreach (short i in Except)
            {
                int hub_content_id = Request["hdnContentID_" + i] != null ? Convert.ToInt32(Request["hdnContentID_" + i]) : 0;
                if (hub_content_id > 0)
                {
                    int status = (int)HubHandler.DeleteHubContent(hub_content_id).Status;
                }
            }
            if (user_temp_cookie != null)
                temp_pics = new JavaScriptSerializer().Deserialize<UserProductTempPictures>(user_temp_cookie.Value);
            foreach (short i in Rows.Where(m => !Except.Contains(m)).ToArray())
            {
                int hub_content_id = Request["hdnContentID_" + i] != null ? Convert.ToInt32(Request["hdnContentID_" + i]) : 0;
                string hub_savedImage = Request["hdnImageName_" + i];
                var pic = temp_pics != null ? temp_pics.UserTempPictures.Where(m => m.Counter == i.ToString()).FirstOrDefault() : null;
                if (hub_content_id > 0)
                {
                    HubContent obj_hub_content = new HubContent();
                    obj_hub_content.HubContentID = hub_content_id;
                    obj_hub_content.SavedName = hub_savedImage;

                    if (obj.HubTemplateID == (int)HubTemplate.Template1)
                    {
                        obj_hub_content.ContentText = Request["Count_" + i];
                    }
                    else
                    {
                        obj_hub_content.ContentText = obj.Count[i];
                    }
                    var res = HubHandler.UpdateHubContent(obj_hub_content, pic);
                }
                else
                {

                    if (obj.HubTemplateID == (int)HubTemplate.Template1)
                    {
                        HubContentToAdd.Add(new HubContent
                        {
                            ContentText = Request["Count_" + i],
                            HubID = obj.HubID,
                            DisplayName = pic != null ? pic.DisplayName : null,
                            MimeType = pic != null ? pic.MimeType : null,
                            SavedName = pic != null ? pic.SavedName : null,
                            SizeInBytes = pic != null ? (int?)pic.SizeInBytes : null,
                            SizeInKB = pic != null ? (decimal?)pic.SizeInKB : null,
                            SizeInMB = pic != null ? (decimal?)pic.SizeInMB : null,
                            Thumbnail = pic != null ? pic.Thumbnail : null,
                        });
                    }
                    else
                    {
                        HubContentToAdd.Add(new HubContent
                        {
                            ContentText = obj.Count[i],
                            HubID = obj.HubID,
                            DisplayName = pic != null ? pic.DisplayName : null,
                            MimeType = pic != null ? pic.MimeType : null,
                            SavedName = pic != null ? pic.SavedName : null,
                            SizeInBytes = pic != null ? (int?)pic.SizeInBytes : null,
                            SizeInKB = pic != null ? (decimal?)pic.SizeInKB : null,
                            SizeInMB = pic != null ? (decimal?)pic.SizeInMB : null,
                            Thumbnail = pic != null ? pic.Thumbnail : null,
                        });
                    }

                }

            }
            if (HubContentToAdd.Count > 0)
            {
                var result = HubHandler.SaveHubContents(HubContentToAdd);
            }
            // delete cookie and move temp files
            if (temp_pics != null)
            {
                foreach (var file in temp_pics.UserTempPictures)
                {
                    Utility.MoveFile("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/" + file.SavedName, "~/Images/" + SiteUserDetails.LoggedInUser.Username + "/Hubs/", file.SavedName);
                    Utility.MoveFile("~/Temp/" + SiteUserDetails.LoggedInUser.Username + "/thumb_" + file.SavedName, "~/Images/" + SiteUserDetails.LoggedInUser.Username + "/Hubs/", file.Thumbnail);
                }
                user_temp_cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(user_temp_cookie);
            }
            return Json(new ActionOutput { Status = ActionStatus.Successfull, Message = "Hub Updated Successfully" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult View(int id)
        {
            ViewBag.Username = SiteUserDetails.LoggedInUser.Username;
            return View(HubHandler.GetHubById(id, SiteUserDetails.LoggedInUser.Id).Object);
        }

        public JsonResult ActivateDeactivateHub(int HubID, int HubStatus)
        {
            return Json(HubHandler.ActivateDeactivateHub(HubID, HubStatus), JsonRequestBehavior.AllowGet);
        }

        [SkipAuthentication]
        public ActionResult HubDetails(string UserName, string HubTopic, int hubid, string HubTitle)
        {
            ViewBag.CentroUsers = SiteUserDetails.LoggedInUser != null ? SiteUserDetails : null;
            ViewBag.IsShopFavorite = false;
            //User user = UsersHandler.UserByUsername(UserName).Object;
            User user = SellersHandler.UserByUsername(UserName).Object;
            if (user.ProfilePicUrl != null)
            {
                string profileImg = user.ProfilePicUrl;
                profileImg = profileImg.Replace(user.UserName + "/", user.UserName + "/thumb_");
                user.ProfilePicUrl = profileImg;
            }
            //Hub obj = HubHandler.GetHubByHubTitle(Utility.HifenToSpace(HubTitle), user.UserID).Object;
            Hub obj = HubHandler.GetHubById(hubid, user.UserID).Object;
            ViewBag.User = user;
            Shop shop = SellersHandler.ShopByUserId(user.UserID).Object;
            ViewBag.Shop = shop;
            if (SiteUserDetails.LoggedInUser != null)
            {
                ViewBag.UserId = SiteUserDetails.LoggedInUser.Id;
                var fav_result = shop != null ? UsersHandler.CheckIsShopFavoriteByUserID(SiteUserDetails.LoggedInUser.Id, shop.ShopID).Object : null;
                if (fav_result != null)
                {
                    ViewBag.IsShopFavorite = true;
                }
            }
            else
                ViewBag.UserId = 0;
            SetPageView(obj.HubID, Cookies.ViewHubPage);
            ViewBag.HubComments = HubHandler.GetHubComments(obj.HubID).List;
            return View(obj);
        }

        [SkipAuthentication]
        public ActionResult AllHubs()
        {
            ViewBag.CentroUsers = SiteUserDetails;
            //return View(HubHandler.GetRandomHubs(12).List);
            return View(HubHandler.GetHubListing(1, 10, "desc", "CreatedOn", "", null, null));
        }

        [SkipAuthentication]
        public ActionResult HubsByTopic(string TopicName, int TopicID)
        {
            ViewBag.HubTopicText = Utility.HifenToSpace(TopicName);
            ViewBag.TopicID = TopicID;
            ViewBag.CentroUsers = SiteUserDetails;
            return View(HubHandler.GetHubListing(1, 9, "desc", "CreatedOn", "", TopicID, null));
        }

        [HttpPost]
        [SkipAuthentication]
        public ActionResult _HubList()
        {
            string sort_by = Request["SortBy"] != null ? Request["SortBy"].ToString() : "mostrecent";
            return PartialView(HubHandler.GetRandomHubs(12, sort_by).List);
        }

        [SkipAuthentication]
        public JsonResult Reports(string PageURL)
        {
            Report obj = new Report();
            ViewBag.ReportTypes = ReportsHandller.GetReportTypes().List;
            
            ViewBag.UserDetail = null;
            string url = Request.RawUrl;
            if (SiteUserDetails.LoggedInUser != null)
            {
                obj.UserID = SiteUserDetails.LoggedInUser.Id;
                User user = UsersHandler.GetUserByID(SiteUserDetails.LoggedInUser.Id).Object;
                if(user!=null)
                {
                    obj.FirstName = user.FirstName;
                    obj.LastName = user.LastName;
                    obj.EmailID = user.EmailId;
                }
                
                ViewBag.UserDetail = user;
            }
            obj.RequestSentFromURL = PageURL.Replace("-|-", "'");
            
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string>
                {
                    RenderRazorViewToString("Reports",obj )
                }
            });
        }

        [HttpPost]
        public JsonResult SaveHubComment(HubComment obj)
        {
            string hub_title = Request["HubTitle"].ToString();
            var result = HubHandler.SaveHubComment(obj);
            if (result.Status == ActionStatus.Successfull)
            {   
                AccountActivity objActivity = new AccountActivity();
                objActivity.ActivityID = result.ID;
                objActivity.ActivityType = (int)FollowType.Hub;                
                objActivity.UserID = SiteUserDetails.LoggedInUser.Id;
                objActivity.ActivityImage = obj.HubPicture != null ? "/Images/" + SiteUserDetails.LoggedInUser.Username + "/Hubs/" + obj.HubPicture : null;
                objActivity.ActivityLink = "/Hubs/" + obj.HubOwnerUserName + "/" + Utility.SpacesToHifen(obj.HubTopicText) + "/" + obj.HubID + "/" + Utility.SpacesToHifen(hub_title) + "#comments";
                objActivity.ActivityText = (ActivityText.HubComment.Replace("{UserName}", SiteUserDetails.LoggedInUser.Username)
                                                                   .Replace("{HubTitle}", hub_title)
                                                                   .Replace("{Link}", objActivity.ActivityLink)
                                                                   .Replace("{Date}", DateTime.Now.ToShortTimeString() + " " + DateTime.Now.ToLongDateString()));
                AccountActivityHandler.SaveActivity(objActivity);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [SkipAuthentication]
        [HttpPost]
        public JsonResult SendReportMessage(Report obj)
        {
            var result = ReportsHandller.SaveReport(obj);
            var resultEmail = EmailHandler.SendReportEmail(obj);
            return Json(resultEmail, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HubCommentByCommentID(int CommentID)
        {
            ViewBag.UserID = SiteUserDetails.LoggedInUser.Id;
            var obj = HubHandler.GetHubCommentByCommentID(CommentID).Object;
            return PartialView(obj);
        }

        [HttpPost]
        public JsonResult DeleteHubComment(int CommentID)
        {
            var result = HubHandler.DeleteCommentByCommentID(CommentID);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [SkipAuthentication]
        public ActionResult UserHub(string UserName)
        {
            ViewBag.CentroUsers = SiteUserDetails.LoggedInUser != null ? SiteUserDetails : null;
            ViewBag.IsShopFavorite = false;
            User user = SellersHandler.UserByUsername(UserName).Object;
            //User user = SellersHandler.ShopOwnerByShopName(UserName).Object;
            if (user.ProfilePicUrl != null)
            {
                string profileImg = user.ProfilePicUrl;
                profileImg = profileImg.Replace(user.UserName + "/", user.UserName + "/thumb_");
                user.ProfilePicUrl = profileImg;
            }
            Shop shop = SellersHandler.ShopByUserId(user.UserID).Object;
            if (SiteUserDetails.LoggedInUser != null)
            {
                ViewBag.UserId = SiteUserDetails.LoggedInUser.Id;
                var fav_result = shop!=null? UsersHandler.CheckIsShopFavoriteByUserID(SiteUserDetails.LoggedInUser.Id, shop.ShopID).Object:null;
                if (fav_result != null)
                {
                    ViewBag.IsShopFavorite = true;
                }
            }

            ViewBag.User = user;
            ViewBag.Shop = shop;
            if (SiteUserDetails.LoggedInUser != null)
                ViewBag.UserId = SiteUserDetails.LoggedInUser.Id;
            else
                ViewBag.UserId = 0;
            return View(HubHandler.GetHubListing(1, 10, "desc", "CreatedOn", "", null, user.UserID));
        }


        [SkipAuthentication]
        public JsonResult _Hubs(int page_no, int per_page_result, string sortOrder, string sortColumn, string search, int? TopicID, int? UserID, bool? IsManageHubs = false)
        {
            bool? AllHubs = IsManageHubs.HasValue && IsManageHubs.Value ? true : false;
            PagingResult<Hub> model = HubHandler.GetHubListing(page_no, per_page_result, sortOrder, sortColumn, search, TopicID, UserID, AllHubs);
            string view = (IsManageHubs.HasValue && IsManageHubs.Value) ? RenderRazorViewToString("_ManageHubsList", model.List) : RenderRazorViewToString("_HubList", model.List);
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string> { view, model.TotalCount.ToString(), model.Message }
            }, JsonRequestBehavior.AllowGet);
        }

        [SkipAuthentication]
        public JsonResult FilterHubs()
        {
            string search=Request["Job-Keyword"];
            int topicid= Convert.ToInt32(Request["searchType"]);
            return _Hubs(1, 10, "Desc", "CreatedOn", search, topicid, null);
        }



        
    }
}
