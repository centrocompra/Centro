using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer.Classes;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Handler;
using System.Web.Script.Serialization;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using PayPal.AdaptivePayments.Model;
using PayPal.AdaptivePayments;

namespace Centro.Controllers
{
    //[Authorize]
    public class PaymentController : FrontEndBaseController
    {
        public ActionResult Index(string shop_name)
        {
            return View();
        }

        public JsonResult PreCheckout(int shop_id, int? country_id, int? state_id, int user_id, string note)
        {
            //BillingAddress obj = SellersHandler.PrimaryBillingAddress(user_id).Object;
            //if (obj == null)
            //{
            //    String url = "<a href=" + Url.Action("ManageBillingAddress", "Home") + ">Add Billing Address</a>";
            //    return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Please add at-least one billing address and make it primary!!!  " + url }, JsonRequestBehavior.AllowGet);
            //}
            HttpCookie cartCookie = Request.Cookies[Cookies.CartCookie];
            if (cartCookie != null)
            {
                //return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Cart has been expired!!!" }, JsonRequestBehavior.AllowGet);
                Cart cart = new JavaScriptSerializer().Deserialize<Cart>(cartCookie.Value);
                for (short i = 0; i < cart.ShopCart.Count; i++)
                {
                    if (cart.ShopCart[i].ShopID == shop_id)
                    {
                        cart.ShopCart[i].ShipToCountryID = country_id;
                        cart.ShopCart[i].ShipToStateID = state_id;
                        cart.ShopCart[i].NoteForShop = note;
                        UpdateCustomCookie(Cookies.CartCookie, false, new JavaScriptSerializer().Serialize(cart), 120);
                        break;
                    }
                }
                return Json(new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { SellersHandler.ShopByShopId(shop_id).Object.ShopName } }, JsonRequestBehavior.AllowGet);
            }
            return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Cart has been expired!" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Checkout(string ShopName)
        {
            HttpCookie cartCookie = Request.Cookies[Cookies.CartCookie];
            if (cartCookie == null)
                return RedirectToAction("Home", "User");

            int user_id = SiteUserDetails.LoggedInUser.Id;
            Shop shop = SellersHandler.ShopByShopName(ShopName).Object;

            Cart cart = new JavaScriptSerializer().Deserialize<Cart>(cartCookie.Value);
            // Fetching Particular ShopCart for Payment
            ShopCart shopCart = cart.ShopCart.Where(m => m.ShopID == shop.ShopID).FirstOrDefault();

            PaymentDetails model = new PaymentDetails();
            model.ShopID = shop.ShopID;

            // Getting Primary Billing and Shipping Address 
            BillingAddress billing = SellersHandler.PrimaryBillingAddress(user_id).Object;
            HttpCookie paymentCookie = Request.Cookies[Cookies.PaymentTempCookie];
            if (paymentCookie != null)
            {
                PaymentDetails paymentData = new JavaScriptSerializer().Deserialize<PaymentDetails>(paymentCookie.Value);
                if (billing == null) billing = UsersHandler.BillingViewModelToDBObject(paymentData.BillingAddress).Object;
            }
            if (billing != null)
            {
                ViewBag.BillingCountries = SellersHandler.CountryGetAll().List;
                ViewBag.BillingStates = SellersHandler.GetStateByCountryId(billing.CountryId).List;
                ViewBag.BillingCities = SellersHandler.GetCityByStateId(billing.StateId).List;
            }
            else
            {

                ViewBag.BillingCountries = SellersHandler.CountryGetAll().List;
                ViewBag.BillingStates = SellersHandler.GetStateByCountryId(0).List;
                ViewBag.BillingCities = SellersHandler.GetCityByStateId(0).List;
            }
            //ShippingAddress shipping = SellersHandler.PrimaryShippingAddress(user_id).Object;

            // Setting Billing Address
            if (billing != null) model.BillingAddress = UsersHandler.DBBillingObjectToBillingViewModel(billing).Object;
            else model.BillingAddress = new BillingAddressViewModel();

            // Check if any item is of shippable
            bool ShowShipping = false;
            foreach (var item in shopCart.ShopCartItems)
            {
                if (item.IsShippingAvailable) { ShowShipping = true; break; }
            }
            ViewBag.ShowShipping = ShowShipping;
            if (ShowShipping)
            {
                if (paymentCookie != null)
                {
                    PaymentDetails paymentData = new JavaScriptSerializer().Deserialize<PaymentDetails>(paymentCookie.Value);
                    model.ShippingAddress = paymentData.ShippingAddress;
                }
                else
                {
                    model.ShippingAddress = new ShippingAddressViewModel();
                    model.ShippingAddress.CountryID = shopCart.ShipToCountryID.Value;
                }
                if (shopCart.ShipToCountryID.Value > 0)
                {
                    // Get shipping country
                    var shippingCountry = SellersHandler.GetCountryByID(shopCart.ShipToCountryID.Value).Object;
                    ViewBag.ShippingCountry = shippingCountry.CountryName;
                }
                else
                {
                    // Get list of countries
                    ViewBag.ShippingCountries = SellersHandler.CountryGetAll().List;
                }
                if (shopCart.ShipToStateID.HasValue)
                {
                    //var shippingstate = SellersHandler.GetStateByZipCode(shopCart.ShipToStateID.Value).Object;
                    var shippingstate = SellersHandler.GetStateById(shopCart.ShipToStateID.Value).Object;
                    ViewBag.ShippingState = shippingstate.StateName;
                    model.ShippingAddress.StateID = shippingstate.StateID;
                    ViewBag.ShippingCities = SellersHandler.GetCityByStateId(shippingstate.StateID).List;
                }
                else if (model.ShippingAddress.CountryID > 0)
                {
                    var shippingStates = SellersHandler.GetStateByCountryId(model.ShippingAddress.CountryID).List;
                    ViewBag.ShippingStates = shippingStates;
                    ViewBag.ShippingCities = SellersHandler.GetCityByStateId(model.ShippingAddress.StateID).List;
                }
                else
                {
                    ViewBag.ShippingStates = SellersHandler.GetStateByCountryId(shopCart.ShipToCountryID.Value).List;
                    ViewBag.ShippingCities = SellersHandler.GetCityByStateId(0).List;
                }

            }
            if (billing != null)
            {
                ViewBag.BillingCountry = SellersHandler.GetCountryByID(billing.CountryId).Object.CountryName;
                string state = SellersHandler.GetStateById(billing.StateId).Object.StateName;
                ViewBag.BillingState = state;
                ViewBag.BillingCity = SellersHandler.GetCityById(billing.CityId).Object.CityName;
            }
            else
            {

            }
            model.UserID = user_id;
            return View(model);
        }

