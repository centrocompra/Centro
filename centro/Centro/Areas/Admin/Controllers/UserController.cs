using System;
using System.Collections.Generic;
using System.Web.Mvc;
using BusinessLayer.Classes;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Handler;
using BusinessLayer.Models.ViewModel;

namespace Centro.Areas.Admin.Controllers
{
    public class UserController : AdminBaseController
    {

        [AuthenticateUser]
        public ActionResult ManageUser()
        {
            ViewBag.UserActionList = CommonMethods.GetListFromEnum(typeof(ActionOnUser), "Select Option");
            var list = UsersHandler.UserPaging(1, 20, "CreatedOn", "Desc", "");
            return View(list);

        }

        public JsonResult UserPaging(int page_no, int per_page_result, string sortOrder, string sortColumn, string username)
        {
            var list = UsersHandler.UserPaging(page_no, per_page_result, sortColumn, sortOrder, username);
            string data = RenderRazorViewToString("_Users", list);
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
        public ActionResult AddEditUser(Int32? user_id)
        {
            User user = new User();
            ViewBag.EditUser = false;
            if (user_id.HasValue)
            {
                var result = UsersHandler.GetUserByID(user_id.Value);
                ViewBag.EditUser = true;
                user = result.Object;
                user.PasswordRecoveryEmailId = user.EmailId;
            }
            return View("AddUser", user);
        }

        [AuthenticateUser]
        [HttpPost]
        public JsonResult AddEditUser(User obj)
        {

            if (obj.UserID == 0)
            {
                
                //if (ModelState.IsValid)
                //{
                    obj.RegistrationIP = Request.UserHostAddress;
                    var result = UsersHandler.UserSignup(obj);
                    if (result.Status == ActionStatus.Successfull)
                    {
                        String verificationURL = "";
                        string code = Server.UrlEncode(Utility.EncryptString(obj.UserName));
                        if (Request.Url.Host.ToLower() == "localhost")
                            verificationURL = Request.Url.Scheme + "://" + Request.Url.Host + ":" + Request.Url.Port + "/Account/ActivateAccount?code=" + code;
                        else
                            verificationURL = Request.Url.Scheme + "://" + Request.Url.Host + "/Account/ActivateAccount?code=" + code;

                        EmailHandler.NormalUserRegistration(obj, verificationURL);

                        return Json(new ActionOutput
                        {
                            Status = ActionStatus.Successfull,
                            Message = result.Message,
                            Results = new List<string> 
                            { 
                                Url.Action("ManageUser", "User") 
                            }
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new ActionOutput
                        {
                            Status = ActionStatus.Error,
                            Message = result.Message
                        }, JsonRequestBehavior.AllowGet);
                    }
                //}
                //else
                //{

                //    return Json(new ActionOutput
                //    {
                //        Status = ActionStatus.Error,
                //        Message = "Some Error Occured Please try later."
                //    }, JsonRequestBehavior.AllowGet);
                //}
            }
            else
            {
                obj.EmailId = obj.PasswordRecoveryEmailId;
                String verificationURL = "";
                string code = Server.UrlEncode(Utility.EncryptString(obj.UserName));

                if (Request.Url.Host.ToLower() == "localhost")
                    verificationURL = Request.Url.Scheme + "://" + Request.Url.Host + ":" + Request.Url.Port + "/Account/VerifyEmail?code=" + code;
                else
                    verificationURL = Request.Url.Scheme + "://" + Request.Url.Host + "/Account/VerifyEmail?code=" + code;

                var result = UsersHandler.UpdateUserDetails(obj, CentroUsers.LoggedInUser, verificationURL);
                return Json(new ActionOutput
                {
                    Status = result.Status,
                    Message = result.Message,
                    Results = new List<string> 
                    { 
                        Url.Action("ManageUser", "User") 
                    }
                }, JsonRequestBehavior.AllowGet);

            }


        }

        [AuthenticateUser]
        public JsonResult UpdateUser(UserAction obj)
        {
            var result = UsersHandler.UpdateUsers(obj, CentroUsers.LoggedInUser);
            return Json(new ActionOutput 
            { 
                Status = result.Status,
                Message = result.Message, 
                Results = new List<string> 
                { 
                    Url.Action("ManageUser") 
                } 
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
