using System;
using System.Web.Mvc;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Classes;
using BusinessLayer.Handler;
using BusinessLayer.Models.ViewModel;
using System.Web.Script.Serialization;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;

namespace Centro.Controllers
{
    public class AccountController : FrontEndBaseController
    {
        /// <summary>
        /// To Render Signup page
        /// </summary>
        [SkipAuthentication]
        public ActionResult Index()
        {
            return RedirectToAction("SignUp", "Home");
        }

        /// <summary>
        /// This method will be used for user registration on the site.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [SkipAuthentication]
        [HttpPost]
        public JsonResult SignUp(User obj)
        {
            obj.Password = Utility.EncryptString(obj.Password);
            obj.EmailOrUsername = "";
            obj.PasswordRecoveryEmailId = "";
            obj.RegistrationIP = Request.UserHostAddress;
            obj.AuthenticatedVia = (int)AuthenticationFrom.Website;
            //if (ModelState.IsValid)
            //{
                var result = UsersHandler.UserSignup(obj);
                if (result.Status == ActionStatus.Successfull)
                {
                    String verificationURL = "";
                    string code = Server.UrlEncode(Utility.EncryptString(obj.UserName));
                    if (Request.Url.Host.ToLower() == "localhost")
                        verificationURL = "http://" + Request.Url.Host + ":" + Request.Url.Port + "/Account/ActivateAccount?code=" + code;
                    else
                        verificationURL = "http://" + Request.Url.Host + "/Account/ActivateAccount?code=" + code;

                    EmailHandler.NormalUserRegistration(obj, verificationURL);

                    return Json(new ActionOutput { Status = ActionStatus.Successfull, ID=result.Object.UserID, Message = result.Message }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new ActionOutput { Status = ActionStatus.Error, Message = result.Message }, JsonRequestBehavior.AllowGet);
                }
           // }
           // else
           // {
            
           //     return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Invalid data" }, JsonRequestBehavior.AllowGet);
          //  }
        }