        public ActionResult Summary(PaymentDetails paymentDetails)
        {
            //if (Request["SaveBilling"] != null)
            //{
            // Save Billing Address Only and return
            UsersHandler.AddUpdateBillingAddress(SiteUserDetails.LoggedInUser.Id, new BillingAddress
            {
                BillingAddressID = paymentDetails.BillingAddress.BillingAddressID,
                CityId = paymentDetails.BillingAddress.CityId,
                CountryId = paymentDetails.BillingAddress.CountryId,
                Email = paymentDetails.BillingAddress.Email,
                FirstName = paymentDetails.BillingAddress.FirstName,
                IsPrimary = true,
                LastName = paymentDetails.BillingAddress.LastName,
                PostCode = paymentDetails.BillingAddress.PostCode,
                StateId = paymentDetails.BillingAddress.StateId,
                UserID = SiteUserDetails.LoggedInUser.Id,
                Address = paymentDetails.BillingAddress.Address
            });
            //    return Json(new ActionOutput { Status = ActionStatus.Successfull, ID = -2 });
            //}
            HttpCookie cartCookie = Request.Cookies[Cookies.CartCookie];
            if (cartCookie != null)
            {
                //return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Cart has been expired!!!" }, JsonRequestBehavior.AllowGet);
                Cart cart = new JavaScriptSerializer().Deserialize<Cart>(cartCookie.Value);
                // Fetching Particular ShopCart for Payment
                ShopCart ShopCart = cart.ShopCart.Where(m => m.ShopID == paymentDetails.ShopID).FirstOrDefault();
                ViewBag.ShopCart = ShopCart;
                // Check if any item is of shippable
                bool ShowShipping = false;
                foreach (var item in ShopCart.ShopCartItems)
                {
                    if (item.IsShippingAvailable) { ShowShipping = true; break; }
                }
                ViewBag.ShowShipping = ShowShipping;
                CreateCustomCookie(Cookies.PaymentTempCookie, false, new JavaScriptSerializer().Serialize(paymentDetails), 120);
                //TempData["PaymentDetails"] = paymentDetails;
                return View(paymentDetails);
            }
            return RedirectToAction("Home", "User");
        }

