using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Classes;
using System.Web.Security;
using System.Web.Script.Serialization;
using BusinessLayer.Handler;
using BusinessLayer.Models.DataModel;
using Microsoft.Web.WebPages.OAuth;
using DotNetOpenAuth.AspNet;

namespace Centro.Controllers
{
    public class FrontEndBaseController : BaseController
    {
        //protected SiteUserDetails CentroUsers { get; set; }
        protected SelectedTabs SelectedTabs { get; set; }

        /** 
      * Purpose  :   executed before any other method is executed and validates the user authorization
      * Inputs   :   filter_context – details of the current context
      */
        protected override void OnAuthorization(AuthorizationContext filter_context)
        {
            if (Request.RawUrl.ToLower().Contains("www.")) filter_context.Result = RedirectPermanent(Request.RawUrl.ToLower().Replace("www.", ""));
            HttpCookie auth_cookie = Request.Cookies[Cookies.UserCookie];

            SiteURL.URL = Request.Url.Scheme + "://" + Request.Url.Host + (Request.Url.Port == 80 ? "/" : ":" + Request.Url.Port + "/");


            #region Fetch url domain name
            //string subdomain = string.Empty;
            //if (System.Configuration.ConfigurationManager.AppSettings["Server"].ToString() == "Localhost") subdomain = RouteData.Values["domain"].ToString();
            //else subdomain = CommonFunctions.GetSubDomain(Request.Url.Host);
            #endregion

            #region If SelectedTabs is null
            if (SelectedTabs == null)
            {
                SelectedTabs = new SelectedTabs();
            }
            #endregion

            #region If auth cookie is present
            if (auth_cookie != null)
            {
                #region If CentroUsers is null
                if (SiteUserDetails == null)
                {
                    FormsAuthenticationTicket auth_ticket = FormsAuthentication.Decrypt(auth_cookie.Value);
                    SiteUserDetails = new JavaScriptSerializer().Deserialize<SiteUserDetails>(auth_ticket.UserData);
                    //System.Web.HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(new FormsIdentity(auth_ticket), null);
                    // Checking for saved cart
                    HttpCookie cart_cookie = Request.Cookies[Cookies.CartCookie];
                    if (cart_cookie == null)
                    {
                        var cart = CartHandler.GetCart(SiteUserDetails.LoggedInUser.Id).Object;
                        if (cart != null)
                        {
                            CreateCustomCookie(Cookies.CartCookie, false, cart.CartData, 120);
                        }
                    }
                }
                #endregion
                #region IF loggedin user is a member
                //if (SiteUserDetails.LoggedInUser.UserRole == UserRole.Buyer
                //    && filter_context.ActionDescriptor.ControllerDescriptor.ControllerName == "Home"
                //    && filter_context.ActionDescriptor.ActionName == "Index")
                //{
                //    //filter_context.Result = RedirectToAction("EditProfile", "User");
                //    filter_context.Result = RedirectToAction("Index", "Home");
                //}
                #endregion

                #region If loggedin user is a super admin
                else if (SiteUserDetails.LoggedInUser.UserRole == UserRole.Administrator && !filter_context.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(Adminstrator), false).Any())
                {
                    if (!filter_context.ActionDescriptor.GetCustomAttributes(typeof(AllowAdmin), false).Any())
                    {
                        filter_context.Result = RedirectToAction("Home", "Admin");
                    }

                }
                #endregion

                ViewBag.CentroUsers = SiteUserDetails;
            }
            #endregion

            #region if authorization cookie is not present and the action method being called is not marked with the [SkipAuthentication] attribute
            else if (!filter_context.ActionDescriptor.GetCustomAttributes(typeof(SkipAuthentication), false).Any())
            {
                if (Request.IsAjaxRequest()) filter_context.Result = Json(new ActionOutput { Results = new List<string> { Url.Action("Signin", "Home") }, Status = ActionStatus.Error }, JsonRequestBehavior.AllowGet);
                else
                    filter_context.Result = RedirectToAction("Signin", "Home");
            }
            #endregion

