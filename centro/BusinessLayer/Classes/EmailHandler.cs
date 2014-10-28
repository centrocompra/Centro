using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Classes;
using BusinessLayer.Handler;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Models.DataModel;
using System.IO;

namespace BusinessLayer.Classes
{
    public class EmailHandler
    {
        private static readonly string TemplateFolder = "~/EmailTemplates/";
        private static readonly string NormalUserRegistrationTemplate = TemplateFolder + "UserRegistration.htm";
        private static readonly string NormalUserRegistrationSubject = "Account activation system.";
        private static readonly string ForgotPasswordTemplate = TemplateFolder + "ForgotPassword.htm";
        private static readonly string ForgotPasswordSubject = "Password recovery system.";
        private static readonly string VerifyEmailOnUpdateTemplate = TemplateFolder + "VerifyEmailOnUpdate.htm";
        private static readonly string SendDownloadFileLinksTemplate = "SendDownloadFileLinks.htm";
        private static readonly string SendProductPurchaseEmailTemplate = "SendProductPurchaseEmail.htm";
        private static readonly string SendProductPurchaseEmailToSellerTemplate = "SendProductPurchaseEmailToSeller.htm";
        private static readonly string filesTemplate = "files.htm";
        private static readonly string productDetailsTemplate = "productDetails.htm";
        private static readonly string VerifyEmailOnUpdateSubject = "Verify your email";
        private static readonly string SendBuyerRefundEmailTemplate = "BuyerRefund.htm";
        private static readonly string customReleasePaymentTemplate = TemplateFolder + "customReleasePayment.htm";
        private static readonly string reportEmailTemplate = TemplateFolder + "ReportMail.html";
        private static readonly string JobConfirmationTemplate = TemplateFolder + "JobConfirmation.htm";
        private static readonly string winnerEmailTemplate = "Winner.htm";
        private static readonly string SendContestRequestTemplate = TemplateFolder + "SendContestRequest.htm";

        private static readonly string UserRegistrationTemplate = "UserRegistration.htm";
        private static readonly string EmailVerificationTemplate = "VerifyEmail.htm";
        private static readonly string PasswordResetTemplate = "PasswordResetTemplate.htm";

        public static void TestEmail()
        {
            Email.SendEmail(Config.AdminEmail.Split(new char[] { ',' })[0], Config.AdminEmail.Split(new char[] { ',' }), null, null, "test", "test");
        }

