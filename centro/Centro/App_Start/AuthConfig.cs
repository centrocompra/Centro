using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using BusinessLayer.Classes;
using System.Configuration;

namespace Centro
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166
            //Dictionary<string, object> facebook_extra_data = new Dictionary<string, object>();
            //facebook_extra_data.Add("Icon", "Images/fbConnect.png");

            Dictionary<string, object> facebook_extra_data = new Dictionary<string, object>();
            facebook_extra_data.Add("Icon", "Images/face.png");

            Dictionary<string, object> twitter_extra_data = new Dictionary<string, object>();
            twitter_extra_data.Add("Icon", "Images/twitter.png");
            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");

            OAuthWebSecurity.RegisterTwitterClient(
                consumerKey: Config.TwitterKey,
                consumerSecret: Config.TwitterSecret,
                displayName: "twitter",
                extraData: twitter_extra_data);

            OAuthWebSecurity.RegisterClient(
                 new MyCustomFacebookClient(
                      appId: ConfigurationManager.AppSettings["FacebookAppID"],
                 appSecret: ConfigurationManager.AppSettings["FacebookAppSecret"]),
                 displayName: "facebook",
                 extraData: facebook_extra_data);

            //OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
