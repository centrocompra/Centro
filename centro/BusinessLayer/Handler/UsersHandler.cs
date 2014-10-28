using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Classes;
using System.Data.Objects;
using System.Web;
using System.Threading;

namespace BusinessLayer.Handler
{
    public class UsersHandler
    {
        /// <summary>
        /// this will be used to add updated user information
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        public static ActionOutput AddUpdateUser(User userModel, string SiteURL)
        {
            using (var entities = new CentroEntities())
            {
                var msg = string.Empty;
                var existingUser = entities.Users.FirstOrDefault(o => o.EmailId.Equals(userModel.EmailId.Trim(), StringComparison.CurrentCultureIgnoreCase)
                                                                            && o.AuthenticatedVia == userModel.AuthenticatedVia
                                                                            && o.UserID != userModel.UserID);
                User u = entities.Users.Where(m => m.EmailId.Equals(userModel.EmailId, StringComparison.InvariantCultureIgnoreCase)
                                                                            && m.AuthenticatedVia == userModel.AuthenticatedVia
                                                                            && m.UserID!=userModel.UserID).FirstOrDefault();
                if (u != null)
                {
                    return new ActionOutput { Status = ActionStatus.Error, Message = "Email Id already In Use." };
                }
                User u1 = entities.Users.Where(m => m.UserName.Equals(userModel.UserName, StringComparison.InvariantCultureIgnoreCase)
                                                        && m.AuthenticatedVia == userModel.AuthenticatedVia
                                                        && m.UserID != userModel.UserID).FirstOrDefault();
                if (u1 != null)
                {
                    return new ActionOutput { Status = ActionStatus.Error, Message = "Username already In Use." };
                }
                if (existingUser != null)
                {
                    return new ActionOutput
                    {
                        Status = ActionStatus.Error,
                        Message = "A user with this email already exist."
                    };
                }
                else
                {
                    existingUser = entities.Users.FirstOrDefault(o => o.UserID == userModel.UserID);

                    if (existingUser != null)
                    {
                        existingUser.FirstName = userModel.FirstName;
                        existingUser.LastName = userModel.LastName;
                        existingUser.RoleId = userModel.RoleId;
                        //existingUser.EmailId = userModel.Email;
                        existingUser.Gender = userModel.Gender;
                        existingUser.PhoneNumber = userModel.PhoneNumber;

                        msg = "The user has been updated successfully";
                    }
                    else
                    {
                        var att = new Attachment
                        {
                            CreatedOn = DateTime.Now,
                            DisplayName = userModel.Attachments.DisplayName,
                            MimeType = userModel.Attachments.MimeType,
                            SavedName = userModel.Attachments.SavedName,
                            SizeInBytes = userModel.Attachments.SizeInBytes,
                            SizeInKB = userModel.Attachments.SizeInKB,
                            SizeInMB = userModel.Attachments.SizeInMB,
                            MessageID = null
                        };

                        entities.Attachments.AddObject(att);

                        entities.Users.AddObject(new User
                        {
                            FirstName = userModel.FirstName,
                            LastName = userModel.LastName,
                            EmailId = userModel.EmailId,
                            RoleId = userModel.RoleId,
                            RegistrationIP = userModel.RegistrationIP,
                            IsVerified = false,
                            VerifiedOn = null,
                            AuthenticatedVia = 1,
                            Gender = userModel.Gender,
                            UserGUID = Guid.NewGuid(),
                            UserName = userModel.UserName,
                            CreatedOn = DateTime.Now,
                            Password = Utility.EncryptString(userModel.Password),
                            VerificationCode = Utility.EncryptString(userModel.EmailId),
                            Attachment = att,
                            DateofBirth = DateTime.Now,
                            LastLoginOn = DateTime.Now
                        });
                        userModel.VerificationCode = Utility.EncryptString(userModel.EmailId);
                        msg = "The user has been created successfully and an email has been sent for email verification.";
                        // Send Registration Email
                        UserRegistrationThreadObject emailTread = new UserRegistrationThreadObject();
                        emailTread.User = userModel;
                        emailTread.SiteURL = SiteURL;
                        emailTread.encodedURL = HttpUtility.UrlDecode(userModel.VerificationCode);
                        emailTread.Path = HttpContext.Current.Server.MapPath("~/EmailTemplates/");

                        Thread thread = new Thread(SendRegistrationEmail);
                        thread.Start(emailTread);

                    }

                    entities.SaveChanges();
                    if (userModel.UserID == 0)// move file from temp to original
                        Utility.MoveFile("~/Temp/LoginFiles/" + userModel.Attachments.SavedName, "~/RegistrationFiles/" + userModel.UserName + "/", userModel.Attachments.SavedName);
                    return new ActionOutput
                    {
                        Status = ActionStatus.Successfull,
                        Message = msg
                    };
                }
            }
        }

        private static void SendRegistrationEmail(object parameter)
        {
            UserRegistrationThreadObject threadObject = parameter as UserRegistrationThreadObject;
            EmailHandler.SendRegistrationEmail(threadObject.User, threadObject.Path, threadObject.SiteURL, threadObject.encodedURL);
        }

        public static ActionOutput<User> GetUserByAuthenticationID(User user)
        {
            using (var context = new CentroEntities())
            {
                user = context.Users.Where(m => m.UserName == user.UserName && m.AuthenticatedVia == user.AuthenticatedVia).FirstOrDefault();
                return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = user };
            }
        }

