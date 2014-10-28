using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Centro.Controllers;
using System.Web.Security;
using System.Web.Script.Serialization;
using BusinessLayer.Classes;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Handler;
using BusinessLayer.Models.DataModel;

namespace Centro.Areas.Admin.Controllers
{
    [NoCache]
    public class AdminBaseController : BaseController
    {
        protected SiteUserDetails CentroUsers { get; set; }

        //protected LoggedInAdminUserDetails LoggedInUser { get; set; }

        protected override void OnAuthorization(AuthorizationContext filter_Context)
        {
            if (Request.RawUrl.ToLower().Contains("www.")) filter_Context.Result = RedirectPermanent(Request.RawUrl.ToLower().Replace("www.", ""));
            HttpCookie auth_cookie = Request.Cookies[Cookies.AdminCookie];

            if (auth_cookie != null && !String.IsNullOrEmpty(auth_cookie.Value))
            {
                if (CentroUsers == null)
                {
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(auth_cookie.Value);
                    CentroUsers = new JavaScriptSerializer().Deserialize<SiteUserDetails>(authTicket.UserData);
                }
            }
            //if authorization cookie is not present and the action method being called is marked with the [AuthenticateUser] attribute
            else if (filter_Context.ActionDescriptor.GetCustomAttributes(typeof(AuthenticateUser), false).Count() > 0)
            {
                ViewBag.LoggedIn = false;
                CentroUsers = new SiteUserDetails();
                filter_Context.Result = RedirectToAction("LogIn", "Home");
            }
            else
            {
                ViewBag.LoggedIn = false;
                CentroUsers = new SiteUserDetails();
            }

            ViewBag.CentroUsers = CentroUsers;

        }

        protected void CreateCustomAuthorisationCookie(String user_name, Boolean is_persistent, String custom_data)
        {
            FormsAuthenticationTicket auth_ticket =
                new FormsAuthenticationTicket(
                    1, user_name,
                    DateTime.Now,
                    DateTime.Now.AddDays(7),
                    is_persistent, custom_data, ""
                );

            String encrypted_ticket_ud = FormsAuthentication.Encrypt(auth_ticket);
            HttpCookie auth_cookie_ud = new HttpCookie(Cookies.AdminCookie, encrypted_ticket_ud);
            if (is_persistent) auth_cookie_ud.Expires = auth_ticket.Expiration;
            System.Web.HttpContext.Current.Response.Cookies.Add(auth_cookie_ud);
        }

        protected override string GetLoggedInUserJson()
        {

            return new JavaScriptSerializer().Serialize(CentroUsers);
        }

        [HttpPost]
        public JsonResult SignIn(User obj)
        {
            string login = obj.EmailOrUsername;
            string password = Utility.EncryptString(obj.Password);
            string ip = Request.ServerVariables["REMOTE_ADDR"];
            bool persistent = Request["persistent"] != null ? true : false;
            // Login the activated user
            var result = UsersHandler.AdminLogin(login, password, ip);
            var user = result.Object;
            if (user != null)
            {
                var user_det = new UserDetails
                {
                    Email = user.EmailId,
                    Username = user.UserName,
                    Id = user.UserID,
                    UserRole = (UserRole)user.RoleId,
                    Gender = (Gender)user.Gender,
                    ShopDetails = user.ShopDetails,
                    ShopPictures = user.ShopPictures
                };

                var Centro_users = new SiteUserDetails
                {
                    IsAuthenticated = true,
                    CurrentUser = user_det,
                    LoggedInUser = user_det,
                    LoginText = user_det.Username
                };
                /* Creating Custom Authorisation Cookie */
                CreateCustomAuthorisationCookie(user_det.Email, persistent, new JavaScriptSerializer().Serialize(Centro_users));


                return Json(new ActionOutput { Status = ActionStatus.Successfull, Message = result.Message, Results = new List<string> { Url.Action("Dashboard", "Home") } }, JsonRequestBehavior.AllowGet);
            }
            return Json(new ActionOutput { Status = ActionStatus.Error, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Used to log out from the site
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            HttpCookie auth_cookie = Request.Cookies[Cookies.AdminCookie];
            auth_cookie.Expires = DateTime.Now.AddDays(-30);
            Response.Cookies.Add(auth_cookie);

            //auth_cookie = Request.Cookies[Cookies.UserCookie];
            //auth_cookie.Expires = DateTime.Now.AddDays(-30);
            //Response.Cookies.Add(auth_cookie);
            return Redirect(Url.Action("LogIn", "Home"));
        }
    }

}

