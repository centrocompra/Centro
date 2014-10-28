using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Centro
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "VerificationEmail",
                url: "VerifyEmail/{VerificationCode}",
                defaults: new { controller = "Account", action = "VerifyEmail", area="" },
                namespaces: new string[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                name: "ProductByCategory",
                url: "Home/Products/{CategoryID}/{CategoryName}",
                defaults: new { controller = "Home", action = "Products", area = "" },
                namespaces: new string[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                name: "PassRest",
                url: "Home/ResetPassword/{Email}/{Code}",
                defaults: new { controller = "Home", action = "ResetPassword"},
                namespaces: new string[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                "InvoiceView",
                "Invoice/View/{id}/{userType}",
                new { controller = "Invoice", action = "View" }, new[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                "InvoiceRoute",
                "Invoice/Create/{id}/{req_id}",
                new { controller = "Invoice", action = "Create", id = "", req_id = 0 }, new[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                "InvoicesRoute",
                "Invoice/Invoices/{ID}/{MessageType}",
                new { controller = "Invoice", action = "Invoices", id = 1, MessageType = 0 }, new[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                "MessageTermsAndConditions",
                "Message/_TermsAndConditions/{InvoiceID}/{RequestID}",
                new { controller = "Message", action = "_TermsAndConditions" }, new[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                "MessageInvoicesRoute",
                "Message/Invoices/{ID}/{MessageType}",
                new { controller = "Message", action = "Invoices", id = 1, MessageType = 0 }, new[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                "MyCustomOrder",
                "Message/MyCustomOrder/{id}/{from}",
                new { controller = "Message", action = "MyCustomOrder", id = 0, from = "" }, new[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                "BuyerCustomOrder",
                "Message/BuyerCustomOrder/{id}/{from}",
                new { controller = "Message", action = "BuyerCustomOrder", id = 0, from = "" }, new[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                 "SubCategoryProduct",
                 "Products/SubCategory/{id}/{cat}/{name}",
                 new { controller = "Products", action = "SubCategory"}, new[] { "Centro.Controllers" }
             );

            routes.MapRoute(
                 "ExclusiveDeals",
                 "Products/ExclusiveDeals/{id}/{itemid}",
                 new { controller = "Products", action = "ExclusiveDeals" }, new[] { "Centro.Controllers" }
             );

            routes.MapRoute(
                 "TypeProduct",
                 "Products/Type/{id}/{cat}/{name}/{type}",
                 new { controller = "Products", action = "Type" }, new[] { "Centro.Controllers" }
             );

            routes.MapRoute(
                 "ProductContests",
                 "Products/Contests/{shopname}/{shopid}",
                 new { controller = "Products", action = "Contests" }, new[] { "Centro.Controllers" }
             );

            routes.MapRoute(
                 "Product",
                 "Products/{shopname}/{shopid}/{categoryid}/{product_id}",
                 new { controller = "Products", action = "Product", product_id = 0 }, new[] { "Centro.Controllers" }
             );
            
            routes.MapRoute(
                 "OrderListing",
                 "Shop/_OrderListing/{id}/{status}/{type}",
                 new { controller = "Shops", action = "_OrderListing", id = 0, type = "B", status = "" }, new[] { "Centro.Controllers" }
             );

            routes.MapRoute(
                 "OrderDetails",
                 "Shop/OrderDetail/{id}/{type}",
                 new { controller = "Shops", action = "OrderDetail", id = 0, type = "B" }, new[] { "Centro.Controllers" }
             );

            routes.MapRoute(
                 "Shops",
                 "Shop/{id}/{shopid}",
                 new { controller = "Shops", action = "Shop", shopid = 0 }, new[] { "Centro.Controllers" }
             );

            routes.MapRoute(
                 "ShopAvailability",
                 "{ShopName}/Availability/{ShopID}",
                 new { controller = "Shops", action = "ShopAvailability", ShopName = "",ShopID=0 }, new[] { "Centro.Controllers" }
             );

            routes.MapRoute(
                 "Payment",
                 "Payment/Checkout/{ShopName}",
                 new { controller = "Payment", action = "Checkout", ShopName = "" }, new[] { "Centro.Controllers" }
             );

            routes.MapRoute(
                "Shop",
                "Shops/Services/{ServiceName}",
                new { controller = "Shops", action = "Services", ServiceName = "" }, new[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                 "HubDetails",
                 "Hubs/{UserName}/{HubTopic}/{hubid}/{HubTitle}",
                 new { controller = "Hub", action = "HubDetails", UserName = "", HubTopic = "", HubTitle = "", hubid=0 }, new[] { "Centro.Controllers" }
             );

            routes.MapRoute(
                 "EditHub",
                 "Hub/EditHub/{HubID}",
                 new { controller = "Hub", action = "EditHub", HubID = 0 }, new[] { "Centro.Controllers" }
             );

            routes.MapRoute(
                "HubByTopic",
                "HubByTopic/{TopicName}/{TopicID}",
                new { controller = "Hub", action = "HubsByTopic", TopicName = "", TopicID = 0 }, new[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                "UserHub",
                "{UserName}/Hubs",
                new { controller = "Hub", action = "UserHub", UserName = ""}, new[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                 "ShopFeedbackDetail",
                 "{ShopName}/FeedBack/{ShopId}",
                 new { controller = "FeedBack", action = "ShopFeedBack", ShopName = "", ShopId = 0 }, new[] { "Centro.Controllers" }
             );


            routes.MapRoute(
                 "Contest3",
                 "Contest/ViewContest/{UserName}/{id}/{CategoryName}",
                 new { controller = "Contest", action = "ViewContest" },
                 new[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                 "Contest2",
                 "Contest/ViewContest/{id}/{CategoryName}",
                 new { controller = "Contest", action = "ViewContest" },
                 new[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                 "Contest1",
                 "Contest/ViewContest/{UserName}",
                 new { controller = "Contest", action = "ViewContest"},
                 new[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                 "Contest21",
                 "Contest/ViewContest",
                 new { controller = "Contest", action = "ViewContest" },
                 new[] { "Centro.Controllers" }
            );

           // routes.MapRoute(
           //     "UserContest",
           //     "Contest/{UserName}/{id}",
           //     new { controller = "Contest", action = "UserContest", name = "", id = 0, UserName = "" },
           //     new[] { "Centro.Controllers" }
           //);

            //routes.MapRoute(
            //     "MyContest",
            //     "Contest/MyContest/{id}/{name}",
            //     new { controller = "Contest", action = "MyContest", name = "", id = 0},
            //     new[] { "Centro.Controllers" }
            //);

            routes.MapRoute(
                 "ContestDetails",
                 "Contest/My/{id}/{name}",
                 new { controller = "Contest", action = "Details", name = "", id = 0 },
                 new[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                 "BuyerJob",
                 "User/Job/{id}/{title}",
                 new { controller = "User", action = "Job", name = "", id = 0, title="" },
                 new[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                 "CreateJob",
                 "User/CreateJob/{id}/{from}/{username}",
                 new { controller = "User", action = "CreateJob", id = 0, from = "", username="" },
                 new[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                 "BuyerMyJob",
                 "User/MyJob/{id}/{title}",
                 new { controller = "User", action = "MyJob", name = "", id = 0, title = "" },
                 new[] { "Centro.Controllers" }
            );

            routes.MapRoute(
                 "Default",
                 "{controller}/{action}/{id}",
                 new { controller = "Home", action = "Index", id = UrlParameter.Optional }, new[] { "Centro.Controllers" }
             );
        }
    }
}