        /// <summary>
        /// Get the user by username, password and set the login history
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="IP"></param>
        /// <returns></returns>
        public static ActionOutput<User> UserLogin(string login, string password, string IP, bool countLoginAttempt)
        {
            using (var context = new CentroEntities())
            {
                User user;
                if (login.Contains('@'))
                    user = context.Users.Where(m => m.EmailId.Equals(login, StringComparison.InvariantCultureIgnoreCase)
                                                        && m.Password.Equals(password, StringComparison.InvariantCultureIgnoreCase)
                                                        && !m.IsDeleted).FirstOrDefault();
                else
                    user = context.Users.Where(m => m.UserName.Equals(login, StringComparison.InvariantCultureIgnoreCase)
                                                        && m.Password.Equals(password, StringComparison.InvariantCultureIgnoreCase)
                                                        && !m.IsDeleted).FirstOrDefault();
                if (user != null)
                {
                    // Check weather the user is verified or not.
                    if (!user.IsVerified)
                        return new ActionOutput<User> { Status = ActionStatus.Error, Object = null, Message = string.Format("You have not verified your email yet. Please click <a href=\"javascript:;\" onclick=\"Resend({0}, this)\">here</a> to resend the verification email.", user.UserID) };


                    // Check weather the user is temporary blocked or not.
                    if (user.AcountBlockedTill.HasValue && user.AcountBlockedTill.Value.Date >= DateTime.Now.Date && user.IsAccountBlocked)
                        return new ActionOutput<User> { Status = ActionStatus.Error, Object = null, Message = "Your Account Has been blocked by the system." };

                    /* Saving login history */
                    UserLoginHistory loginHistory = new UserLoginHistory();
                    loginHistory.LastLoginIP = IP;
                    loginHistory.LastLoginOn = DateTime.Now;
                    loginHistory.UserId = user.UserID;
                    context.UserLoginHistories.AddObject(loginHistory);

                    user.LastLoginOn = DateTime.Now;
                    /* Updating User InvalidLogin Attempt */
                    if (user.InValidLoginAttempt != 0)
                    {
                        user.IsAccountBlocked = false;
                        user.InValidLoginAttempt = 0;
                        user.AcountBlockedTill = null;
                    }

                    context.SaveChanges();

                    if (user.RoleId == (int)UserRole.Seller)
                    {
                        /* Geting Shop Information */
                        user.ShopDetails = context.Shops.Where(s => s.UserId == user.UserID).FirstOrDefault();
                        user.ShopPictures = context.ShopPictures.Where(sp => sp.ShopId == user.ShopDetails.ShopID).ToList();
                    }
                    if (user.ProfilePicId != null)
                    {
                        var picture = context.Pictures.Where(p => p.PictureID == user.ProfilePicId).FirstOrDefault();
                        user.ProfilePicUrl = "/Images/ProfileImage/" + user.UserName + "/" + picture.SavedName;
                    }
                    if (user.CityId != null)
                    {
                        user.UserLocation = user.City.CityName + ", ";
                    }
                    if (user.StateId != null)
                    {
                        user.UserLocation += user.StateProvince.StateName + ", ";
                    }
                    if (user.CountryId != null)
                    {
                        String countryName = user.Country.CountryName;
                        user.UserLocation += countryName;
                    }


                    return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = user, Message = "Login Successful." };
                }
                else if (countLoginAttempt)
                {
                    user = context.Users.Where(m => m.EmailId.Equals(login, StringComparison.InvariantCultureIgnoreCase)
                                                        || m.UserName.Equals(login, StringComparison.InvariantCultureIgnoreCase)
                                                        && !m.IsDeleted).FirstOrDefault();
                    if (user != null)
                    {
                        user.InValidLoginAttempt = user.InValidLoginAttempt + 1;
                        if (user.InValidLoginAttempt >= Constants.InvalidLoginAttempt)
                        {
                            user.IsAccountBlocked = true;
                            user.AcountBlockedTill = DateTime.Now.AddDays(Constants.BlockAcountDays);
                            context.SaveChanges();
                            return new ActionOutput<User> { Status = ActionStatus.Error, Object = null, Message = "Your Account Has been blocked by the system." };
                        }
                        else
                        {
                            context.SaveChanges();
                            return new ActionOutput<User> { Status = ActionStatus.Error, Object = null, Message = "Invalid credentials." };

                        }
                    }
                    else
                    {
                        return new ActionOutput<User> { Status = ActionStatus.Error, Object = null, Message = "Invalid credentials." };
                    }
                }
                else
                {
                    return new ActionOutput<User> { Status = ActionStatus.Error, Object = null, Message = "Invalid credentials." };
                }
            }
        }

        /// <summary>
        /// User Signup
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ActionOutput<User> UserSignup(User obj)
        {
            obj.LastLoginOn = DateTime.Now;
            using (var context = new CentroEntities())
            {
                User u;
                if (obj.AuthenticatedVia == (int)AuthenticationFrom.Twitter)
                {
                    u = context.Users.Where(m => m.UserName.Equals(obj.UserName, StringComparison.InvariantCultureIgnoreCase) && m.AuthenticatedVia == obj.AuthenticatedVia).FirstOrDefault();
                }
                else
                {
                    u = context.Users.Where(m => m.EmailId.Equals(obj.EmailId, StringComparison.InvariantCultureIgnoreCase) && m.AuthenticatedVia == obj.AuthenticatedVia).FirstOrDefault();
                }
                if (u != null)
                {
                    return new ActionOutput<User> { Status = ActionStatus.Error, Message = "Email Id already In Use." };
                }
                User u1 = context.Users.Where(m => m.UserName.Equals(obj.UserName, StringComparison.InvariantCultureIgnoreCase) && m.AuthenticatedVia==obj.AuthenticatedVia).FirstOrDefault();
                if (u1 != null)
                {
                    return new ActionOutput<User> { Status = ActionStatus.Error, Message = "Username already In Use." };
                }
                obj.CreatedBy = null;
                obj.CreatedOn = DateTime.Now;
                obj.DeletedBy = null;
                obj.DeletedOn = null;
                obj.IsDeleted = false;

                if (obj.AuthenticatedVia == (int)AuthenticationFrom.Website)
                {
                    obj.IsVerified = false;
                    obj.VerifiedOn = null;
                    obj.AuthenticatedVia = (int)AuthenticationFrom.Website;
                }
                else
                {
                    obj.IsVerified = true;
                    obj.VerifiedOn = DateTime.Now;
                    obj.Password = "FacebookTwitter";
                }
                obj.UpdatedBy = null;
                obj.UpdatedOn = null;

                obj.RoleId = (int)UserRole.Buyer;
                obj.UserGUID = System.Guid.NewGuid();
                obj.IsPasswordReset = false;
                obj.InValidLoginAttempt = 0;
                context.Users.AddObject(obj);
                context.SaveChanges();
                return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = obj };
            }
        }

        /// <summary>
        /// This method will be used to activate account by verification code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static ActionOutput<User> ActivateAccount(string code)
        {
            string login = Utility.DecryptString(code);
            using (var context = new CentroEntities())
            {
                var member = context.Users.Where(m => m.UserName.Equals(login, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (member != null)
                {
                    if (member.IsVerified)
                    {
                        return new ActionOutput<User> { Status = ActionStatus.Error, Object = member, Message = "This Account is already verified." };
                    }
                    else
                    {
                        member.IsVerified = true;
                        member.VerifiedOn = DateTime.Now;
                        context.SaveChanges();
                        return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = member, };
                    }

                }
                return new ActionOutput<User> { Status = ActionStatus.Unauthorized, Object = null };
            }
        }

        /// <summary>
        /// This method will be used to get user by user id
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="IP"></param>
        /// <returns></returns>
        public static ActionOutput<User> GetUserByID(Int32 userId)
        {
            using (var context = new CentroEntities())
            {
                User user;
                user = context.Users.Where(m => m.UserID.Equals(userId)
                                                    && !m.IsDeleted).FirstOrDefault();
                user.PaypalIdOptional = user.PaypalID;
                if (user != null)
                {
                    // Check weather the user is verified or not.
                    //if (!user.IsVerified)
                    //  return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = null, Message = "User has not been verified yet." };
                    // if (!user.IsVerified)
                    // return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = null, Message = "User has not been verified yet." };
                    //if (user.RoleId == (int)UserRole.Seller)
                    ActionOutput<Shop> shop;
                    shop = SellersHandler.ShopByUserId(userId);
                    if (shop.Object != null)
                    {
                        user.ShopDetails = context.Shops.Where(s => s.UserId == user.UserID).FirstOrDefault();
                        user.ShopPictures = context.ShopPictures.Where(sp => sp.ShopId == user.ShopDetails.ShopID).ToList();
                        user.ShopID = shop.Object.ShopID;
                    }
                    if (user.ProfilePicId != null)
                    {
                        var picture = context.Pictures.Where(p => p.PictureID == user.ProfilePicId).FirstOrDefault();
                        user.ProfilePicUrl = "/Images/ProfileImage/" + user.UserName + "/" + picture.SavedName;
                    }
                    if (user.CityId != null)
                    {
                        user.UserLocation = user.City.CityName + ", ";
                    }
                    if (user.StateId != null)
                    {
                        user.UserLocation += user.StateProvince.StateName + ", ";
                    }
                    if (user.CountryId != null)
                    {
                        String countryName = user.Country.CountryName;
                        user.UserLocation += countryName;
                    }
                    return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = user };
                }
                return new ActionOutput<User> { Status = ActionStatus.Unauthorized, Object = null, Message = "Invalid user id." };
            }
        }

        ///// <summary>
        ///// This method will be used to get user by user id
        ///// </summary>
        ///// <param name="userId"></param>
        ///// <returns></returns>
        //public static ActionOutput<User> GetUserByID(Int32 userId)
        //{
        //    using (var context = new CentroEntities())
        //    {
        //        User user;
        //        user = context.Users.Where(m => m.UserID.Equals(userId)
        //                                            && !m.IsDeleted).FirstOrDefault();
        //        if (user != null)
        //        {
        //            if (user.RoleId == (int)UserRole.Seller)
        //            {
        //                /* Geting Shop Information */
        //                user.ShopDetails = context.Shops.Where(s => s.UserId == user.UserID).FirstOrDefault();
        //                user.ShopPictures = context.ShopPictures.Where(sp => sp.ShopId == user.ShopDetails.ShopID).ToList();
        //            }
        //            return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = user, Message = "" };
        //        }
        //        return new ActionOutput<User> { Status = ActionStatus.Error, Object = null, Message = "User does not exists." };
        //    }
        //}

        /// <summary>
        /// This method will be used to get user information bu authentication Id && email id
        /// </summary>
        /// <param name="userObj"></param>
        /// <param name="IP"></param>
        /// <returns></returns>
        public static ActionOutput<User> GetUserByAuthenticationID(User userObj, string IP)
        {
            using (var context = new CentroEntities())
            {
                User user;
                user = context.Users.Where(m => m.AuthenticationID.Equals(userObj.AuthenticationID)
                                                    || m.EmailId.Equals(userObj.EmailId, StringComparison.InvariantCultureIgnoreCase)
                                                    && !m.IsDeleted).FirstOrDefault();
                if (user != null)
                {

                    /* Saving login history */
                    UserLoginHistory loginHistory = new UserLoginHistory();
                    loginHistory.LastLoginIP = IP;
                    loginHistory.LastLoginOn = DateTime.Now;
                    loginHistory.UserId = user.UserID;
                    context.UserLoginHistories.AddObject(loginHistory);
                    context.SaveChanges();
                    if (user.AuthenticationID == null)
                    {
                        user.AuthenticationID = userObj.AuthenticationID;
                        user.AuthenticatedVia = userObj.AuthenticatedVia;
                        context.SaveChanges();
                    }
                    if (user.RoleId == (int)UserRole.Seller)
                    {
                        /* Geting Shop Information */
                        user.ShopDetails = context.Shops.Where(s => s.UserId == user.UserID).FirstOrDefault();
                        user.ShopPictures = context.ShopPictures.Where(sp => sp.ShopId == user.ShopDetails.ShopID).ToList();
                    }
                    return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = user, Message = "Login Successful." };
                }
                return new ActionOutput<User> { Status = ActionStatus.Unauthorized, Object = null, Message = "Invalid credentials." };
            }
        }

        /// <summary>
        /// This Method will be used to send password link to the user
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="Link_url"></param>
        public static ActionOutput SendUserPassword(User obj, String Link_url)
        {
            using (var context = new CentroEntities())
            {
                User us = context.Users.Where(u => u.EmailId.Equals(obj.EmailId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (!us.IsVerified)
                    if (!us.IsVerified)
                        return new ActionOutput { Status = ActionStatus.Error, Message = string.Format("You have not verified your email yet. Please click <a href=\"javascript:;\" onclick=\"Resend({0}, this)\">here</a> to resend the verification email.", us.UserID) };
                EmailHandler.ForgotPassword(obj.FirstName + " " + obj.LastName, obj.EmailId, Link_url);
                us.IsPasswordReset = true;
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "A Link has been sent to your registered email-id to reset the password." };
            }
        }

        /// <summary>
        /// This Method will be used to get an user by email id
        /// </summary>
        /// <param name="EmailId"></param>
        /// <returns></returns>
        public static ActionOutput<User> GetUserByEmail(String EmailId, Boolean? checkAlternate)
        {
            using (var context = new CentroEntities())
            {
                User user;
                if (checkAlternate.HasValue)
                    user = context.Users.Where(m => m.EmailId.Equals(EmailId, StringComparison.InvariantCultureIgnoreCase) || m.AlterateEmailId.Equals(EmailId, StringComparison.InvariantCultureIgnoreCase) && m.IsVerified
                                                    && !m.IsDeleted).FirstOrDefault();
                else
                    user = context.Users.Where(m => m.EmailId.Equals(EmailId, StringComparison.InvariantCultureIgnoreCase) && m.IsVerified
                                                    && !m.IsDeleted).FirstOrDefault();

                if (user != null)
                {
                    return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = user };
                }
                else
                    return new ActionOutput<User> { Status = ActionStatus.Error, Object = null, Message = "Sorry, there's no account associated with that email address" };
            }
        }

        /// <summary>
        /// This method will be used to reset password
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static ActionOutput ResetPassword(string code, string newPassword)
        {
            string emailid = Utility.DecryptString(code);
            using (var context = new CentroEntities())
            {
                var member = context.Users.Where(m => m.EmailId.Equals(emailid, StringComparison.InvariantCultureIgnoreCase)
                                                        && m.IsPasswordReset.Value).FirstOrDefault();
                if (member != null)
                {
                    member.Password = Utility.EncryptString(newPassword);
                    member.UpdatedOn = DateTime.Now;
                    member.IsPasswordReset = false;
                    context.SaveChanges();
                    return new ActionOutput { ID = member.UserID, Status = ActionStatus.Successfull };
                }

            }
            return new ActionOutput { Status = ActionStatus.Error, Message = "Invalid code" };
        }

        /// <summary>
        /// Get the user by username, password and set the login history
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="IP"></param>
        /// <returns></returns>
        public static ActionOutput<User> AdminLogin(string login, string password, string IP)
        {
            using (var context = new CentroEntities())
            {
                User user;
                if (login.Contains('@'))
                    user = context.Users.Where(m => m.EmailId.Equals(login, StringComparison.InvariantCultureIgnoreCase)
                                                        && m.Password.Equals(password, StringComparison.InvariantCultureIgnoreCase)
                                                        && !m.IsDeleted && m.IsVerified && m.RoleId == (int)UserRole.Administrator).FirstOrDefault();
                else
                    user = context.Users.Where(m => m.UserName.Equals(login, StringComparison.InvariantCultureIgnoreCase)
                                                        && m.Password.Equals(password, StringComparison.InvariantCultureIgnoreCase)
                                                        && !m.IsDeleted && m.IsVerified && m.RoleId == (int)UserRole.Administrator).FirstOrDefault();
                if (user != null)
                {


                    /* Saving login history */
                    UserLoginHistory loginHistory = new UserLoginHistory();
                    loginHistory.LastLoginIP = IP;
                    loginHistory.LastLoginOn = DateTime.Now;
                    loginHistory.UserId = user.UserID;
                    context.UserLoginHistories.AddObject(loginHistory);
                    context.SaveChanges();

                    return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = user, Message = "Login Successful." };
                }
                return new ActionOutput<User> { Status = ActionStatus.Unauthorized, Object = null, Message = "Invalid credentials." };
            }
        }

        public static PagingResult<UserListing_Result> UserPaging(int page_no, int per_page_result, string sortColumn, string sortOrder, string username)
        {
            using (var context = new CentroEntities())
            {
                ObjectParameter output = new ObjectParameter("TotalCount", typeof(int));
                var list = context.UserListing(page_no, per_page_result, username, sortColumn, sortOrder, output).ToList();
                PagingResult<UserListing_Result> pagingResult = new PagingResult<UserListing_Result>();
                pagingResult.List = list;
                pagingResult.TotalCount = Convert.ToInt32(output.Value);
                pagingResult.Status = ActionStatus.Successfull;
                return pagingResult;
            }
        }

        /// <summary>
        /// This will be used to bulk update from admin
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="LoggedInUser"></param>
        /// <returns></returns>
        public static ActionOutput UpdateUsers(UserAction obj, UserDetails LoggedInUser)
        {
            using (var context = new CentroEntities())
            {
                if (obj.ActionID == ActionOnUser.Activate)
                {
                    foreach (var id in obj.UserID)
                    {
                        var user = context.Users.Where(u => u.UserID.Equals(id)).FirstOrDefault();
                        user.IsAccountBlocked = false;
                        user.AcountBlockedTill = null;
                        user.UpdatedOn = DateTime.Now;
                        user.UpdatedBy = LoggedInUser.Id;

                        context.SaveChanges();
                    }
                }
                else if (obj.ActionID == ActionOnUser.DeActivate)
                {
                    foreach (var id in obj.UserID)
                    {
                        var user = context.Users.Where(u => u.UserID.Equals(id)).FirstOrDefault();
                        user.IsAccountBlocked = true;
                        user.AcountBlockedTill = DateTime.Now.AddYears(1);
                        user.UpdatedOn = DateTime.Now;
                        user.UpdatedBy = LoggedInUser.Id;

                        context.SaveChanges();
                    }
                }
                else if (obj.ActionID == ActionOnUser.Delete)
                {
                    foreach (var id in obj.UserID)
                    {
                        // Its Hard delete (all data except invoices and PrototypeRequests would be deleted permanantly.
                        var user = context.Users.Where(u => u.UserID.Equals(id)).FirstOrDefault();
                        var invoices = context.Invoices.Where(m => m.BuyerID == user.UserID || m.SellerID == user.UserID).ToList();
                        foreach (var invoice in invoices)
                        {
                            if (invoice.SellerID == user.UserID)
                                invoice.SellerID = null;
                            else
                                invoice.BuyerID = null;
                        }
                        var contracts = context.PrototypeRequests.Where(m => m.BuyerId == user.UserID || m.SellerId == user.UserID).ToList();
                        foreach (var contract in contracts)
                        {
                            if (contract.SellerId == user.UserID)
                            {
                                contract.SellerId = null;
                                contract.ShopId = null;
                            }
                            else
                                contract.BuyerId = null;
                        }
                        context.DeleteObject(user);
                        context.SaveChanges();
                    }
                }
            }
            return new ActionOutput { Status = ActionStatus.Successfull, Message = "Records Updated Successfully." };
        }


        public static ActionOutput UpdateUserPaypalID(User obj)
        {
            using (var context = new CentroEntities())
            {
                var user = context.Users.Where(u => u.UserID == obj.UserID).FirstOrDefault();
                if (string.IsNullOrEmpty(user.PaypalID))
                    user.PaypalID = obj.PaypalID; // Will not change if already exists
                //user.FirstName = obj.FirstName;
                //user.LastName = obj.LastName;
                //user.EmailId = obj.EmailId;
                //user.CountryId = obj.CountryId;
                //user.StateId = obj.StateId;
                //user.CityId = obj.CityId;
                //user.PostalCode = obj.PostalCode;
                //user.StreetAddress1 = obj.StreetAddress1;
                //user.StreetAddress2 = user.StreetAddress2;
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Paypal ID has been updated." };
            }
        }

        /// <summary>
        /// this will be used ti update user details from admin and from frontend
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="LoggedInUser"></param>
        /// <param name="verification_url"></param>
        /// <returns></returns>
        public static ActionOutput UpdateUserDetails(User obj, UserDetails LoggedInUser, string verification_url)
        {
            using (var context = new CentroEntities())
            {
                var user = context.Users.Where(u => u.UserID.Equals(obj.UserID)).FirstOrDefault();
                user.FirstName = user.FirstName.Equals(obj.FirstName, StringComparison.InvariantCultureIgnoreCase) ? user.FirstName : obj.FirstName;
                user.LastName = user.LastName.Equals(obj.LastName, StringComparison.InvariantCultureIgnoreCase) ? user.LastName : obj.LastName;
                user.Gender = user.Gender.Equals(obj.Gender) ? user.Gender : obj.Gender;
                user.NewsLetterEnabled = user.NewsLetterEnabled.Equals(obj.NewsLetterEnabled) ? user.NewsLetterEnabled : obj.NewsLetterEnabled;
                user.AboutUs = obj.AboutUs;
                user.Industry = obj.Industry;
                user.LinkedIn = obj.LinkedIn;
                user.Backlinked = obj.Backlinked;
                if (string.IsNullOrEmpty(user.PaypalID))
                    user.PaypalID = obj.PaypalIdOptional; // will not change if already exists
                user.StreetAddress1 = obj.StreetAddress1;
                obj.StreetAddress2 = obj.StreetAddress2;
                user.PostalCode = obj.PostalCode;
                user.Licence = obj.Licence;
                if (!String.IsNullOrEmpty(obj.Password))
                    user.Password = obj.Password;
                if (obj.DateofBirth != null)
                    user.DateofBirth = obj.DateofBirth;
                //if (obj.AboutUs != null)
                user.AboutUs = obj.AboutUs; ;
                if (obj.CountryId != null)
                    user.CountryId = obj.CountryId;
                if (obj.StateId != null)
                    user.StateId = obj.StateId;
                if (obj.CityId != null)
                    user.CityId = obj.CityId;
                obj.UserTagsList = obj.UserTagsList != null ? obj.UserTagsList : "";
                if (obj.UserTagsList != null)
                {
                    List<string> tagList = obj.UserTagsList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();

                    var user_tag_list = context.UserTags.Where(o => o.UserId == obj.UserID).ToList();
                    var tags_to_be_removed = user_tag_list.Where(o => !tagList.Contains(o.Tag.TagText, true)).ToList();
                    foreach (var item in tags_to_be_removed)
                    {
                        context.UserTags.DeleteObject(item);
                        // context.SaveChanges();
                    }
                    foreach (var item in tagList)
                    {
                        Tag tg = new Tag();
                        tg.TagText = item;
                        tg.TagType = (int)TagType.UserTag;
                        var tag = context.Tags.Where(o => o.TagText.Equals(item, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        if (tag == null)
                        {
                            context.Tags.AddObject(tg);
                            UserTag usertg = new UserTag();
                            usertg.UserId = obj.UserID;
                            usertg.Tag = tg;
                        }
                        else
                        {
                            UserTag usertg = context.UserTags.Where(o => o.TagId.Value.Equals(tag.TagID) && o.UserId.Equals(obj.UserID)).FirstOrDefault();
                            if (usertg == null)
                            {
                                usertg = new UserTag();
                                usertg.UserId = obj.UserID;
                                usertg.Tag = tag;
                                context.UserTags.AddObject(usertg);

                            }
                        }

                    }
                }

                user.UpdatedOn = DateTime.Now;
                user.UpdatedBy = LoggedInUser.Id;
                if (obj.EmailId != null)
                {
                    if (!obj.EmailId.Equals(user.EmailId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var result = UsersHandler.GetUserByEmail(obj.EmailId, true);
                        if (result.Object != null)
                        {
                            return new ActionOutput { Status = ActionStatus.Error, Message = "Sorry This Email id already exist for any other account." };
                        }
                        else
                        {
                            EmailHandler.VerifyEmail(obj.FirstName + " " + obj.LastName, obj.EmailId, verification_url);
                            user.AlterateEmailId = obj.EmailId;
                            context.SaveChanges();
                            return new ActionOutput { Status = ActionStatus.Successfull, Message = String.Format("An Email has been sent to your email id {0} Please verify your email.", obj.EmailId) };
                        }
                    }

                }
                context.SaveChanges();

                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Records Updated Successfully." };



            }

        }

        /// <summary>
        /// This method will be used to verify email by verification code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static ActionOutput<User> VerifyEmail(string code)
        {
            string login = Utility.DecryptString(code);
            using (var context = new CentroEntities())
            {
                var member = context.Users.Where(m => m.EmailId.Equals(login, StringComparison.InvariantCultureIgnoreCase) && m.AuthenticatedVia==(int)AuthenticationFrom.Website).FirstOrDefault();
                if (member != null)
                {
                    //if (String.IsNullOrEmpty(member.AlterateEmailId))
                    //{
                    //    return new ActionOutput<User> { Status = ActionStatus.Error, Object = member, Message = "This EmailId is already verified." };
                    //}
                    //else
                    //{
                    member.IsVerified = true;
                    context.SaveChanges();
                    return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = member, Message="Your account is verified now." };
                    //}

                }
                return new ActionOutput<User> { Status = ActionStatus.Error, Object = null, Message="Invalid activation code." };
            }
        }

        public static ActionOutput UserRegDetail(Int32 userId)
        {
            string CreatedOn = "", RegistrationCount = "";
            using (var context = new CentroEntities())
            {
                User user = context.Users.Where(m => m.UserID.Equals(userId) && !m.IsDeleted).FirstOrDefault();
                if (user != null)
                {
                    RegistrationCount = context.Users.Where(m => m.CreatedOn.Month == user.CreatedOn.Month && m.UserID <= user.UserID).Count().ToString();
                    CreatedOn = user.CreatedOn.ToLongDateString();
                }
                string regCount = "";
                if (RegistrationCount == "1") regCount = RegistrationCount + "st";
                else if (RegistrationCount == "2") regCount = RegistrationCount + "nd";
                else regCount = RegistrationCount + "th";
                return new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { regCount, CreatedOn } };
            }
        }


        /// <summary>
        /// used to get user tag list
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public static string GetUserTags(int user_id)
        {
            string s = "";
            using (var context = new CentroEntities())
            {
                var tagIds = context.UserTags.Where(tg => tg.UserId == user_id).Select(tg => tg.TagId).ToList();
                List<string> tagList = new List<string>();
                foreach (var item in tagIds)
                {
                    string tag = context.Tags.Where(tg => tg.TagID == item).Select(z => z.TagText).FirstOrDefault();
                    tagList.Add(tag);
                }
                s = string.Join(",", tagList.ToArray());
            }
            return s;
        }

        /// <summary>
        /// update user profile picture
        /// </summary>
        /// <param name="user_profile_pic"></param>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public static ActionOutput UpdateUserProfilePic(UserProductPicture user_profile_pic, Int32 user_id)
        {
            using (var context = new CentroEntities())
            {
                User user = context.Users.Where(o => o.UserID == user_id).FirstOrDefault();
                var item = user_profile_pic;
                if (user.ProfilePicId == null)
                {

                    Picture pic = new Picture();
                    pic.CreatedBy = user_id;
                    pic.CreatedOn = DateTime.Now;
                    pic.DeletedBy = null;
                    pic.DeletedOn = null;
                    pic.DisplayName = item.DisplayName;
                    pic.SavedName = item.SavedName;
                    pic.Thumbnail = item.Thumbnail;
                    pic.IsDeleted = false;
                    pic.MimeType = item.MimeType;
                    pic.SizeInBytes = item.SizeInBytes;
                    pic.SizeInKB = item.SizeInKB;
                    pic.SizeInMB = item.SizeInMB;
                    pic.UpdatedBy = null;
                    pic.UpdatedOn = null;
                    context.Pictures.AddObject(pic);
                    context.SaveChanges();

                    user.ProfilePicId = pic.PictureID;

                    context.SaveChanges();
                }
                else
                {
                    Picture pic = context.Pictures.Where(o => o.PictureID == user.ProfilePicId.Value).FirstOrDefault();
                    pic.DeletedBy = null;
                    pic.DeletedOn = null;
                    pic.DisplayName = item.DisplayName;
                    pic.SavedName = item.SavedName;
                    pic.Thumbnail = item.Thumbnail;
                    pic.IsDeleted = false;
                    pic.MimeType = item.MimeType;
                    pic.SizeInBytes = item.SizeInBytes;
                    pic.SizeInKB = item.SizeInKB;
                    pic.SizeInMB = item.SizeInMB;
                    pic.UpdatedBy = user_id;
                    pic.UpdatedOn = DateTime.Now; ;

                    context.SaveChanges();
                }

                // Move temp files to user's folder
                Utility.MoveFile("~/Temp/" + item.Username + "/" + item.SavedName, "~/Images/ProfileImage/" + item.Username + "/", item.SavedName);
                Utility.MoveFile("~/Temp/" + item.Username + "/" + item.Thumbnail, "~/Images/ProfileImage/" + item.Username + "/", item.Thumbnail);
                var picture = context.Pictures.Where(p => p.PictureID == user.ProfilePicId).FirstOrDefault();
                string picUrl = user.ProfilePicUrl = "/Images/ProfileImage/" + user.UserName + "/" + picture.SavedName;
                return new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { picUrl }, Message = "Images have been saved." };
            }
        }


        public static ActionOutput<SalesTax> GetSalesTaxByUserID(int user_id)
        {
            using (var context = new CentroEntities())
            {
                var list = context.SalesTaxes.Where(s => s.UserID == user_id).ToList();
                var countries = context.Countries.ToList();
                var USSTates = context.StateProvinces.Where(m => m.CountryID == 1).ToList();
                for (short i = 0; i < list.Count(); i++)
                {
                    list[i].CountryName = countries.Where(m => m.CountryID == list[i].ToCountryID).FirstOrDefault().CountryName;
                    if (list[i].ToStateID != null)
                        list[i].StateName = USSTates.Where(m => m.StateID == list[i].ToStateID).FirstOrDefault().StateName;
                }
                return new ActionOutput<SalesTax> { List = list, Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput ChangeUserPassword(Int32 userId, ChangePasswordModel model)
        {

            using (var context = new CentroEntities())
            {
                var encryptedString = Utility.EncryptString(model.CurrentPassword);

                User user = context.Users.Where(m => m.UserID.Equals(userId) && m.Password.Equals(encryptedString, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (user != null)
                {
                    user.Password = Utility.EncryptString(model.NewPassword);
                    user.UpdatedBy = userId;
                    user.UpdatedOn = DateTime.Now;
                    context.SaveChanges();
                    return new ActionOutput
                    {
                        Status = ActionStatus.Successfull,
                        Message = "Password Updated Successfully"
                    };
                }
                else
                {

                    return new ActionOutput
                    {
                        Status = ActionStatus.Error,
                        Message = "Old Password does not match. Please enter correct password."
                    };
                }
            }
        }

        public static ActionOutput<BillingAddress> GetBillingAddress(int user_id)
        {
            using (var context = new CentroEntities())
            {
                var list = context.BillingAddresses.Where(b => b.UserID == user_id).ToList();
                return new ActionOutput<BillingAddress> { List = list, Status = ActionStatus.Successfull };
            }
        }
        public static BillingAddress GetBillingAddressByID(int user_id, int billingAddressId)
        {
            using (var context = new CentroEntities())
            {
                var address = context.BillingAddresses.Where(b => b.UserID == user_id && b.BillingAddressID == billingAddressId).FirstOrDefault();
                return address;
            }
        }
        public static ActionOutput<ShippingAddress> GetShippingAddress(int user_id)
        {
            using (var context = new CentroEntities())
            {
                var list = context.ShippingAddresses.Where(b => b.UserID == user_id).ToList();
                return new ActionOutput<ShippingAddress> { List = list, Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput AddUpdateBillingAddress(int user_id, BillingAddress obj)
        {
            using (var context = new CentroEntities())
            {
                if (obj.UserID > 0 && obj.BillingAddressID != 0)
                {
                    var address = context.BillingAddresses.Where(b => b.BillingAddressID == obj.BillingAddressID && b.UserID == user_id).FirstOrDefault();
                    if (obj.IsPrimary)
                    {
                        var exitingPrimaryContact = context.BillingAddresses.Where(b => b.IsPrimary && b.UserID == user_id).FirstOrDefault();
                        if (exitingPrimaryContact != null && exitingPrimaryContact.BillingAddressID != address.BillingAddressID)
                        {
                            exitingPrimaryContact.IsPrimary = false;
                            context.SaveChanges();
                        }
                    }
                    else
                    {
                        var addresses = context.BillingAddresses.Where(b => b.IsPrimary && b.UserID == user_id).FirstOrDefault();
                        if (addresses.BillingAddressID == obj.BillingAddressID)
                        {
                            obj.IsPrimary = true;
                        }

                    }

                    address.FirstName = obj.FirstName;
                    address.LastName = obj.LastName;
                    address.Email = obj.Email;
                    address.CountryId = obj.CountryId;
                    address.StateId = obj.StateId;
                    address.CityId = obj.CityId;
                    address.Address = obj.Address;
                    address.PostCode = obj.PostCode;
                    address.IsPrimary = obj.IsPrimary;


                    context.SaveChanges();

                    return new ActionOutput { ID = address.BillingAddressID, Status = ActionStatus.Successfull, Message = "Billing Address Updated Successfully." };
                }
                else
                {
                    obj.UserID = user_id;
                    if (obj.IsPrimary)
                    {
                        var exitingPrimaryContact = context.BillingAddresses.Where(b => b.IsPrimary && b.UserID == user_id).FirstOrDefault();
                        if (exitingPrimaryContact != null)
                            exitingPrimaryContact.IsPrimary = false;
                        context.SaveChanges();
                    }
                    else
                    {
                        int c = context.BillingAddresses.Where(b => b.IsPrimary && b.UserID == user_id).Count();
                        if (c == 0)
                            obj.IsPrimary = true;
                    }
                    context.BillingAddresses.AddObject(obj);
                    context.SaveChanges();
                    return new ActionOutput { ID = obj.BillingAddressID, Status = ActionStatus.Successfull, Message = "Billing Address Added Successfully." };
                }


            }
        }

        public static ActionOutput<BillingAddress> BillingViewModelToDBObject(BillingAddressViewModel viewmodel)
        {
            BillingAddress obj = new BillingAddress();
            obj.BillingAddressID = viewmodel.BillingAddressID;
            obj.UserID = viewmodel.UserID;
            obj.FirstName = viewmodel.FirstName;
            obj.LastName = viewmodel.LastName;
            obj.Email = viewmodel.Email;
            obj.CountryId = viewmodel.CountryId;
            obj.StateId = viewmodel.StateId;
            obj.CityId = viewmodel.CityId;
            obj.Address = viewmodel.Address;
            obj.PostCode = viewmodel.PostCode;
            obj.IsPrimary = viewmodel.IsPrimary;
            return new ActionOutput<BillingAddress> { Status = ActionStatus.Successfull, Object = obj };
        }

        public static ActionOutput<ShippingAddress> ShippingViewModelToDBObject(ShippingAddressViewModel viewmodel)
        {
            ShippingAddress obj = new ShippingAddress();
            obj.ShippingAddressID = viewmodel.ShippingAddressID;
            obj.UserID = viewmodel.UserID;
            obj.FirstName = viewmodel.FirstName;
            obj.LastName = viewmodel.LastName;
            obj.Email = viewmodel.Email;
            obj.CountryID = viewmodel.CountryID;
            obj.StateID = viewmodel.StateID;
            obj.CityID = viewmodel.CityID;
            obj.Address = viewmodel.Address;
            obj.PostCode = viewmodel.PostCode;
            obj.IsPrimary = viewmodel.IsPrimary;
            return new ActionOutput<ShippingAddress> { Status = ActionStatus.Successfull, Object = obj };
        }

        public static ActionOutput<BillingAddressViewModel> DBBillingObjectToBillingViewModel(BillingAddress viewmodel)
        {
            BillingAddressViewModel obj = new BillingAddressViewModel();
            obj.BillingAddressID = viewmodel.BillingAddressID;
            obj.UserID = viewmodel.UserID;
            obj.FirstName = viewmodel.FirstName;
            obj.LastName = viewmodel.LastName;
            obj.Email = viewmodel.Email;
            obj.CountryId = viewmodel.CountryId;
            obj.StateId = viewmodel.StateId;
            obj.CityId = viewmodel.CityId;
            obj.Address = viewmodel.Address;
            obj.PostCode = viewmodel.PostCode;
            obj.IsPrimary = viewmodel.IsPrimary;
            return new ActionOutput<BillingAddressViewModel> { Status = ActionStatus.Successfull, Object = obj };
        }

        public static ActionOutput<ShippingAddressViewModel> DBShippingObjectToShippingViewModel(ShippingAddress viewmodel)
        {
            ShippingAddressViewModel obj = new ShippingAddressViewModel();
            obj.ShippingAddressID = viewmodel.ShippingAddressID;
            obj.UserID = viewmodel.UserID;
            obj.FirstName = viewmodel.FirstName;
            obj.LastName = viewmodel.LastName;
            obj.Email = viewmodel.Email;
            obj.CountryID = viewmodel.CountryID;
            obj.StateID = viewmodel.StateID;
            obj.CityID = viewmodel.CityID;
            obj.Address = viewmodel.Address;
            obj.PostCode = viewmodel.PostCode;
            obj.IsPrimary = viewmodel.IsPrimary;
            return new ActionOutput<ShippingAddressViewModel> { Status = ActionStatus.Successfull, Object = obj };
        }

        public static ActionOutput<string> UsersStartsWith(string startswith, string[] except)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<string>
                {
                    List = context.Users.Where(m => m.UserName.StartsWith(startswith)
                                                    && !except.Contains(m.UserName)
                                                    && !m.IsAccountBlocked && !m.IsDeleted).Select(m => m.UserName).ToList(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput<User> UserByUsername(string username)
        {
            using (var context = new CentroEntities())
            {
                var user = context.Users.Where(m => m.UserName == username).FirstOrDefault();
                if (user != null)
                {
                    // Check weather the user is verified or not.
                    //if (!user.IsVerified)
                    //  return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = null, Message = "User has not been verified yet." };
                    // if (!user.IsVerified)
                    // return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = null, Message = "User has not been verified yet." };
                    //if (user.RoleId == (int)UserRole.Seller)
                    //{
                    /* Geting Shop Information */
                    user.ShopDetails = context.Shops.Where(s => s.UserId == user.UserID).FirstOrDefault();
                    user.ShopPictures = user.ShopDetails != null ? context.ShopPictures.Where(sp => sp.ShopId == user.ShopDetails.ShopID).ToList() : null;
                    //}
                    if (user.ProfilePicId != null)
                    {
                        var picture = context.Pictures.Where(p => p.PictureID == user.ProfilePicId).FirstOrDefault();
                        user.ProfilePicUrl = "/Images/ProfileImage/" + user.UserName + "/" + picture.SavedName;
                    }
                    if (user.CityId != null)
                    {
                        user.UserLocation = user.City.CityName + ", ";
                    }
                    if (user.StateId != null)
                    {
                        user.UserLocation += user.StateProvince.StateName + ", ";
                    }
                    if (user.CountryId != null)
                    {
                        String countryName = user.Country.CountryName;
                        user.UserLocation += countryName;
                    }
                    return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = user };
                }
                return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = user };
            }
        }

        public static ActionOutput<User> UserByShopId(int shop_id)
        {
            using (var context = new CentroEntities())
            {
                User user;
                user = context.Users.Join(context.Shops,
                                            u => u.UserID,
                                            s => s.UserId,
                                            (u, s) => new { u, s })
                                    .Where(m => m.s.ShopID == shop_id && !m.u.IsDeleted)
                                    .Select(m => m.u)
                                    .FirstOrDefault();
                if (user != null)
                {
                    // Check weather the user is verified or not.
                    //if (!user.IsVerified)
                    //  return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = null, Message = "User has not been verified yet." };
                    // if (!user.IsVerified)
                    // return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = null, Message = "User has not been verified yet." };
                    //if (user.RoleId == (int)UserRole.Seller)
                    //{
                    /* Geting Shop Information */
                    user.ShopDetails = context.Shops.Where(s => s.UserId == user.UserID).FirstOrDefault();
                    user.ShopPictures = context.ShopPictures.Where(sp => sp.ShopId == user.ShopDetails.ShopID).ToList();
                    //}
                    if (user.ProfilePicId != null)
                    {
                        var picture = context.Pictures.Where(p => p.PictureID == user.ProfilePicId).FirstOrDefault();
                        user.ProfilePicUrl = "/Images/ProfileImage/" + user.UserName + "/" + picture.SavedName;
                    }
                    if (user.CityId != null)
                    {
                        user.UserLocation = user.City.CityName + ", ";
                    }
                    if (user.StateId != null)
                    {
                        user.UserLocation += user.StateProvince.StateName + ", ";
                    }
                    if (user.CountryId != null)
                    {
                        String countryName = user.Country.CountryName;
                        user.UserLocation += countryName;
                    }
                    return new ActionOutput<User> { Status = ActionStatus.Successfull, Object = user };
                }
                return new ActionOutput<User> { Status = ActionStatus.Unauthorized, Object = null, Message = "Invalid user id." };
            }
        }

        public static ActionOutput SaveShowUsernameSetting(UserSetting setting)
        {
            using (var context = new CentroEntities())
            {
                var Old_setting = context.UserSettings.Where(m => m.UserID == setting.UserID).FirstOrDefault();
                if (Old_setting != null)
                {
                    Old_setting.ShowFullname = setting.ShowFullname;
                    Old_setting.ShowLocation = setting.ShowLocation;
                    Old_setting.ShowSkills = setting.ShowSkills;
                }
                else
                    context.UserSettings.AddObject(setting);
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput<UserSetting> GetUsersetting(int user_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<UserSetting>
                {
                    Object = context.UserSettings.Where(m => m.UserID == user_id).FirstOrDefault(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput<ShippingAddress> GetShippingAddressById(int id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<ShippingAddress>
                {
                    Status = ActionStatus.Successfull,
                    Object = context.ShippingAddresses.Where(m => m.ShippingAddressID == id).FirstOrDefault()
                };
            }
        }

        public static ActionOutput AddToFavotire(int ID, bool ForProduct, int userID)
        {
            using (var context = new CentroEntities())
            {
                if (ForProduct)
                {
                    var fav = context.Favorites.Where(m => m.ProductId == ID && m.UserId == userID).FirstOrDefault();
                    if (fav == null)
                    {
                        context.Favorites.AddObject(new Favorite { ProductId = ID, ForProduct = true, ShopId = null, CreatedOn = DateTime.Now, UserId = userID });
                        context.SaveChanges();
                    }
                }
                else
                {
                    var fav = context.Favorites.Where(m => m.ShopId == ID && m.UserId == userID).FirstOrDefault();
                    if (fav == null)
                    {
                        context.Favorites.AddObject(new Favorite { ShopId = ID, ForProduct = false, ProductId = null, CreatedOn = DateTime.Now, UserId = userID });
                        context.SaveChanges();
                    }
                }

                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "Item has been added to favorites."
                };
            }
        }

        public static ActionOutput RemoveFromFavorite(int ID, bool ForProduct, int userID)
        {
            using (var context = new CentroEntities())
            {
                if (ForProduct)
                {
                    var fav = context.Favorites.Where(m => m.ProductId == ID && m.UserId == userID).FirstOrDefault();
                    if (fav != null)
                    {
                        context.DeleteObject(fav);
                        context.SaveChanges();
                    }
                }
                else
                {
                    var fav = context.Favorites.Where(m => m.ShopId == ID && m.UserId == userID).FirstOrDefault();
                    if (fav != null)
                    {
                        context.DeleteObject(fav);
                        context.SaveChanges();
                    }
                }

                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "Item has been removed from favorites."
                };
            }
        }

        public static ActionOutput<Favorite> GetFavoritesByUserId(int user_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Favorite> { Status = ActionStatus.Successfull, List = context.Favorites.Where(m => m.UserId == user_id).ToList() };
            }
        }

        public static ActionOutput<ProductViewModel> GetFavoriteProductsByUserId(int user_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<ProductViewModel>
                {
                    List = context.Products.Join(context.Favorites,
                                                p => p.ProductID,
                                                f => f.ProductId,
                                                (p, f) => new { p, f })
                                            .Join(context.Users,
                                                m => m.p.Manufacturer,
                                                u => u.UserName,
                                                (m, u) => new { m, u })
                                            .Join(context.Shops,
                                                m => m.m.p.ShopId,
                                                s => s.ShopID,
                                                (m, s) => new { m.m, m.u, s })
                                            .Where(m => m.m.f.UserId == user_id)
                                            .Select(s => new ProductViewModel
                                            {
                                                ProductID = s.m.p.ProductID,
                                                Title = s.m.p.Title,
                                                PrimaryPicture = s.m.p.PrimaryPicture,
                                                IsDownloadable = s.m.p.IsDownloadable,
                                                ShopOwnerName = s.u.UserName,
                                                ShopName = s.s.ShopName,
                                                ShopId = s.s.ShopID,
                                                CategoryId = s.m.p.CategoryId
                                            })
                                            .ToList(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput<ProductViewModel> GetFavoriteShopsByUserId(int user_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<ProductViewModel>
                {


                    List = context.Shops.Join(context.Favorites,
                                               s => s.ShopID,
                                               f => f.ShopId,
                                               (s, f) => new { s, f })
                                        .Join(context.Users,
                                                m => m.s.UserId,
                                                u => u.UserID,
                                                (m, u) => new { m, u })
                                            .Join(context.Products,
                                                m => m.m.s.ShopID,
                                                p => p.ShopId,
                                                (m, p) => new { m.m, m.u, p })
                                            .Where(m => m.m.f.UserId == user_id)
                                            .Select(s => new ProductViewModel
                                            {
                                                ProductID = s.p.ProductID,
                                                Title = s.p.Title,
                                                PrimaryPicture = s.p.PrimaryPicture,
                                                IsDownloadable = s.p.IsDownloadable,
                                                ShopOwnerName = s.u.UserName,
                                                ShopName = s.m.s.ShopName,
                                                ShopId = s.p.ShopId,
                                                CategoryId = s.p.CategoryId
                                            })
                                            .ToList(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput<Favorite> CheckIsShopFavoriteByUserID(int user_id, int shop_id)
        {
            using (var context = new CentroEntities())
            {
                Favorite fav = context.Favorites.Where(m => m.UserId == user_id && m.ShopId == shop_id).FirstOrDefault();
                return new ActionOutput<Favorite>
                {
                    Status = ActionStatus.Successfull,
                    Object = fav
                };
            }
        }

        public static ActionOutput<ShopViewModel> GetFavoriteShopByUserIdForView(int user_id)
        {
            using (var context = new CentroEntities())
            {
                List<ShopViewModel> lst = context.Shops.Join(context.Favorites,
                                               s => s.ShopID,
                                               f => f.ShopId,
                                               (s, f) => new { s, f })
                                        .Join(context.Users,
                                                m => m.s.UserId,
                                                u => u.UserID,
                                                (m, u) => new { m, u }).Where(mf => mf.m.f.UserId == user_id)
                                                .Select(s => new ShopViewModel
                                                {
                                                    ShopOwnerName = s.u.UserName,
                                                    ShopName = s.m.s.ShopName,
                                                    ShopID = s.m.s.ShopID,
                                                }).ToList();
                return new ActionOutput<ShopViewModel>
                {
                    Status = ActionStatus.Successfull,
                    List = lst
                };
            }
        }
    }
}