            #region if authorization cookie is not present and the action method being called is marked with the [SkipAuthentication] attribute
            else
            {
                SiteUserDetails = new SiteUserDetails();
                ViewBag.CentroUsers = SiteUserDetails;
            }
            #endregion
        }

        /** 
        * Purpose  :   created a custom authorization cookie
        * Inputs   :   user_name – username of the loggedin user
        *              is_persistent – if the cookie is to be saved after the browser closed
         *             custom_data – the additional details of the logged in user
        */
        protected void CreateCustomAuthorisationCookie(String user_name, Boolean is_persistent, String custom_data)
        {
            FormsAuthenticationTicket auth_ticket =
                new FormsAuthenticationTicket(
                    1, user_name,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(30),
                    is_persistent, custom_data, ""
                );

            String encrypted_ticket_ud = FormsAuthentication.Encrypt(auth_ticket);
            HttpCookie auth_cookie_ud = new HttpCookie(Cookies.UserCookie, encrypted_ticket_ud);
            if (is_persistent) auth_cookie_ud.Expires = auth_ticket.Expiration;
            System.Web.HttpContext.Current.Response.Cookies.Add(auth_cookie_ud);

            //CreateMemberCookie(user_name);
        }

        

        /// <summary>
        /// This will be used to update custom authorization cookie
        /// </summary>
        /// <param name="current_user"></param>
        protected void UpdateCustomAuthorisationCookie(UserDetails loggedin_user)
        {
            loggedin_user.ShopDetails = null;
            loggedin_user.ShopPictures = null;
            HttpCookie auth_cookie = Request.Cookies[Cookies.UserCookie];

            if (auth_cookie != null)
            {
                SiteUserDetails.LoggedInUser = loggedin_user;

                if (SiteUserDetails.LoggedInUser.Id != SiteUserDetails.LoggedInUser.Id) SiteUserDetails.LoginText = loggedin_user.Username;
                else SiteUserDetails.LoginText = SiteUserDetails.LoggedInUser.Username;

                FormsAuthenticationTicket authTicketExt = FormsAuthentication.Decrypt(auth_cookie.Value);

                FormsAuthenticationTicket auth_ticket =
                new FormsAuthenticationTicket(
                    1, authTicketExt.Name,
                    authTicketExt.IssueDate,
                    authTicketExt.Expiration,
                    authTicketExt.IsPersistent, new JavaScriptSerializer().Serialize(SiteUserDetails), String.Empty
                );

                String encryptedTicket = FormsAuthentication.Encrypt(auth_ticket);

                auth_cookie = new HttpCookie(Cookies.UserCookie, encryptedTicket);
                if (authTicketExt.IsPersistent) auth_cookie.Expires = auth_ticket.Expiration;
                System.Web.HttpContext.Current.Response.Cookies.Add(auth_cookie);
            }

        }

        /// <summary>
        /// This will be used to update user cookie.
        /// </summary>
        public void UpdateMemberCookie()
        {
            /*
            var user = UserHandler.MemberById(CentroUsers.LoggedInUser.MemberID).Object;
            if (user != null)
            {
                var user_det = new MemberDetails
                {
                    MemberEmail = user.email,
                    Login = user.login,
                    MemberID = user.id,
                    Tokenbalance = user.tokensbalance,
                    AvatarImageID = user.avatar_id_image,
                    AvatarImage = user.avatar_image,
                    MemberAccessType = user.MemberRole,
                    Gender = user.gender
                };

                var Centro_users = new SiteUserDetails
                {
                    IsAuthenticated = true,
                    CurrentUser = user_det,
                    LoggedInUser = user_det,
                    LoginText = user_det.Login
                };
                CreateCustomAuthorisationCookie(user.email, false, new JavaScriptSerializer().Serialize(Centro_users));
            }
            */
        }