        /// <summary>
        /// To Resend Verification Email
        /// </summary>
        [SkipAuthentication]
        public JsonResult ResendVerificationEmail(int id)
        {
            var user=UsersHandler.GetUserByID(id);
            String verificationURL = "";
            string code = Server.UrlEncode(Utility.EncryptString(user.Object.UserName));
            if (Request.Url.Host.ToLower() == "localhost")
                verificationURL = "http://" + Request.Url.Host + ":" + Request.Url.Port + "/Account/ActivateAccount?code=" + code;
            else
                verificationURL = "http://" + Request.Url.Host + "/Account/ActivateAccount?code=" + code;
            EmailHandler.NormalUserRegistration(user.Object, verificationURL);
            return Json(new ActionOutput { Status = ActionStatus.Successfull, Message = "Verification email has been resent." }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To Activate the account
        /// </summary>
        [SkipAuthentication]
        public ActionResult ActivateAccount(String code)
        {

            ActionOutput<User> result = UsersHandler.ActivateAccount(code);
            if (result.Status == ActionStatus.Successfull)
            {
                #region Skip as per client's request
                // Login the activated user

                /*
                var user = UsersHandler.GetUserByID((Int32)result.ID, Request.UserHostAddress).Object;
                var res = UsersHandler.UserLogin(result.Object.UserName, result.Object.Password, Request.UserHostAddress, false);
                var user = res.Object;

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
                    // Creating Custom Authorisation Cookie 
                    CreateCustomAuthorisationCookie(user_det.Email, false, new JavaScriptSerializer().Serialize(Centro_users));
                    var url = Request.Url;
                    string redirectTo, section;
                    //Checking User Role and redirecting accordingly 
                    if ((int)user.RoleId == (int)UserRole.Administrator)
                        section = CentroDefaults.AdminSection;
                    else if ((int)user.RoleId == (int)UserRole.Seller)
                        section = CentroDefaults.SellerSection;
                    else
                        section = CentroDefaults.BuyerSection;
                    redirectTo = "http://" + url.Host + ":" + url.Port + "/" + section;

                    //return View(CentroDefaults.DefaultPage, "Home");
                    return RedirectToAction("Index", "User");
                    

                    return RedirectToAction("Index", "User");
                   // return View(CentroDefaults.DefaultPage, "Home");


                }
                else
                {
                    // Send to Error Page with Activation Error Message
                    return RedirectToAction("Error", "Home", new { Error = "Invalid Activation Code!!!" });
                }
                */
                #endregion
                return RedirectToAction("Signin", "Home", new { status = "verified" });
            }
            else
            {

                // Send to Error Page with Activation Error Message
                return RedirectToAction("Error", "Home", new { Error = "Invalid Activation Code!!!" });

            }

        }

        /// <summary>
        /// To list external logins
        /// </summary>
        [SkipAuthentication]
        public ActionResult ExternalLoginsList()
        {

            return PartialView("_ExternalFacebookPartial", OAuthWebSecurity.RegisteredClientData);
        }

        /// <summary>
        /// login via FB
        /// </summary>
        [SkipAuthentication]
        public ActionResult LoginViaFB()
        {

            return PartialView("_facebookLogin", OAuthWebSecurity.RegisteredClientData);
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
                user.FirstName = result.ExtraData["first_name"];
                user.LastName = result.ExtraData["last_name"];
                user.EmailId = result.ExtraData["email"];
                Random rnd = new Random();
                user.UserName = result.UserName;// +rnd.Next(1, 100);
                if (result.ExtraData["gender"].ToLower() == "male")
                    user.Gender = (int)Gender.Male;
                else if (result.ExtraData["gender"].ToLower() == "female")
                    user.Gender = (int)Gender.Female;
                else
                    user.Gender = (int)Gender.Other;
                user.AuthenticatedVia = (int)AuthenticationFrom.Facebook;
                user.AuthenticationID = result.ProviderUserId;
                user.RegistrationIP = Request.UserHostAddress;
                var check_user = UsersHandler.GetUserByAuthenticationID(user, Request.UserHostAddress);
                if (check_user.Object == null)
                {
                    var resultObj = UsersHandler.UserSignup(user);
                    if (resultObj.Status == ActionStatus.Successfull)
                    {
                        var result_obj = UsersHandler.GetUserByAuthenticationID(user, Request.UserHostAddress);
                        var user_obj = result_obj.Object;
                        if (user_obj != null)
                        {
                            var user_det = new UserDetails
                            {
                                Email = user_obj.EmailId,
                                Username = user_obj.UserName,
                                Id = user_obj.UserID,
                                UserRole = (UserRole)user_obj.RoleId,
                                Gender = (Gender)user_obj.Gender,
                                ShopDetails = user_obj.ShopDetails,
                                ShopPictures = user_obj.ShopPictures
                            };

                            var Centro_users = new SiteUserDetails
                            {
                                IsAuthenticated = true,
                                CurrentUser = user_det,
                                LoggedInUser = user_det,
                                LoginText = user_det.Username
                            };
                            /* Creating Custom Authorisation Cookie */
                            CreateCustomAuthorisationCookie(user_det.Email, false, new JavaScriptSerializer().Serialize(Centro_users));

                            var url = Request.Url;
                            string redirectTo;
                            if ((int)user_obj.RoleId == (int)UserRole.Seller)
                                redirectTo = Url.Action("Index", "Seller");
                            else
                                redirectTo = Url.Action("Index", "User");
                            returnUrl = redirectTo;
                        }
                    }
                }
                else
                {
                    var user_obj = check_user.Object;
                   
                        var user_det = new UserDetails
                        {
                            Email = user_obj.EmailId,
                            Username = user_obj.UserName,
                            Id = user_obj.UserID,
                            UserRole = (UserRole)user_obj.RoleId,
                            Gender = (Gender)user_obj.Gender,
                            ShopDetails = user_obj.ShopDetails,
                            ShopPictures = user_obj.ShopPictures
                        };

                        var Centro_users = new SiteUserDetails
                        {
                            IsAuthenticated = true,
                            CurrentUser = user_det,
                            LoggedInUser = user_det,
                            LoginText = user_det.Username
                        };
                        /* Creating Custom Authorisation Cookie */
                        CreateCustomAuthorisationCookie(user_det.Email, false, new JavaScriptSerializer().Serialize(Centro_users));

                        var url = Request.Url;
                        string redirectTo;
                        /* Checking User Role and redirecting accordingly */
                        if ((int)user_obj.RoleId == (int)UserRole.Seller)
                            redirectTo = Url.Action("Index", "Seller");
                        else
                            redirectTo = Url.Action("Index", "User");
                        
                        returnUrl = redirectTo;
                   
                }
                return RedirectToLocal(returnUrl);
            }

        }

        /// <summary>
        /// To Verify Email
        /// </summary>
        [SkipAuthentication]
        //public ActionResult VerifyEmail(String code)
        //{

        //    ActionOutput<User> result = UsersHandler.VerifyEmail(code);
        //    if (result.Status == ActionStatus.Successfull)
        //    {
        //        // Login the activated user
        //        var res = UsersHandler.UserLogin(result.Object.UserName, result.Object.Password, Request.UserHostAddress, false);
        //        var user = res.Object;
        //        if (user != null)
        //        {
        //            var user_det = new UserDetails
        //            {
        //                Email = user.EmailId,
        //                Username = user.UserName,
        //                Id = user.UserID,
        //                UserRole = (UserRole)user.RoleId,
        //                Gender = (Gender)user.Gender,
        //                ShopDetails = user.ShopDetails,
        //                ShopPictures = user.ShopPictures
        //            };

        //            var Centro_users = new SiteUserDetails
        //            {
        //                IsAuthenticated = true,
        //                CurrentUser = user_det,
        //                LoggedInUser = user_det,
        //                LoginText = user_det.Username
        //            };
        //            /* Creating Custom Authorisation Cookie */
        //            CreateCustomAuthorisationCookie(user_det.Email, false, new JavaScriptSerializer().Serialize(Centro_users));
        //            var url = Request.Url;
        //            string redirectTo, section;
        //            /* Checking User Role and redirecting accordingly */
        //            if ((int)user.RoleId == (int)UserRole.Administrator)
        //                section = CentroDefaults.AdminSection;
        //            else if ((int)user.RoleId == (int)UserRole.Seller)
        //                section = CentroDefaults.SellerSection;
        //            else
        //                section = CentroDefaults.BuyerSection;
        //            redirectTo = "http://" + url.Host + ":" + url.Port + "/" + section;
        //            return RedirectToAction("Index", "Home");
        //             //return View(CentroDefaults.DefaultPage, "Home");

        //        }
        //        else
        //        {
        //            // Send to Error Page with Activation Error Message
        //            return RedirectToAction("Error", "Home", new { Error = "Invalid Activation Code!!!" });
        //        }
        //    }
        //    else
        //    {

        //        // Send to Error Page with Activation Error Message
        //        return RedirectToAction("Error", "Home", new { Error = "Invalid Activation Code!!!" });

        //    }

        //}

        #region Helpers

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
