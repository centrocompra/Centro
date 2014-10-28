using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessLayer.Classes
{
    public static class Cookies
    {
        public const string AdminCookie = "Centro___AdminCookie";
        public const string UserCookie = "Centro___UserCookie";
        public const string CaptchaCookie = "Centro___CaptchaCookie";
        public const string UserTempPictures = "Centro___UserTempPictures";
        public const string ProductTempFile = "Centro___ProductTempFile";
        public const string TempAttachments = "Centro___TempAttachments";
        public const string UserProfilePicture = "Centro___UserProfilePictures";
        public const string CartCookie = "Centro___CartCookie";
        public const string ReturnUrlCookie = "Centro___ReturnCookie";
        public const string PaymentTempCookie = "Centro___PaymentTempCookie";
        public const string TempFileAttachments = "Centro___FileAttachments";
        public const string UserPaykey = "Centro___UserPaykey";
        public const string DonationPaykey = "Centro___DonationPaykey";
        public const string UserTempHubPictures = "Centro___UserTempHubPictures";
        public const string ViewedPage = "Centro_____ViewedPage";
        public const string ViewHubPage = "Centro_____ViewHubPage";
        public const string JobsFilter = "Centro___JobsFilter";        
    }

    public static class HttpMethod
    {
        public const string GET = "GET";
        public const string POST = "POST";
    }

    public static class CentroDefaults
    {
        public const string DefaultPage = "Index";
        public const string SignInPage = "SignIn";
        public const string BuyerSection = "User/EditProfile";//"User";
        public const string SellerSection = "Seller";
        public const string AdminSection = "Admin";
        public const string BuyerDefaultPage = "Index";
        public const string SellerDefaultPage = "Index";
        public const string AdminDefaultPage = "Index";
    }

    public static class Constants
    {
        public const Int32 InvalidLoginAttempt = 500;
        public const Int32 BlockAcountDays = 7;
        public const Int32 InvalidLoginAttemptForCaptcha = 300;
        public const Int32 PageSize = 12;
    }

    public static class PaypalReturnActions
    {
        public const string NormalReturnAction = "Payment/PaypalReturn";
        public const string NormalCancelAction = "Cart/MyCart";
        public const string CustomReturnAction = "Payment/BuyerCustomOrderReturnFromPaypal/";
        public const string CustomCancelAction = "Message/BuyerCustomOrder/";
    }

    public static class ActivityText
    {
        public const string HubCreate = "<div class='activity-first-outer'><div class='activity-first'><a href=\"javascript:;\">{UserName}</a> posted a <a href=\"{Link}\">new article</a> on {Date}</div><div class='activity-title'><a href=\"{Link}\">{HubTitle}</a></div></div>";
        public const string HubComment = "<div class='activity-first-outer'><div class='activity-first'><a href=\"javascript:;\">{UserName}</a> commented on an <a href=\"{Link}\">article</a> on {Date}</div><div class='activity-title'><a href=\"{Link}\">{HubTitle}</a></div></div>";
        public const string ContestCreate = "<div class='activity-first-outer'><div class='activity-first'><a href=\"javascript:;\">{UserName}</a> posted a <a href=\"{Link}\">contest</a> on {Date}</div><div class='activity-title'><a href=\"{Link}\">{ContestTitle}</a></div></div>";
        public const string ContestWinner = "<div class='activity-first-outer'><div class='activity-first'><a href=\"javascript:;\">{UserName}</a> won the <a href=\"{Link}\">contest</a> on {Date}</div><div class='activity-title'><a href=\"{Link}\">{ContestTitle}</a></div></div>";
        public const string ContestComment = "<div class='activity-first-outer'><div class='activity-first'><a href=\"javascript:;\">{UserName}</a> commented on a <a href=\"{Link}\">contest</a> on {Date}</div><div class='activity-title'><a href=\"{Link}\">{ContestTitle}</a></div></div>";
        public const string ProductCreate = "<div class='activity-first-outer'><div class='activity-first'><a href=\"javascript:;\">{UserName}</a> added a <a href=\"{Link}\">new product</a> to their shop on {Date}</div><div class='activity-title'><a href=\"{Link}\">{ProductTitle}</a></div></div><div class='activity-price'>${ItemCost} USD</div>";
        public const string ProductUpdate = "<div class='activity-first-outer'><div class='activity-first'><a href=\"javascript:;\">{UserName}</a> updated a <a href=\"{Link}\">product</a> on {Date}</div><div class='activity-title'><a href=\"{Link}\">{ProductTitle}</a></div></div>";
        public const string JobCreate = "<div class='activity-first-outer'><div class='activity-first'><a href=\"javascript:;\">{UserName}</a> posted a <a href=\"{Link}\">new job listing</a> on {Date}</div><div class='activity-title'>hiring: <a href=\"{Link}\">{JobTitle}</a></div></div><div class='activity-price'>${Price} USD<br>Est. Fixed Price</div>";
        public const string JobUpdate = "<div class='activity-first-outer'><div class='activity-first'><a href=\"javascript:;\">{UserName}</a> updated a <a href=\"{Link}\">job listing</a> on {Date}</div><div class='activity-title'>hiring: <a href=\"{Link}\">{JobTitle}</a></div></div>";
    }

    /*
    public static class CCBillCode
    {
        public static Dictionary<int, string> DenialKey = new Dictionary<int, string>
        {
            {1,"Website is not available for signup"},
            {2,"Unable to determine website signup requirements"},
            {3,"Your card type is not accepted, please try another type of credit card"},
            {4,"Banking System Error"},
            {5,"The credit card you entered is not valid"},
            {6,"Please check to ensure you entered your expiration dateUsed to show individual corresponding yearly, monthly or daily dates for report data. The date function's format is year‐month‐day; for example, 2002‐01‐01. correctly"},
            {7,"Please check to ensure you entered your bank account number correctly"},
            {8,"Please check to ensure you entered your bank's routing number correctly"},
            {9,"Banking System Error, please try again"},
            {10,"Website has invalid pricing"},
            {11,"Transaction Declined"},
            {12,"You currently have a subscription and are unable to signup"},
            {13,"You have already had a free trial"},
            {14,"You must enter your CVV2 number on the back of your card"},
            {15,"Your account is currently being processed, please check the website you are joining to see if you have access. If not, please contact support@ccbill.com"},
            {16,"Subscription ID Provided is invalid"},
            {17,"Subscription ID does not exist in system"},
            {18,"Previous Transaction Attempt in request was declined"},
            {19,"You are not authorized to signup with the provided credentials"},
            {20,"No Decline"},
            {21,"You have already had a trial, please select a normal recurring membership option"},
            {22,"Error contacting bank, please try again later"},
            {23,"Invalid Credit Card Provided"},
            {24,"Transaction Denied by Bank"},
            {25,"Bank Error"},
            {26,"Card Processing Setup Incorrect for Client"},
            {27,"System Error, Please Try Again"},
            {28,"We are unable to process your transaction at this time. Please try again at a later time"},
            {29,"Card Expired"},
            {30,"We are unable to bill the telephone number provided for this transaction. Please return to the website and choose an alternate payment method"},
            {31,"Insufficient Funds"},
            {32,"You must provide CVV2 to complete transaction"},
            {33,"Unable to determine transaction type"},
            {34,"Error contacting bank, please try again later"},
            {35,"Card Declined at Pre‐Auth SC"},
            {36,"Unable To Contact Bank"},
            {37,"We currently do not process for your banks bin"},
            {38,"Transaction Refused by Issuing Bank"},
            {39,"You Have Submitted Too Many Times Today"},
            {40,"The Card you are using is not accepted by this Client"},
            {41,"Client Inactive"},
            {42,"Incorrect Address Provided"},
            {43,"We are unable to process your telephone billing transaction because your provider only allows for one charge, per telephone number, per day, and our records show that you have an existing daily charge to website and choose an alternative payment method.this telephone number. Please return to the"},
            {44,"We're sorry, at this time prepaid cards are not allowed. Please try a different card type."},
            {45,"Transaction requires additional approval: please refer to your confirmation e‐mail for further instructions."}
        };     
    }
    */
}