        public JsonResult _TermsAndConditions()
        {
            return Json(new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Results = new List<string>
                    {
                        RenderRazorViewToString("_TermsAndConditions", null)
                    }
            });
        }

        public JsonResult MakePayment()
        {
            HttpCookie temp = Request.Cookies["temp"];
            if (temp != null)
            {
                temp.Expires = DateTime.Now.AddDays(-1);
                Request.Cookies.Add(temp);
            }
            HttpCookie paymentCookie = Request.Cookies[Cookies.PaymentTempCookie];
            if (paymentCookie != null)
            {
                //PaymentDetails paymentDetails = (PaymentDetails)TempData["PaymentDetails"]; //new JavaScriptSerializer().Deserialize<PaymentDetails>(paymentCookie.Value);
                // Getting paypal id of seller and set in paymentDetails                
                PaymentDetails paymentDetails = new JavaScriptSerializer().Deserialize<PaymentDetails>(paymentCookie.Value);
                User seller = UsersHandler.UserByShopId(paymentDetails.ShopID).Object;
                paymentDetails.PaypalEmail = seller.PaypalID;
                // Change primary billing address
                var bill = UsersHandler.AddUpdateBillingAddress(SiteUserDetails.LoggedInUser.Id, UsersHandler.BillingViewModelToDBObject(paymentDetails.BillingAddress).Object);
                if (paymentDetails.BillingAddress.BillingAddressID == 0)
                {
                    paymentDetails.BillingAddress.BillingAddressID = bill.ID;
                    paymentDetails.BillingAddress.UserID = SiteUserDetails.LoggedInUser.Id;
                    UpdateCustomCookie(Cookies.PaymentTempCookie, false, new JavaScriptSerializer().Serialize(paymentDetails));
                }

                HttpCookie cartCookie = Request.Cookies[Cookies.CartCookie];
                if (cartCookie == null)
                    return Json(new ActionOutput { Status = ActionStatus.Error, Message = "Cart has been expired!!!" }, JsonRequestBehavior.AllowGet);

                Cart cart = new JavaScriptSerializer().Deserialize<Cart>(cartCookie.Value);
                // Fetching Particular ShopCart for Payment
                ShopCart shopCart = cart.ShopCart.Where(m => m.ShopID == paymentDetails.ShopID).FirstOrDefault();
                string url = Request.Url.Scheme + "://" + Request.Url.Host + ":" + Request.Url.Port;
                // Redirecting to Paypal
                var output = PaymentHandler.MakeTransactionUsingPaypal(paymentDetails, shopCart);
                if (output.Status != ActionStatus.Successfull)
                    return Json(output, JsonRequestBehavior.AllowGet);
                // Saving paykey to database
                PaymentHandler.SaveUserTransaction(new UserTransaction { BuyerID = SiteUserDetails.LoggedInUser.Id, Paykey = output.Results[1], ShopID = shopCart.ShopID });

                // Saving paykey to Cookie
                CreateCustomCookie(Cookies.UserPaykey, false, new JavaScriptSerializer().Serialize(output.Results[1]));

                //var oustput = PaymentHandler.MakePayment(paymentDetails, shopCart, url, CentroUsers.LoggedInUser.Username); // old code
                return Json(output, JsonRequestBehavior.AllowGet);
            }
            return Json(new ActionOutput { Message = "Something went wrong!!!", Status = ActionStatus.Error }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PaypalReturn()
        {
            HttpCookie temp = Request.Cookies["temp"];
            if (temp == null)
                CreateCustomCookie("temp", false, "1");
            else
                UpdateCustomCookie("temp", false, "2");
            HttpCookie temp1 = Request.Cookies["temp"];
            return View(Convert.ToInt32(temp1.Value));
        }

        public JsonResult OrderProcessing()
        {
            //Utility.WriteFile("~/Logs/test.txt", "Controller:-- " + DateTime.Now.ToString());
            // Check payment status
            HttpCookie cookie = Request.Cookies[Cookies.UserPaykey];
            if (cookie != null)
            {
                HttpCookie paymentCookie = Request.Cookies[Cookies.PaymentTempCookie];
                PaymentDetails paymentDetails = new JavaScriptSerializer().Deserialize<PaymentDetails>(paymentCookie.Value);

                HttpCookie cartCookie = Request.Cookies[Cookies.CartCookie];
                Cart cart = new JavaScriptSerializer().Deserialize<Cart>(cartCookie.Value);

                // Fetching Particular ShopCart for Payment
                ShopCart shopCart = cart.ShopCart.Where(m => m.ShopID == paymentDetails.ShopID).FirstOrDefault();
                string url = Request.Url.Scheme + "://" + Request.Url.Host + ":" + Request.Url.Port;

                PaymentDetailsRequest req = new PaymentDetailsRequest(new RequestEnvelope("en_US"));
                req.payKey = new JavaScriptSerializer().Deserialize<string>(cookie.Value);

                // All set. Fire the request            
                AdaptivePaymentsService service = new AdaptivePaymentsService();
                PaymentDetailsResponse resp = null;

                resp = service.PaymentDetails(req);

                // Display response values. 
                Dictionary<string, string> keyResponseParams = new Dictionary<string, string>();
                if (!(resp.responseEnvelope.ack == AckCode.FAILURE) && !(resp.responseEnvelope.ack == AckCode.FAILUREWITHWARNING))
                {
                    // Process the order in case of success
                    var output = PaymentHandler.ProcessOrder(paymentDetails, shopCart, url, SiteUserDetails.LoggedInUser.Username);

                    keyResponseParams.Add("Pay key", resp.payKey);
                    keyResponseParams.Add("Payment execution status", resp.status);
                    keyResponseParams.Add("Sender email", resp.senderEmail);

                    //Selenium Test Case
                    keyResponseParams.Add("Acknowledgement", resp.responseEnvelope.ack.ToString());
                    keyResponseParams.Add("Action Type", resp.actionType);

                    //displayResponse(context, "PaymentDetails", keyResponseParams, service.getLastRequest(), service.getLastResponse(),resp.error, redirectUrl);
                    string[] Response = service.getLastResponse().Split(new char[] { '&' });
                    Dictionary<string, string> ResponseDictionary = new Dictionary<string, string>();
                    foreach (string resp_item in Response)
                    {
                        string[] arr = resp_item.Split(new char[] { '=' });
                        ResponseDictionary.Add(arr[0].Trim(), arr[1].Trim());
                    }
                    // Updating UserPaykey db table based on Paykey
                    PaymentHandler.UpdateUserTransaction(new UserTransaction
                    {
                        Paykey = req.payKey,
                        OrderID = output.ID,
                        Message = ResponseDictionary["paymentInfoList.paymentInfo(0).transactionStatus"],
                        Status = ResponseDictionary["status"],
                        TransactionID = ResponseDictionary["paymentInfoList.paymentInfo(0).transactionId"]
                    });
                    return Json(new ActionOutput { Status = ActionStatus.Successfull, Message = "Order has been processed...", Results = new List<string> { output.Results[0], output.Results[1] } });
                    //return RedirectToAction("Thankyou", "Payment", new { id = output.Results[0], shop_id = output.Results[1] });
                }
                //return RedirectToAction("Index", "Error");
                return Json(new ActionOutput { Status = ActionStatus.Error });
            }
            //return RedirectToAction("Index", "Error");
            return Json(new ActionOutput { Status = ActionStatus.Error });
        }

        public ActionResult Thankyou(string id, int shop_id)
        {
           // Utility.WriteFile("~/Logs/test.txt", "Thankyou:-- " + DateTime.Now.ToString());
            HttpCookie temp = Request.Cookies["temp"];
            if (temp != null)
            {
                temp.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(temp);
            }
            HttpCookie cartCookie = Request.Cookies[Cookies.CartCookie];
            Cart cart = new JavaScriptSerializer().Deserialize<Cart>(cartCookie.Value);
            ShopCart shopCart = cart.ShopCart.Where(m => m.ShopID == shop_id).FirstOrDefault();
            ViewBag.OrderID = id;
            ViewBag.ShopName = SellersHandler.ShopByShopId(shopCart.ShopID).Object.ShopName;

            for (short i = 0; i < cart.ShopCart.Count; i++)
            {
                if (cart.ShopCart[i].ShopID == shop_id)
                {
                    // Remove shop cart
                    var shop = cart.ShopCart.Where(s => s.ShopID == shop_id).FirstOrDefault();
                    var products = cart.ShopCart[i].ShopCartItems.Where(m => m.IsShippingAvailable || m.IsDownloadable).ToList();
                    for (short j = 0; j < products.Count; j++)
                    {
                        cart.ShopCart[i].ShopCartItems.Remove(products[j]);
                    }
                    if (cart.ShopCart[i].ShopCartItems.Count == 0)
                        cart.ShopCart.Remove(shop);
                }
            }
            UpdateCustomCookie(Cookies.CartCookie, false, new JavaScriptSerializer().Serialize(cart), 120);
            var paymentTempCookie = Request.Cookies[Cookies.PaymentTempCookie];
            if (paymentTempCookie != null)
            {
                paymentTempCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(paymentTempCookie);
            }
            //return View();
            return RedirectToAction("ManageOrders", "Shops", new { id = "B" });
        }

        public JsonResult EscrowPayment(int InvoiceID, int RequestID)
        {
            Invoice invoice = InvoiceHandler.InvoiceById(InvoiceID).Object;
            Shop sellerShop = SellersHandler.ShopByUserId(invoice.SellerID.Value).Object;
            var output = PaymentHandler.MakeEscrowPaymentUsingPaypal(invoice, RequestID);
            if (output.Status != ActionStatus.Successfull)
                return Json(output, JsonRequestBehavior.AllowGet);
            // Saving paykey to database
            PaymentHandler.SaveUserTransaction(new UserTransaction { BuyerID = invoice.BuyerID.Value, Paykey = output.Results[1], ShopID = sellerShop.ShopID, InvoiceID = InvoiceID });

            // Saving paykey to Cookie
            CreateCustomCookie(Cookies.UserPaykey, false, new JavaScriptSerializer().Serialize(output.Results[1]));

            //var oustput = PaymentHandler.MakePayment(paymentDetails, shopCart, url, CentroUsers.LoggedInUser.Username); // old code
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BuyerCustomOrderReturnFromPaypal(int id)
        {
            HttpCookie temp = Request.Cookies["temp"];
            if (temp == null)
                CreateCustomCookie("temp", false, "1");
            else
                UpdateCustomCookie("temp", false, "2");
            HttpCookie temp1 = Request.Cookies["temp"];
            int[] values = { Convert.ToInt32(temp1.Value), id };
            return View("CustomOrderReturn", values);
        }

        public JsonResult CustomOrderProcessing()
        {
            HttpCookie temp = Request.Cookies["temp"];
            if (temp != null)
            {
                temp.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(temp);
            }
           // Utility.WriteFile("~/Logs/test.txt", "Controller:--CustomOrderProcessing-- " + DateTime.Now.ToString());
            // Check payment status
            HttpCookie cookie = Request.Cookies[Cookies.UserPaykey];
            if (cookie != null)
            {
                PaymentDetailsRequest req = new PaymentDetailsRequest(new RequestEnvelope("en_US"));
                req.payKey = new JavaScriptSerializer().Deserialize<string>(cookie.Value);

                // All set. Fire the request            
                AdaptivePaymentsService service = new AdaptivePaymentsService();
                PaymentDetailsResponse resp = null;

                resp = service.PaymentDetails(req);

                // Display response values. 
                Dictionary<string, string> keyResponseParams = new Dictionary<string, string>();
                if (!(resp.responseEnvelope.ack == AckCode.FAILURE) && !(resp.responseEnvelope.ack == AckCode.FAILUREWITHWARNING))
                {
                    string url = Request.Url.Scheme + "://" + Request.Url.Host + ":" + Request.Url.Port;
                    // Make Updation in Invoice table
                    var output = PaymentHandler.CustomProcessOrder(req.payKey, url, SiteUserDetails.LoggedInUser.Username);

                    keyResponseParams.Add("Pay key", resp.payKey);
                    keyResponseParams.Add("Payment execution status", resp.status);
                    keyResponseParams.Add("Sender email", resp.senderEmail);

                    //Selenium Test Case
                    keyResponseParams.Add("Acknowledgement", resp.responseEnvelope.ack.ToString());
                    keyResponseParams.Add("Action Type", resp.actionType);

                    //displayResponse(context, "PaymentDetails", keyResponseParams, service.getLastRequest(), service.getLastResponse(),resp.error, redirectUrl);
                    string[] Responses = service.getLastResponse().Split(new char[] { '&' });
                    Dictionary<string, string> ResponseDictionary = new Dictionary<string, string>();
                    foreach (string resp_item in Responses)
                    {
                        string[] arr = resp_item.Split(new char[] { '=' });
                        ResponseDictionary.Add(arr[0].Trim(), arr[1].Trim());
                    }
                    // Updating UserPaykey db table based on Paykey
                    PaymentHandler.UpdateUserTransaction(new UserTransaction
                    {
                        Paykey = req.payKey,
                        InvoiceID = output.ID,
                        Message = ResponseDictionary["paymentInfoList.paymentInfo(0).transactionStatus"],
                        Status = ResponseDictionary["status"],
                        TransactionID = ResponseDictionary["paymentInfoList.paymentInfo(0).transactionId"],
                        EscrowedOn = DateTime.Now
                    });
                    PaymentHandler.CustomReleasePayment(output.ID, Convert.ToInt32(output.Results[1]) , SiteUserDetails.LoggedInUser.Username);

                    return Json(new ActionOutput { Status = ActionStatus.Successfull, Message = "Custom Order has been processed..." });
                    //return RedirectToAction("Thankyou", "Payment", new { id = output.Results[0], shop_id = output.Results[1] });
                }
                //return RedirectToAction("Index", "Error");
                return Json(new ActionOutput { Status = ActionStatus.Error });
            }
            //return RedirectToAction("Index", "Error");
            return Json(new ActionOutput { Status = ActionStatus.Error });
        }

        public JsonResult ReleasePayment(int InvoiceID, int RequestID)
        {
            return Json(PaymentHandler.CustomReleasePayment(InvoiceID, RequestID, SiteUserDetails.LoggedInUser.Username), JsonRequestBehavior.AllowGet);
        }
    }
}