        /// <summary>
        /// Used to log out from the site
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            HttpCookie auth_cookie = Request.Cookies[Cookies.UserCookie];
            auth_cookie.Expires = DateTime.Now.AddDays(-30);
            Response.Cookies.Add(auth_cookie);

            HttpCookie cart_cookie = Request.Cookies[Cookies.CartCookie];
            if (cart_cookie != null)
            {
                cart_cookie.Expires = DateTime.Now.AddDays(-30);
                Response.Cookies.Add(cart_cookie);
            }


            HttpCookie payment_cookie = Request.Cookies[Cookies.PaymentTempCookie];
            if (payment_cookie != null)
            {
                payment_cookie.Expires = DateTime.Now.AddDays(-30);
                Response.Cookies.Add(payment_cookie);
            }

            return Redirect(Url.Action(CentroDefaults.DefaultPage, "Home"));
        }

        /// <summary>
        /// Render Sign In Page For the Application
        /// </summary>
        /// <returns></returns>
        [SkipAuthentication]
        public ActionResult Signin(string status)
        {
            Int32 count_value = GetCaptchaCookie();
            if (count_value >= 3)
                ViewBag.ShowCaptcha = true;
            else
                ViewBag.ShowCaptcha = false;
            ViewBag.Status = status;
            //ModelState["UniqueUsername"].Errors.Clear();
            //ModelState.Remove("UniqueUsername");

            return View(new User());
        }

        /// <summary>
        /// Render Sign Up Page for the application
        /// </summary>
        /// <returns></returns>
        [SkipAuthentication]
        public ActionResult Signup()
        {
            return View("SignUp", new User());
        }

        /// <summary>
        /// Handle User Login Request
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        [SkipAuthentication]
        public JsonResult Login(User obj)
        {
            string login = obj.EmailOrUsername;
            string password = Utility.EncryptString(obj.Password);
            string ip = Request.ServerVariables["REMOTE_ADDR"];
            bool persistent = Request["persistent"] != null ? true : false;

            Int32 captcha_count = GetCaptchaCookie();
            bool countInvalidattempt = false;
            if (captcha_count - Constants.InvalidLoginAttemptForCaptcha >= 0)
                countInvalidattempt = true;
            // Login the activated user
            var result = UsersHandler.UserLogin(login, password, ip, countInvalidattempt);
            var user = result.Object;
            ViewBag.ShowCaptcha = false;
            if (user != null)
            {
                //Delete Captcha Cookie
                CaptchaCookie(false);
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
                    UserLocation = user.UserLocation,
                    FirstName=user.FirstName,
                    LastName=user.LastName
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

                var url = Request.Url;
                string redirectTo, section;
                /* Checking User Role and redirecting accordingly */
                if ((int)user.RoleId == (int)UserRole.Seller)
                    section = CentroDefaults.SellerSection;
                else
                    section = CentroDefaults.BuyerSection;

                if(obj.CreatedOn.Year==DateTime.Now.Year && obj.CreatedOn.Month==DateTime.Now.Month && obj.CreatedOn.Day==DateTime.Now.Day)
                    redirectTo = "http://" + url.Host + ":" + url.Port ;//+ "/User/EditProfile";
                else
                    redirectTo = "http://" + url.Host + ":" + url.Port;// +"/User/Activities";

                // check for return url
                HttpCookie returnUrlCookie = Request.Cookies[Cookies.ReturnUrlCookie];
                if (returnUrlCookie != null)
                {
                    redirectTo = returnUrlCookie.Value;
                    UpdateCustomCookie(Cookies.ReturnUrlCookie, false, "", -10);
                }
                return Json(new ActionOutput { Status = ActionStatus.Successfull, Message = result.Message, Results = new List<string> { redirectTo } }, JsonRequestBehavior.AllowGet);
            }
            Int32 count = CaptchaCookie(true);
            if (count == Constants.InvalidLoginAttemptForCaptcha)
            {
                ViewBag.ShowCaptcha = true;
                return Json(new ActionOutput { Status = result.Status, Message = "Please verify your identity.", ID = Constants.InvalidLoginAttemptForCaptcha }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new ActionOutput { Status = result.Status, Message = result.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [SkipAuthentication]
        public ActionResult Error()
        {
            return View("Error");
        }

        protected override string GetLoggedInUserJson()
        {

            return new JavaScriptSerializer().Serialize(SiteUserDetails);
        }

        /// <summary>
        /// Used to Create cookie in case user attempts in valid login
        /// </summary>
        /// <param name="Create"></param>
        /// <returns></returns>
        protected Int32 CaptchaCookie(bool Create)
        {
            if (Create)
            {
                HttpCookie captcha_cookie = Request.Cookies[Cookies.CaptchaCookie];
                Int32 count_attempt = 0;
                if (captcha_cookie == null)
                {
                    captcha_cookie = new HttpCookie(Cookies.CaptchaCookie, Utility.EncryptString("1"));
                    captcha_cookie.Expires = DateTime.Now.AddMinutes(10);
                    System.Web.HttpContext.Current.Response.Cookies.Add(captcha_cookie);
                    count_attempt = 1;
                }
                else
                {
                    String count_value = Utility.DecryptString(captcha_cookie.Value);
                    captcha_cookie.Value = Utility.EncryptString((Convert.ToInt32(count_value) + 1).ToString());
                    captcha_cookie.Expires = DateTime.Now.AddMinutes(10);
                    System.Web.HttpContext.Current.Response.Cookies.Add(captcha_cookie);
                    count_attempt = Convert.ToInt32(count_value) + 1;
                }
                return count_attempt;
            }
            else
            {
                HttpCookie captcha_cookie = Request.Cookies[Cookies.CaptchaCookie];
                if (captcha_cookie != null)
                {
                    captcha_cookie.Expires = DateTime.Now.AddDays(-30);
                    System.Web.HttpContext.Current.Response.Cookies.Add(captcha_cookie);
                }
                return 0;
            }
        }

        /// <summary>
        /// get invalid login count
        /// </summary>
        /// <returns></returns>
        protected Int32 GetCaptchaCookie()
        {

            HttpCookie captcha_cookie = Request.Cookies[Cookies.CaptchaCookie];
            Int32 count_attempt = 0;
            if (captcha_cookie == null)
            {
                count_attempt = 0;
            }
            else
            {
                String count_value = Utility.DecryptString(captcha_cookie.Value);
                count_attempt = Convert.ToInt32(count_value);
            }
            return count_attempt;

        }

        /// <summary>
        /// Tpo count the view of contest and hubs
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="CookieName"></param>
        [SkipAuthentication]
        public void SetPageView(int ID, string CookieName)
        {
            var productId = 1;
            if (Request.Cookies[CookieName] != null)
            {
                if (Request.Cookies[CookieName][string.Format("pId_{0}", productId)] == null)
                {
                    HttpCookie cookie = (HttpCookie)Request.Cookies[CookieName];
                    cookie[string.Format("pId_{0}", productId)] = "1";
                    cookie.Expires = DateTime.Now.AddDays(1);
                    Response.Cookies.Add(cookie);
                    if (CookieName == Cookies.ViewedPage)
                        ContestHandler.SetPageView(ID);
                    else if (CookieName == Cookies.ViewHubPage)
                        HubHandler.SetHubPageView(ID);
                }
            }
            else
            {
                HttpCookie cookie = new HttpCookie(CookieName);
                cookie[string.Format("pId_{0}", productId)] = "1";
                cookie.Expires = DateTime.Now.AddDays(1);
                Response.Cookies.Add(cookie);
                if (CookieName == Cookies.ViewedPage)
                    ContestHandler.SetPageView(ID);
                else if (CookieName == Cookies.ViewHubPage)
                    HubHandler.SetHubPageView(ID);
            }
        }

        [SkipAuthentication]
        public JsonResult _Registration()
        {
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string>
                {
                    RenderRazorViewToString("_Registration", new User())
                }
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, SkipAuthentication]
        public JsonResult AddUpdateUser(User model)
        {
            // check for verification doc
            HttpCookie cookie = Request.Cookies[Cookies.TempFileAttachments];
            if (cookie != null)
            {
                FileAttachmentViewModel file = new JavaScriptSerializer().Deserialize<FileAttachmentViewModel>(cookie.Value);
                model.Attachments = file;
            }
            model.RegistrationIP = Request.UserHostAddress;
            model.AuthenticatedVia = model.UserID == 0 ? (int)AuthenticationFrom.Website : model.AuthenticatedVia;
            string SiteURL = "http://" + Request.Url.Host + (Request.Url.Port == 80 ? "" : ":" + Request.Url.Port);
            return Json(UsersHandler.AddUpdateUser(model, SiteURL), JsonRequestBehavior.AllowGet);
        }

       
        [SkipAuthentication]
        public ActionResult VerifyEmail(string VerificationCode)
        {
            //string decodedUrl = HttpUtility.UrlDecode(VerificationCode);
            var output = UsersHandler.VerifyEmail(VerificationCode.Replace("-", "+"));
            TempData["Status"] = (int)output.Status;
            TempData["Message"] = output.Message;
            return RedirectToAction("Index", "Home");
        }
       

        [SkipAuthentication]
        public JsonResult _RegistrationAsCompany()
        {
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string>
                {
                    RenderRazorViewToString("_RegistrationAsCompany", new User())
                }
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, SkipAuthentication, AjaxOnly]
        public JsonResult UniqueUsername(string Username)
        {

            return Json(Utility.UniqueUsername(Username));
        }

        [HttpPost, SkipAuthentication, AjaxOnly]
        public JsonResult UniqueEmail(string Email)
        {
            int? currentuserid = SiteUserDetails.LoggedInUser == null ? null : (int?)SiteUserDetails.LoggedInUser.Id;

            return Json(Utility.UniqueEmail(Email, currentuserid));
        }

        [SkipAuthentication]
        public ActionResult LoginViaFB()
        {

            return PartialView("_facebookLogin", OAuthWebSecurity.RegisteredClientData);
        }

        [AllowAnonymous]
        [SkipAuthentication]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {

            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                User user = new User();
                if (result.Provider.ToLower() == "facebook")
                {
                    user.FirstName = result.ExtraData["first_name"];
                    user.LastName = result.ExtraData["last_name"];
                    user.EmailId = result.ExtraData["email"];
                    user.Password = "FACEBOOK";
                    Random rnd = new Random();
                    user.UserName = result.UserName;// +rnd.Next(1, 100);
                    if (result.ExtraData["gender"].ToLower() == "male")
                        user.Gender = (int)Gender.Male;
                    else if (result.ExtraData["gender"].ToLower() == "female")
                        user.Gender = (int)Gender.Female;
                    user.AuthenticatedVia = (int)AuthenticationFrom.Facebook;
                }
                else if (result.Provider.ToLower() == "twitter")
                {
                    user.FirstName = result.UserName;
                    user.LastName = "";
                    user.EmailId = "";
                    user.Password = "TWITTER";
                    user.UserName = result.UserName;
                    user.Gender = (int)Gender.Male;
                    user.AuthenticatedVia = (int)AuthenticationFrom.Twitter;
                }
                user.AuthenticationID = result.ProviderUserId;
                user.RegistrationIP = Request.UserHostAddress;

                var check_user = UsersHandler.GetUserByAuthenticationID(user);

                User resultObj;
                if (check_user.Object == null)
                {
                    resultObj = UsersHandler.UserSignup(user).Object;
                }
                else
                {
                    resultObj = check_user.Object;
                }

                UserDetails userDetails = new UserDetails();
                userDetails.FirstName = resultObj.FirstName;
                userDetails.LastName = resultObj.LastName;
                userDetails.Email = resultObj.EmailId;
                userDetails.Id = resultObj.UserID;
                userDetails.Username = resultObj.UserName;

                var site_users = new SiteUserDetails
                {
                    IsAuthenticated = true,
                    CurrentUser = userDetails,
                    LoggedInUser = userDetails,
                    LoginText = userDetails.Username,
                    SiteURL = "http://" + Request.Url.Host + (Request.Url.Port == 80 ? "" : ":" + Request.Url.Port),
                };
                /* Creating Custom Authorisation Cookie */
                CreateCustomAuthorisationCookie(userDetails.Email, false, new JavaScriptSerializer().Serialize(site_users));

                string redirectTo = site_users.SiteURL;

                // check for return url
                HttpCookie returnUrlCookie = Request.Cookies[Cookies.ReturnUrlCookie];
                if (returnUrlCookie != null)
                {
                    redirectTo = returnUrlCookie.Value;
                    UpdateCustomCookie(Cookies.ReturnUrlCookie, false, "", -10);
                }
                return RedirectToLocal(returnUrl);
            }

        }

        /// <summary>
        /// Save External login
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [SkipAuthentication]
        public ActionResult ExternalLogin(string provider)
        {

            string s = null;
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = s }));
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [AjaxOnly, SkipAuthentication]
        public JsonResult UploadLoginFile(HttpPostedFileBase loginfile)
        {
            if (loginfile.ContentLength > 10240000)
            {
                if ((Request.Browser).Browser == "IE")
                    return Json(new ActionOutput { Status = ActionStatus.Error, Message = "File size must be less than 10 MB" }, "text/plain", JsonRequestBehavior.AllowGet);
                else
                    return Json(new ActionOutput { Status = ActionStatus.Error, Message = "File size must be less than 10 MB" }, JsonRequestBehavior.AllowGet);
            }
            string newFilename = Guid.NewGuid().ToString() + "." + loginfile.FileName.Substring(loginfile.FileName.LastIndexOf(".") + 1);
            string fileExt = System.IO.Path.GetExtension(loginfile.FileName).ToLower();
            ActionOutput result;
            // Saving Image to Temp Folder
            string temp_folder = Server.MapPath("~/Temp/LoginFiles/");
            if (!System.IO.Directory.Exists(temp_folder))
                System.IO.Directory.CreateDirectory(temp_folder);
            loginfile.SaveAs(temp_folder + newFilename);
            #region Temp Cookie
            // Check if cookie already exists
            FileAttachmentViewModel temp_file;
            HttpCookie attachment_temp_cookie = Request.Cookies[Cookies.TempFileAttachments];

            temp_file = new FileAttachmentViewModel();
            temp_file.DisplayName = loginfile.FileName;
            temp_file.MimeType = loginfile.ContentType;
            temp_file.SavedName = newFilename;
            temp_file.SizeInBytes = loginfile.ContentLength;
            temp_file.SizeInKB = loginfile.ContentLength / 1024;
            temp_file.SizeInMB = loginfile.ContentLength / (1024 * 1024);
            // Creating Temp cookie for User product file
            CreateCustomCookie(Cookies.TempFileAttachments, false, new JavaScriptSerializer().Serialize(temp_file));
            #endregion
            result = new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { newFilename, loginfile.FileName } };

            if ((Request.Browser).Browser == "IE")
                return Json(result, "text/plain", JsonRequestBehavior.AllowGet);
            else
                return Json(result, JsonRequestBehavior.AllowGet);
        }
        #region helper
        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {

                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);

            }
        }
        #endregion
    }
}
