using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Models.DataModel;
using System.Web;

namespace BusinessLayer.Classes
{
    public static class Config
    {
        public static string AdminEmail
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["AdminEmail"].ToString();
            }
        }

        public static string SiteURL
        {
            get
            {
                string baseURL = string.Empty;
                baseURL = "http://" + HttpContext.Current.Request.Url.Host + (HttpContext.Current.Request.Url.Port == 80 ? "/" : ":" + HttpContext.Current.Request.Url.Port + "/");
                return baseURL;
            }
        }

        public static string SalesTaxAPIKey
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["SalesTaxApiKey"].ToString();
            }
        }

        public static string AuthorizeLogin
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["AuthorizeLogin"].ToString();
            }
        }

        public static string AuthorizeTransactionKey
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["AuthorizeTransactionKey"].ToString();
            }
        }

        public static bool SSL
        {
            get
            {
                return Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["SSL"].ToString());
            }
        }

        public static string EncryptionPassword
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["EncryptionPassword"];
            }
        }
        public static string FacebookKey
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["FacebookAppID"];
            }
        }
        public static string FacebookSecret
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["FacebookAppSecret"];
            }
        }

        public static string AdminPaypalBusinessAccount
        {
            get {
                return System.Configuration.ConfigurationManager.AppSettings["AdminPaypalBusinessAccount"];
            }
        }

        public static bool UseSandBox
        {
            get
            {
                return Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["UseSandBox"]);
            }
        }

        public static string PaypalBaseReturnURL
        {
            get
            {
                string baseURL = string.Empty;
                baseURL = "http://" + HttpContext.Current.Request.Url.Host + (HttpContext.Current.Request.Url.Port == 80 ? "/" : ":"+HttpContext.Current.Request.Url.Port + "/");
                return baseURL;
            }
        }

        public static SiteFee SiteFee
        {
            get
            {
                using (var context = new CentroEntities())
                {
                    return context.SiteFees.FirstOrDefault();
                }
            }
        }

        public static string TwitterKey
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["TwitterKey"];
            }
        }

        public static string TwitterSecret
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["TwitterSecret"];
            }
        }
    }
}