        /// <summary>
        /// Send Email after user registration
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="verificationURL"></param>
        /// <returns></returns>
        public static void NormalUserRegistration(User obj, string verificationURL)
        {
            try
            {
                string template = Utility.ReadFileText(NormalUserRegistrationTemplate);
                template = template.Replace("{Name}", obj.FirstName + " " + obj.LastName)
                                   .Replace("{VerificationUrl}", verificationURL);

                Email.SendEmail(Config.AdminEmail.Split(new char[] { ',' })[0], obj.EmailId.Split(new char[] { ',' }), null, null, NormalUserRegistrationSubject, template);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Send password reset email
        /// </summary>
        /// <param name="login"></param>
        /// <param name="email"></param>
        /// <param name="passwordResetUrl"></param>
        /// <returns></returns>
        public static void ForgotPassword(string user_name, string email, string passwordResetUrl)
        {
            try
            {
                string template = Utility.ReadFileText(ForgotPasswordTemplate);
                template = template.Replace("{Name}", user_name)
                                   .Replace("{ResetUrl}", passwordResetUrl);

                Email.SendEmail(Config.AdminEmail.Split(new char[] { ',' })[0], email.Split(new char[] { ',' }), null, null, ForgotPasswordSubject, template);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Send Template less Emails from Admin
        /// </summary>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static void SendEmailWithoutTemplate(string[] to, string subject, string content)
        {
            Email.SendEmail(Config.AdminEmail, to, null, null, subject, content);
        }

        /// <summary>
        /// This will be use to send an verification email to an user in case of user updation
        /// </summary>
        /// <param name="user_name"></param>
        /// <param name="email"></param>
        /// <param name="email_verification_url"></param>
        public static void VerifyEmail(string user_name, string email, string email_verification_url)
        {
            try
            {
                string template = Utility.ReadFileText(VerifyEmailOnUpdateTemplate);
                template = template.Replace("{Name}", user_name)
                                   .Replace("{EmailVerification}", email_verification_url);

                Email.SendEmail(Config.AdminEmail.Split(new char[] { ',' })[0], email.Split(new char[] { ',' }), null, null, VerifyEmailOnUpdateSubject, template);
            }
            catch
            {
                throw;
            }
        }

        public static void SendDownloadFileLinks(List<Product> products, int user_id, string baseURL, string Emailid, string templateFoler)
        {
            string files = "";
            string fileTemplate = File.ReadAllText(templateFoler + filesTemplate);

            foreach (Product item in products)
            {
                files += fileTemplate.Replace("{URL}", baseURL + "/User/DownloadFile?user_id=" + user_id + "&product_id=" + item.ProductID)
                                     .Replace("{Item}", item.Title);
            }
            string template = Utility.ReadFileText(templateFoler + SendDownloadFileLinksTemplate);

            template = template.Replace("{Files}", files);
            Email.SendEmail(Config.AdminEmail.Split(new char[] { ',' })[0], Emailid.Split(new char[] { ',' }), null, null, "Centro Product files", template);
        }

        public static void SendProductPurchaseEmail(string Emailid, int order_id, ShopCart shop_cart, List<Product> all_products, string baseURL, string username, string templatePath)
        {
            SiteFee siteFee = Config.SiteFee;
            string files = "";
            string fileTemplate = File.ReadAllText(templatePath + SendProductPurchaseEmailTemplate);

            string items = File.ReadAllText(templatePath + productDetailsTemplate);
            short i = 0;
            foreach (Product item in all_products)
            {
                string image = string.Empty;
                image = item.PrimaryPicture != null ? baseURL + "/Images/ProductImages/" + shop_cart.ShopOwnerName + "/" + item.PrimaryPicture : baseURL + "/Content/images/default_product.png";
                int quantity = shop_cart.ShopCartItems.Where(m => m.ProductID == item.ProductID).FirstOrDefault().Quantity;
                files += items.Replace("{Title}", item.Title)
                              .Replace("{Image}", image)
                              .Replace("{Quantity}", quantity.ToString())
                              .Replace("{Price}", item.UnitPrice.ToString());
            }

            fileTemplate = fileTemplate.Replace("{OrderID}", order_id.ToString())
                                       .Replace("{BuyerName}", username)
                                       .Replace("{ShopOwner}", shop_cart.ShopOwnerName)
                                       .Replace("{ManageOrderURL}", baseURL + "/Shops/ManageOrders/B")
                                       .Replace("{FAQURL}", baseURL + "/FAQ")
                                       .Replace("{TaxValue}", Math.Round((shop_cart.ItemTotalPrice * (shop_cart.Tax.HasValue ? shop_cart.Tax.Value : 0) / 100), 2).ToString())
                                       .Replace("{FeePercent}", siteFee.SiteFee1.ToString())
                                       .Replace("{FeeValue}", Math.Round(siteFee.IsPercentage ? (shop_cart.ItemTotalPrice * siteFee.SiteFee1 / 100) : siteFee.SiteFee1, 2).ToString())
                                       .Replace("{TnCURL}", baseURL + "/TermsAndConditions")
                                       .Replace("{PPURL}", baseURL + "/PrivacyPolicy")
                                       .Replace("{Shipping}", shop_cart.ItemTotalShippingPrice.ToString())
                                       .Replace("{Tax}", Math.Round(shop_cart.Tax.HasValue ? shop_cart.Tax.Value : 0, 2).ToString())
                                       .Replace("{TotalAmountToBePaid}", Math.Round(shop_cart.TotalAmountToBePaid, 2).ToString())
                                       .Replace("{productDetails}", files);

            Email.SendEmail(Config.AdminEmail.Split(new char[] { ',' })[0], Emailid.Split(new char[] { ',' }), null, null, "Centro Shopping Details", fileTemplate);
        }

        public static void SendProductPurchaseEmailToSeller(string Emailid, int order_id, ShopCart shop_cart, List<Product> all_products, string baseURL, string username, string templatePath)
        {
            SiteFee siteFee = Config.SiteFee;
            string files = "";
            string fileTemplate = File.ReadAllText(templatePath + SendProductPurchaseEmailToSellerTemplate);

            string items = File.ReadAllText(templatePath + productDetailsTemplate);
            short i = 0;
            foreach (Product item in all_products)
            {
                string image = string.Empty;
                image = item.PrimaryPicture != null ? baseURL + "/Images/ProductImages/" + shop_cart.ShopOwnerName + "/" + item.PrimaryPicture : baseURL + "/Content/images/default_product.png";
                files += items.Replace("{Title}", item.Title)
                                     .Replace("{Image}", image)
                                     .Replace("{Quantity}", shop_cart.ShopCartItems[i++].Quantity.ToString())
                                     .Replace("{Price}", item.UnitPrice.ToString());
            }

            fileTemplate = fileTemplate.Replace("{OrderID}", order_id.ToString())
                                       .Replace("{BuyerName}", username)
                                       .Replace("{Shipping}", Math.Round(shop_cart.ItemTotalShippingPrice, 2).ToString())
                                       .Replace("{Tax}", Math.Round(shop_cart.Tax.HasValue ? shop_cart.Tax.Value : 0, 2).ToString())
                                       .Replace("{TotalAmountToBePaid}", Math.Round(shop_cart.TotalAmountToBePaid, 2).ToString())
                                       .Replace("{productDetails}", files)
                                       .Replace("{TaxValue}", Math.Round((shop_cart.ItemTotalPrice * (shop_cart.Tax.HasValue ? shop_cart.Tax.Value : 0) / 100), 2).ToString())
                                       .Replace("{FeePercent}", siteFee.SiteFee1.ToString())
                                       .Replace("{FeeValue}", Math.Round(siteFee.IsPercentage ? (shop_cart.ItemTotalPrice * siteFee.SiteFee1 / 100) : siteFee.SiteFee1, 2).ToString())
                                       .Replace("{name}", files);

            Email.SendEmail(Config.AdminEmail.Split(new char[] { ',' })[0], Emailid.Split(new char[] { ',' }), null, null, "Centro Shopping Details", fileTemplate);
        }

        public static void SendBuyerRefundEmail(BillingAddress buyer, Order order, string base_url)
        {
            string template = File.ReadAllText(base_url + SendBuyerRefundEmailTemplate);
            template = template.Replace("{OrderID}", order.OrderID.ToString());

            Email.SendEmail(Config.AdminEmail.Split(new char[] { ',' })[0], buyer.Email.Split(new char[] { ',' }), null, null, "Refund for Order # " + order.OrderID, template);
        }

        public static void test(string base_url)
        {
            string template = File.ReadAllText(base_url + SendBuyerRefundEmailTemplate);
        }

        /*
        public static void SendInvoiceEcsrowedEmailToSeller(string p, Invoice invoice, string baseURL, string username)
        {
            throw new NotImplementedException();
        }
        */

        public static void SendCustomReleasePaymentEmail(User seller, Invoice invoice)
        {
            string template = Utility.ReadFileText(customReleasePaymentTemplate);
            template = template.Replace("{InvoiceID}", invoice.InvoiceID.ToString());

            Email.SendEmail(Config.AdminEmail.Split(new char[] { ',' })[0], seller.EmailId.Split(new char[] { ',' }), null, null, "Payment Released for Invoice # " + invoice.InvoiceID, template);
        }

        public static ActionOutput SendReportEmail(Report report)
        {
            string template = Utility.ReadFileText(reportEmailTemplate);
            if (report.UserID.HasValue)
            {
                User user = UsersHandler.GetUserByID(report.UserID.Value).Object;

                template = template.Replace("{UserName}", user.UserName);
                template = template.Replace("{UserEmail}", user.EmailId);
            }
            else
            {
                template = template.Replace("{UserName}", report.FirstName);
                template = template.Replace("{UserEmail}", report.EmailID);
            }
            template = template.Replace("{ReportType}", report.ReportType);
            template = template.Replace("{Message}", report.Message);
            string[] To = { "help@Centro.com" };
            Email.SendEmail(Config.AdminEmail.Split(new char[] { ',' })[0], To, null, null, "Report", template);
            return new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Message = "Report submitted successfully"
            };
        }

        public static void SendWinner(ContestParticipant winner, string title, User winnerUser, string base_url)
        {
            string template = File.ReadAllText(base_url + winnerEmailTemplate);
            template = template.Replace("{Name}", winnerUser.FirstName + " " + winnerUser.LastName)
                    .Replace("{ContestURL}", Config.SiteURL + "Contest/Entries/" + winner.ContestID)
                    .Replace("{ContestTitle}", winner.Contest.Title);

            Email.SendEmail(Config.AdminEmail.Split(new char[] { ',' })[0], winnerUser.EmailId.Split(','), null, null, "Centro :: Contest Winner", template);
        }

        internal static void JobConfirmation(PrototypeRequest req, User seller)
        {
            string template = Utility.ReadFileText(JobConfirmationTemplate);
            template = template.Replace("{Name}", seller.FirstName + " " + seller.LastName)
                               .Replace("{URL}", SiteURL.URL + "Message/MyCustomOrder/" + req.RequestID)
                               .Replace("{Title}", req.RequestTitle);

            Email.SendEmail(Config.AdminEmail.Split(new char[] { ',' })[0], seller.EmailId.Split(','), null, null, "Centro :: Job Confirmation", template);
        }

        internal static void SendContestRequest(ContestRequest model)
        {
            string template = Utility.ReadFileText(SendContestRequestTemplate);
            template = template.Replace("{Email}", model.Email)
                               .Replace("{Title}", model.Title)
                               .Replace("{Synosis}", model.Synosis)
                               .Replace("{Criteria}", model.Criteria)
                               .Replace("{IsAccepted}", model.IsAccepted ? "1" : "0");

            Email.SendEmail(model.Email, Config.AdminEmail.Split(new char[] { ',' }), null, null, "Centro :: Contest Request", template);
        }

        internal static void SendRegistrationEmail(User User, string Path, string SiteURL, string encodedURL)
        {
            string userTemplate = System.IO.File.ReadAllText(Path + EmailVerificationTemplate);
            userTemplate = userTemplate.Replace("{FirstName}", User.FirstName)
                                     .Replace("{LastName}", User.LastName)
                                     .Replace("{Email}", User.EmailId)
                                     .Replace("{VerificationUrl}", SiteURL + "/VerifyEmail/" + encodedURL.Replace(" ","-"));
            Email.SendEmail(Config.AdminEmail, User.EmailId.Split(' '), null, null, "Centro : Email Verification is required.", userTemplate);
        }

        internal static void SendPasswordResetEmail(PasswordResetThreadObject obj)
        {
            string userTemplate = System.IO.File.ReadAllText(obj.Path + PasswordResetTemplate);
            userTemplate = userTemplate.Replace("{FullName}", obj.FullName)
                                     .Replace("{ResetURL}", obj.SiteURL + "/Home/ResetPassword/" + obj.Email + "/" + obj.Code);
            Email.SendEmail(Config.AdminEmail, obj.Email.Split(' '), null, null, "Centro :: Password reset link.", userTemplate);
        }
    }
}
