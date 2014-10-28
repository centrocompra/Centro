using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.Collections.Generic;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Classes;
using System.Transactions;

using PayPal;
using PayPal.Exception;
using PayPal.Util;
using PayPal.AdaptivePayments;
using System.Configuration;
using PayPal.AdaptivePayments.Model;
using System.Web.Script.Serialization;
using System.Threading;

namespace BusinessLayer.Handler
{
    public static class PaymentHandler
    {
        public static ActionOutput ProcessOrder(PaymentDetails payment_details, ShopCart shop_cart, string baseURL, string username)
        {
            SiteFee siteFee = Config.SiteFee;
            User ShopOwner = UsersHandler.UserByShopId(shop_cart.ShopID).Object;
            //Utility.WriteFile("~/Logs/test.txt", "Handler:-- "+DateTime.Now.ToString());
            int order_id = 0;
            int user_id = 0;
            using (TransactionScope scope = new TransactionScope())
            using (var context = new CentroEntities())
            {
                //TransactionDetail transactionDetail;
                foreach (ShopCartItems item in shop_cart.ShopCartItems)
                {
                    // Getting Available Quantity of each product
                    Product prod = ProductsHandler.ProductById(item.ProductID).Object;
                    
                    if (!item.IsDownloadable && item.Quantity > prod.Quantity)
                    {
                        // Cancel the process and return with a sold out message
                        return new ActionOutput { ID = 0, Status = ActionStatus.Error, Message = "Selected items have been already sold out!!!" };
                    }
                    else
                    {
                        if (!prod.IsDownloadable && item.IsShippingAvailable && !prod.SendDownloadVia.HasValue)
                        {
                            //prod.Quantity = prod.Quantity - item.Quantity;
                            var pro = context.Products.Where(p => p.ProductID == item.ProductID).FirstOrDefault();
                            pro.Quantity = pro.Quantity - item.Quantity;
                            context.SaveChanges();
                        }
                    }
                    // Getting Available Quantity of each product
                }
                // Making monetory transaction
                // MakeTransactionUsingPaypal(payment_details, shop_cart);
                // transactionDetail = MakeTransaction(payment_details, shop_cart);
                // if (transactionDetail.ResponseCode != "1")
                //    return new ActionOutput { Status = ActionStatus.Error, Message = "Error: " + transactionDetail.Message };
                // Making monetory transaction
                int? shippingaddressid = null;
                // Saving ShippingAddress if present
                if (payment_details.ShippingAddress != null)
                {
                    ShippingAddress ship = new ShippingAddress();
                    ship.Address = payment_details.ShippingAddress.Address;
                    ship.CityID = payment_details.ShippingAddress.CityID;
                    ship.CountryID = payment_details.ShippingAddress.CountryID;
                    ship.Email = payment_details.ShippingAddress.Email;
                    ship.FirstName = payment_details.ShippingAddress.FirstName;
                    ship.LastName = payment_details.ShippingAddress.LastName;
                    ship.PostCode = payment_details.ShippingAddress.PostCode;
                    ship.StateID = payment_details.ShippingAddress.StateID;
                    ship.UserID = payment_details.UserID;

                    context.ShippingAddresses.AddObject(ship);
                    context.SaveChanges();
                    shippingaddressid = ship.ShippingAddressID;
                }

                // Saving Order to database
                Order order = new Order();
                order.BillingAddressId = payment_details.BillingAddress.BillingAddressID;
                order.CreatedOn = DateTime.Now;
                order.DiscountRate = 0;
                order.ItemTotalPrice = shop_cart.ItemTotalPrice;
                order.ItemTotalShippingPrice = shop_cart.ItemTotalShippingPrice;
                order.NotForShop = shop_cart.NoteForShop;
                order.OrderStatusId = (int)OrderStatus.Pending;
                //order.PaymentStatusId = Convert.ToInt32(transactionDetail.ResponseCode); //(int)PaymentStatus.Completed; // Check for correct status
                order.ShippingAddressId = shippingaddressid;
                order.ShippingStatusId = (int)ShippingStatus.NotShipped;
                order.ShipToCountryId = shop_cart.ShipToCountryID;
                //order.TransactionID = transactionDetail.TransactionID; // payment transaction id returning from payment gateway
                //order.TransactionMessage = transactionDetail.Message;// payment transaction message returning from payment gateway
                order.UserID = payment_details.UserID;
                order.TotalAmountToBePaid = shop_cart.TotalAmountToBePaid;
                order.Tax = shop_cart.Tax;
                order.AdminCommission = Config.SiteFee.SiteFee1;
                order.SellerId = UsersHandler.UserByShopId(shop_cart.ShopID).Object.UserID;
                order.IsPercentage = siteFee.IsPercentage;
                context.Orders.AddObject(order);
                context.SaveChanges();

                order_id = order.OrderID;
                user_id = order.UserID;
                // Saving Order details to database
                foreach (var item in shop_cart.ShopCartItems)
                {
                    OrderItem order_item = new OrderItem();
                    order_item.CreatedOn = DateTime.Now;
                    order_item.IsDownloabale = item.IsDownloadable;
                    order_item.IsShippingAvailable = item.IsShippingAvailable;
                    order_item.OrderID = order.OrderID;
                    order_item.ProductID = item.ProductID;
                    order_item.Quantity = item.Quantity;
                    order_item.ShopID = item.ShopID;
                    order_item.TotalShippingPrice = item.TotalShippingPrice;
                    order_item.UnitPrice = item.UnitPrice;
                    order_item.UnitShippingAfterFirst = item.UnitShippingAfterFirst;
                    order_item.UnitShippingPrice = item.UnitShippingPrice;
                    context.OrderItems.AddObject(order_item);
                    context.SaveChanges();
                }
                scope.Complete();
            }

            // log alert into database

            AccountActivityHandler.SaveAlert(new Alert
            {
                AlertForID = order_id,
                AlertLink = "/Shop/OrderDetail/" + order_id+"/S",
                AlertText = username + " has placed an order.",
                UserID = ShopOwner.UserID
            });

            // Email Notifications
            using (var context = new CentroEntities())
            {
                // send email for downloadable files
                List<int> downloadable_products = new List<int>();
                List<int> all_productIds = new List<int>();
                foreach (var item in shop_cart.ShopCartItems.Where(m => m.IsDownloadable).ToList())
                {
                    downloadable_products.Add(item.ProductID);
                }

                foreach (var item in shop_cart.ShopCartItems.Where(m => m.IsShippingAvailable || m.IsDownloadable).ToList())
                {
                    all_productIds.Add(item.ProductID);
                }

                if (downloadable_products.Count > 0)
                {
                    List<Product> products = context.Products.Where(m => downloadable_products.Contains(m.ProductID)).ToList();
                    //EmailHandler.SendDownloadFileLinks(products, user_id, baseURL, payment_details.BillingAddress.Email);
                    DownloadFileLinks d = new DownloadFileLinks();
                    d.BaseURL = baseURL;
                    d.Email = payment_details.BillingAddress.Email;
                    d.Products = products;
                    d.UserID = user_id;
                    d.TemplatePath = HttpContext.Current.Server.MapPath("~/EmailTemplates/");
                    Thread dThread = new Thread(DownloadFileLinks);
                    dThread.Start(d);
                }
                List<Product> all_products = context.Products.Where(m => all_productIds.Contains(m.ProductID)).ToList();

                // Email to buyer
                ProductPurchaseEmailBuyer b = new ProductPurchaseEmailBuyer();
                b.BaseURL = baseURL;
                b.Email = payment_details.BillingAddress.Email;
                b.OrderID = order_id;
                b.Products = all_products;
                b.ShopCart = shop_cart;
                b.Username = username;
                b.TemplatePath = HttpContext.Current.Server.MapPath("~/EmailTemplates/");
                Thread bThread = new Thread(ProductPurchaseEmailBuyer);
                bThread.Start(b);
                //EmailHandler.SendProductPurchaseEmail(payment_details.BillingAddress.Email, order_id, shop_cart, all_products, baseURL, username);

                // Email to seller
                ProductPurchaseEmailBuyer s = new ProductPurchaseEmailBuyer();
                s.BaseURL = baseURL;
                s.Email = payment_details.BillingAddress.Email;
                s.OrderID = order_id;
                s.Products = all_products;
                s.ShopCart = shop_cart;
                s.Username = username;
                s.TemplatePath = HttpContext.Current.Server.MapPath("~/EmailTemplates/");
                Thread sThread = new Thread(ProductPurchaseEmailSeller);
                sThread.Start(s);

                User seller = context.Users.Where(m => m.UserName == shop_cart.ShopOwnerName).FirstOrDefault();
                //EmailHandler.SendProductPurchaseEmailToSeller(seller.EmailId, order_id, shop_cart, all_products, baseURL, username);

                return new ActionOutput { ID = order_id, Status = ActionStatus.Successfull, Message = "Order has been processsed, Redirecting...", Results = new List<string> { order_id.ToString(), shop_cart.ShopID.ToString() } };
            }
            //return new ActionOutput { Status = ActionStatus.Error, Message = "Error: Error in processing your order" /*transactionDetail.Message*/ };
        }

        private static void ProductPurchaseEmailBuyer(object parameter)
        {
            ReleasePaymentScheduler(); // Making instant payment release to seller and admin
            ProductPurchaseEmailBuyer obj = parameter as ProductPurchaseEmailBuyer;
            EmailHandler.SendProductPurchaseEmail(obj.Email, obj.OrderID, obj.ShopCart, obj.Products, obj.BaseURL, obj.Username, obj.TemplatePath);
        }

        private static void ProductPurchaseEmailSeller(object parameter)
        {
            ProductPurchaseEmailBuyer obj = parameter as ProductPurchaseEmailBuyer;
            EmailHandler.SendProductPurchaseEmailToSeller(obj.Email, obj.OrderID, obj.ShopCart, obj.Products, obj.BaseURL, obj.Username, obj.TemplatePath);
        }

        private static void DownloadFileLinks(object parameter)
        {
            DownloadFileLinks obj = parameter as DownloadFileLinks;
            EmailHandler.SendDownloadFileLinks(obj.Products, obj.UserID, obj.BaseURL, obj.Email, obj.TemplatePath);
        }

        //public static TransactionDetail MakeTransaction(PaymentDetails payment, ShopCart shop_cart)
        //{
        //    // By default, this sample code is designed to post to our test server for
        //    // developer accounts: https://test.authorize.net/gateway/transact.dll
        //    // for real accounts (even in test mode), please make sure that you are
        //    // posting to: https://secure.authorize.net/gateway/transact.dll
        //    String post_url = "https://test.authorize.net/gateway/transact.dll";

        //    Dictionary<string, string> post_values = new Dictionary<string, string>();
        //    //the API Login ID and Transaction Key must be replaced with valid values
        //    post_values.Add("x_login", Config.AuthorizeLogin);
        //    post_values.Add("x_tran_key", Config.AuthorizeTransactionKey);
        //    post_values.Add("x_delim_data", "TRUE");
        //    post_values.Add("x_delim_char", "|");
        //    post_values.Add("x_relay_response", "FALSE");

        //    post_values.Add("x_type", "AUTH_CAPTURE");
        //    post_values.Add("x_method", "CC");
        //    post_values.Add("x_card_num", payment.CardNumber);
        //    post_values.Add("x_exp_date", payment.ExpiryMonth + (payment.ExpiryYear.Substring(0, 2)));

        //    post_values.Add("x_amount", shop_cart.TotalAmountToBePaid.ToString());
        //    post_values.Add("x_description", "Centro Sample Transaction");

        //    post_values.Add("x_first_name", payment.NameOnCard);
        //    post_values.Add("x_last_name", "");
        //    post_values.Add("x_address", payment.BillingAddress.Address);
        //    post_values.Add("x_state", payment.BillingAddress.StateName);
        //    post_values.Add("x_zip", payment.BillingAddress.PostCode.ToString());
        //    // Additional fields can be added here as outlined in the AIM integration
        //    // guide at: http://developer.authorize.net

        //    // This section takes the input fields and converts them to the proper format
        //    // for an http post.  For example: "x_login=username&x_tran_key=a1B2c3D4"
        //    String post_string = "";

        //    foreach (KeyValuePair<string, string> post_value in post_values)
        //    {
        //        post_string += post_value.Key + "=" + HttpUtility.UrlEncode(post_value.Value) + "&";
        //    }
        //    post_string = post_string.TrimEnd('&');

        //    // The following section provides an example of how to add line item details to
        //    // the post string.  Because line items may consist of multiple values with the
        //    // same key/name, they cannot be simply added into the above array.
        //    //
        //    // This section is commented out by default.
        //    /*
        //    string[] line_items = {
        //        "item1<|>golf balls<|><|>2<|>18.95<|>Y",
        //        "item2<|>golf bag<|>Wilson golf carry bag, red<|>1<|>39.99<|>Y",
        //        "item3<|>book<|>Golf for Dummies<|>1<|>21.99<|>Y"};

        //    foreach( string value in line_items )
        //    {
        //        post_string += "&x_line_item=" + HttpUtility.UrlEncode(value);
        //    }
        //    */

        //    // create an HttpWebRequest object to communicate with Authorize.net
        //    HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(post_url);
        //    objRequest.Method = "POST";
        //    objRequest.ContentLength = post_string.Length;
        //    objRequest.ContentType = "application/x-www-form-urlencoded";

        //    // post data is sent as a stream
        //    StreamWriter myWriter = null;
        //    myWriter = new StreamWriter(objRequest.GetRequestStream());
        //    myWriter.Write(post_string);
        //    myWriter.Close();

        //    // returned values are returned as a stream, then read into a string
        //    String post_response;
        //    HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
        //    using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
        //    {
        //        post_response = responseStream.ReadToEnd();
        //        responseStream.Close();
        //    }

        //    // the response string is broken into an array
        //    // The split character specified here must match the delimiting character specified above
        //    string[] response_array = post_response.Split('|');

        //    // the results are output to the screen in the form of an html numbered list.
        //    /*
        //    resultSpan.InnerHtml += "<OL> \n";
        //    foreach (string value in response_array)
        //    {
        //        resultSpan.InnerHtml += "<LI>" + value + "&nbsp;</LI> \n";
        //    }
        //    resultSpan.InnerHtml += "</OL> \n";
        //    */
        //    TransactionDetail details = new TransactionDetail();
        //    details.AuthorizationCode = response_array[4];
        //    details.CustomerID = response_array[12];
        //    details.Message = response_array[3];
        //    details.ResponseCode = response_array[0];
        //    details.TransactionID = response_array[6];
        //    details.UserID = payment.UserID;

        //    return details;
        //    // individual elements of the array could be accessed to read certain response
        //    // fields.  For example, response_array[0] would return the Response Code,
        //    // response_array[2] would return the Response Reason Code.
        //    // for a list of response fields, please review the AIM Implementation Guide
        //}

        public static ActionOutput MakeTransactionUsingPaypal(PaymentDetails payment, ShopCart shop_cart)
        {
            SiteFee siteFee = Config.SiteFee;
            ReceiverList receiverList = new ReceiverList();
            receiverList.receiver = new List<Receiver>();
            string action_type = "PAY_PRIMARY";
            decimal amnt_to_admin = siteFee.IsPercentage ? ((shop_cart.TotalAmountToBePaid * siteFee.SiteFee1) / 100) : siteFee.SiteFee1;

            /*Total Amount to Admin Account */
            Receiver rec1 = new Receiver(shop_cart.TotalAmountToBePaid);
            rec1.email = Config.AdminPaypalBusinessAccount;
            rec1.primary = true;

            /*Amount after deducting to Admin Commision to Seller */
            Receiver rec2 = new Receiver(Math.Round((shop_cart.TotalAmountToBePaid - amnt_to_admin), 2, MidpointRounding.ToEven));
            rec2.email = payment.PaypalEmail; // "anuj_merchant@xicom.biz";

            receiverList.receiver.Add(rec1);
            receiverList.receiver.Add(rec2);
            PayRequest req = new PayRequest(new RequestEnvelope("en_US"), action_type, Config.PaypalBaseReturnURL + PaypalReturnActions.NormalCancelAction, "USD", receiverList, Config.PaypalBaseReturnURL + PaypalReturnActions.NormalReturnAction);

            // All set. Fire the request            
            AdaptivePaymentsService service = new AdaptivePaymentsService();

            PayResponse resp = null;
            //TransactionDetail details = new TransactionDetail();

            resp = service.Pay(req);
            String PayKey = resp.payKey;
            String PaymentStatus = resp.paymentExecStatus;
            ResponseEnvelope ResponseEnvelope = resp.responseEnvelope;
            PayErrorList errorList = resp.payErrorList;
            List<ErrorData> errorData = resp.error;
            if (errorData.Count > 0)
            {
                return new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = errorData[0].message
                };
            }
            FundingPlan defaultFundingPlan = resp.defaultFundingPlan;
            WarningDataList warningDataList = resp.warningDataList;
            string redirectUrl = null;
            if (!(resp.responseEnvelope.ack == AckCode.FAILURE) &&
                !(resp.responseEnvelope.ack == AckCode.FAILUREWITHWARNING))
            {
                redirectUrl = ConfigurationManager.AppSettings["PAYPAL_REDIRECT_URL"] + "_ap-payment&paykey=" + resp.payKey;

            }
            return new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Message = "Redirecting to paypal...",
                Results = new List<string> { redirectUrl, resp.payKey }
            };
        }

        public static ActionOutput SaveUserTransaction(UserTransaction transaction)
        {
            using (var context = new CentroEntities())
            {
                transaction.CreatedOn = DateTime.Now;
                context.UserTransactions.AddObject(transaction);
                context.SaveChanges();
                return new ActionOutput { ID = transaction.ID, Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput UpdateUserTransaction(UserTransaction transaction)
        {
            using (var context = new CentroEntities())
            {
                var old_userpaykey = context.UserTransactions.Where(m => m.Paykey == transaction.Paykey).FirstOrDefault();
                old_userpaykey.OrderID = transaction.OrderID;
                old_userpaykey.Message = transaction.Message;
                old_userpaykey.Status = transaction.Status;
                old_userpaykey.TransactionID = transaction.TransactionID;
                old_userpaykey.EscrowedOn = transaction.EscrowedOn;
                context.SaveChanges();
                return new ActionOutput { ID = transaction.ID, Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput UpdateUserTransactionForReleasePayment(UserTransaction transaction)
        {
            using (var context = new CentroEntities())
            {
                var old_userpaykey = context.UserTransactions.Where(m => m.Paykey == transaction.Paykey).FirstOrDefault();
                old_userpaykey.ReleasedOn = transaction.ReleasedOn;
                context.SaveChanges();
                return new ActionOutput { ID = transaction.ID, Status = ActionStatus.Successfull };
            }
        }

        public static void ReleasePaymentScheduler()
        {
            using (var context = new CentroEntities())
            {
                var transactions = context.UserTransactions
                                          .Join(context.Orders,
                                                t => t.OrderID,
                                                o => o.OrderID,
                                                (t, o) => new { t, o })
                                          .Where(m => m.t.Status.ToUpper() == "INCOMPLETE" 
                                               // && (m.o.ShippingStatusId == (int)ShippingStatus.Shipped || m.o.ShippingStatusId == (int)ShippingStatus.Received)
                                           )
                                          .ToList();
                foreach (var transaction in transactions)
                {
                    ExecutePaymentRequest req = new ExecutePaymentRequest(new RequestEnvelope("en_US"), transaction.t.Paykey);
                    //Fix for release
                    // req.fundingPlanId = parameters["fundingPlanId"];

                    // All set. Fire the request            
                    AdaptivePaymentsService service = new AdaptivePaymentsService();
                    ExecutePaymentResponse resp = null;
                    resp = service.ExecutePayment(req);

                    // Display response values. 
                    Dictionary<string, string> keyResponseParams = new Dictionary<string, string>();
                    if (!(resp.responseEnvelope.ack == AckCode.FAILURE) && !(resp.responseEnvelope.ack == AckCode.FAILUREWITHWARNING))
                    {
                        transaction.t.Status = resp.paymentExecStatus;
                        decimal final_amount = transaction.o.TotalAmountToBePaid;
                        decimal admin_commission = transaction.o.AdminCommission;
                        User admin = context.Users.Where(u => u.RoleId == (int)UserRole.Administrator).FirstOrDefault();
                        admin.TotalEarning = admin.TotalEarning.HasValue ? admin.TotalEarning.Value : 0;
                        admin.TotalEarning += admin_commission; // (final_amount * admin_commission / 100);
                        User seller = context.Users.Join(context.Shops, u => u.UserID, s => s.UserId, (u, s) => new { u, s }).Where(m => m.s.ShopID == transaction.t.ShopID).Select(m => m.u).FirstOrDefault();
                        seller.TotalEarning = seller.TotalEarning.HasValue ? seller.TotalEarning.Value : 0;
                        seller.TotalEarning += final_amount - admin_commission;  // final_amount - (final_amount * admin_commission / 100);
                    }
                    else
                    {
                        foreach (var error in resp.error)
                        {
                            transaction.t.ErrorMessage += error.message + "-||-";
                        }
                    }
                    context.SaveChanges();
                    // Send payment release notification Email to seller and admin

                }
            }
        }

        public static void RefundPaymentScheduler(int minutes)
        {
            using (var context = new CentroEntities())
            {
                DateTime beforeTime = DateTime.Now.AddMinutes(-minutes);
                var transactions = context.UserTransactions
                                          .Join(context.Orders,
                                                t => t.OrderID,
                                                o => o.OrderID,
                                                (t, o) => new { t, o })
                                          .Where(m =>
                                              m.t.Status.ToUpper() == "INCOMPLETE" &&
                                              m.o.ShippingStatusId == (int)ShippingStatus.NotShipped &&
                                              m.o.CreatedOn < beforeTime &&
                                              m.o.ShipToCountryId != null)
                                          .ToList();
                foreach (var transaction in transactions)
                {
                    RefundRequest req = new RefundRequest(new RequestEnvelope("en_US"));
                    req.currencyCode = "USD";
                    req.payKey = transaction.t.Paykey;
                    //Fix for release
                    // req.fundingPlanId = parameters["fundingPlanId"];
                    // All set. Fire the request            
                    AdaptivePaymentsService service = new AdaptivePaymentsService();
                    RefundResponse resp = null;
                    resp = service.Refund(req);

                    // Display response values. 
                    Dictionary<string, string> keyResponseParams = new Dictionary<string, string>();
                    if (!(resp.responseEnvelope.ack == AckCode.FAILURE) && !(resp.responseEnvelope.ack == AckCode.FAILUREWITHWARNING))
                    {
                        transaction.t.Status = "REFUNDED";
                        //decimal final_amount = transaction.o.TotalAmountToBePaid;
                        //decimal admin_commission = transaction.o.AdminCommission;
                        //User admin = context.Users.Where(u => u.RoleId == (int)UserRole.Administrator).FirstOrDefault();
                        //admin.TotalEarning -= (final_amount * admin_commission / 100);
                        //User seller = context.Users.Join(context.Shops, u => u.UserID, s => s.UserId, (u, s) => new { u, s }).Where(m => m.s.ShopID == transaction.t.ShopID).Select(m => m.u).FirstOrDefault();
                        // seller.TotalEarning -= final_amount - (final_amount * admin_commission / 100);
                        transaction.o.OrderStatusId = (int)OrderStatus.Canceled;
                        transaction.t.RefundDate = DateTime.Now;
                        transaction.t.RefundStatus = resp.refundInfoList.refundInfo[0].refundStatus;
                        transaction.t.RefundCorrelationId = resp.responseEnvelope.correlationId;
                        List<OrderItem> order_items = context.OrderItems.Where(m => m.OrderID == transaction.o.OrderID).ToList();
                        List<int?> order_item_ids = order_items.Select(m => m.ProductID).ToList();
                        List<Product> items = context.Products.Where(m => order_item_ids.Contains(m.ProductID)).ToList();
                        for (short i = 0; i < items.Count; i++)
                        {
                            items[i].Quantity += order_items.Where(m => m.ProductID == items[i].ProductID).Select(m => m.Quantity).FirstOrDefault();
                        }
                    }
                    else
                    {
                        foreach (var error in resp.error)
                        {
                            transaction.t.ErrorMessage += error.message + "-||-";
                        }
                    }
                    context.SaveChanges();
                    var buyer = context.Users.Join(context.BillingAddresses,
                                                    u => u.UserID,
                                                    b => b.UserID,
                                                    (u, b) => new { u, b })
                                             .Where(m => m.u.UserID == transaction.t.BuyerID && m.b.IsPrimary).Select(m => m.b).FirstOrDefault();
                    // Send Refund email to buyer and seller
                    EmailHandler.SendBuyerRefundEmail(buyer, transaction.o, AppDomain.CurrentDomain.BaseDirectory);
                }



            }
        }

        public static ActionOutput MakeEscrowPaymentUsingPaypal(Invoice invoice, int RequestID)
        {
            SiteFee siteFee = Config.SiteFee;
            // Get invoice details            
            User seller;
            using (var context = new CentroEntities())
            {
                seller = context.Users.Where(m => m.UserID == invoice.SellerID).FirstOrDefault();
            }
            ReceiverList receiverList = new ReceiverList();
            receiverList.receiver = new List<Receiver>();
            string action_type = "PAY_PRIMARY";
            decimal amnt_to_admin = siteFee.IsPercentage ? ((invoice.InvoiceAmount.Value * siteFee.SiteFee1) / 100) : siteFee.SiteFee1;

            /*Total Amount to Admin Account */
            Receiver rec1 = new Receiver(invoice.InvoiceAmount.Value);
            rec1.email = Config.AdminPaypalBusinessAccount;
            rec1.primary = true;

            /*Amount after deducting to Admin Commision to Seller */
            Receiver rec2 = new Receiver(Math.Round((invoice.InvoiceAmount.Value - amnt_to_admin), 2, MidpointRounding.ToEven));
            rec2.email = seller.PaypalID; // "anuj_merchant@xicom.biz";

            receiverList.receiver.Add(rec1);
            receiverList.receiver.Add(rec2);
            PayRequest req = new PayRequest(new RequestEnvelope("en_US"), action_type, Config.PaypalBaseReturnURL + PaypalReturnActions.CustomCancelAction + RequestID, "USD", receiverList, Config.PaypalBaseReturnURL + PaypalReturnActions.CustomReturnAction + RequestID);

            // All set. Fire the request            
            AdaptivePaymentsService service = new AdaptivePaymentsService();

            PayResponse resp = null;
            //TransactionDetail details = new TransactionDetail();

            resp = service.Pay(req);
            String PayKey = resp.payKey;
            String PaymentStatus = resp.paymentExecStatus;
            ResponseEnvelope ResponseEnvelope = resp.responseEnvelope;
            PayErrorList errorList = resp.payErrorList;
            List<ErrorData> errorData = resp.error;
            if (errorData.Count > 0)
            {
                return new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = errorData[0].message
                };
            }
            FundingPlan defaultFundingPlan = resp.defaultFundingPlan;
            WarningDataList warningDataList = resp.warningDataList;
            string redirectUrl = null;
            if (!(resp.responseEnvelope.ack == AckCode.FAILURE) &&
                !(resp.responseEnvelope.ack == AckCode.FAILUREWITHWARNING))
            {
                redirectUrl = ConfigurationManager.AppSettings["PAYPAL_REDIRECT_URL"] + "_ap-payment&paykey=" + resp.payKey;

            }
            return new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Message = "Redirecting to paypal...",
                Results = new List<string> { redirectUrl, resp.payKey }
            };
        }

        public static ActionOutput CustomProcessOrder(string paykey, string baseURL, string username)
        {
            //Utility.WriteFile("~/Logs/test.txt", "Handler:-- " + DateTime.Now.ToString());
            using (var context = new CentroEntities())
            {
                UserTransaction transaction = context.UserTransactions.Where(m => m.Paykey == paykey).FirstOrDefault();
                // Change Invoice status
                Invoice invoice = context.Invoices.Where(m => m.InvoiceID == transaction.InvoiceID.Value).FirstOrDefault();
                invoice.Status = (int)InvoiceStatus.EscrowPayment;
                context.SaveChanges();

                // log alert into database
                AccountActivityHandler.SaveAlert(new Alert
                {
                    AlertForID = (int)invoice.RequestID,
                    AlertLink = "/Message/MyCustomOrder/" + invoice.RequestID + "/MyContracts",
                    AlertText = username + " has escrowed the payment of an invoice \"" + invoice.Title + "\".",
                    UserID = invoice.SellerID.Value
                });
                // Email Notifications
                // send email for downloadable files
                // Email to seller
                //User seller = context.Users.Where(m => m.UserID == invoice.SellerID).FirstOrDefault();
                //EmailHandler.SendInvoiceEcsrowedEmailToSeller(seller.EmailId, invoice, baseURL, username);

                return new ActionOutput { ID = invoice.InvoiceID, Status = ActionStatus.Successfull, Message = "Custom Order has been processsed, Redirecting...", Results = new List<string> { invoice.InvoiceID.ToString(), invoice.RequestID.ToString() } };
            }
            //return new ActionOutput { Status = ActionStatus.Error, Message = "Error: Error in processing your order" /*transactionDetail.Message*/ };
        }

        public static ActionOutput CustomReleasePayment(int InvoiceID, int RequestID, string username)
        {
            SiteFee siteFee = Config.SiteFee;
            using (var context = new CentroEntities())
            {
                var transaction = context.UserTransactions
                                          .Join(context.Invoices,
                                                t => t.InvoiceID,
                                                i => i.InvoiceID,
                                                (t, i) => new { t, i })
                                          .Where(m => m.i.InvoiceID == InvoiceID && m.t.TransactionID != null && m.t.Status == "INCOMPLETE")
                                          .FirstOrDefault();

                ExecutePaymentRequest req = new ExecutePaymentRequest(new RequestEnvelope("en_US"), transaction.t.Paykey);
                //Fix for release
                // All set. Fire the request            
                AdaptivePaymentsService service = new AdaptivePaymentsService();
                ExecutePaymentResponse resp = null;
                resp = service.ExecutePayment(req);

                // Display response values. 
                Dictionary<string, string> keyResponseParams = new Dictionary<string, string>();
                if (!(resp.responseEnvelope.ack == AckCode.FAILURE) && !(resp.responseEnvelope.ack == AckCode.FAILUREWITHWARNING))
                {
                    transaction.t.Status = resp.paymentExecStatus;
                    transaction.i.Status = (int)InvoiceStatus.ReleasePayment;
                    // credit the amount in seller's total earning
                    User seller = context.Users.Where(m => m.UserID == transaction.i.SellerID).FirstOrDefault();
                    if (seller.TotalEarning == null) seller.TotalEarning = 0;
                    seller.TotalEarning += transaction.i.InvoiceAmount - (siteFee.IsPercentage ? ((transaction.i.InvoiceAmount.Value * siteFee.SiteFee1) / 100) : siteFee.SiteFee1);

                    User admin = context.Users.Where(m => m.RoleId == (int)UserRole.Administrator).FirstOrDefault();
                    admin.TotalEarning = admin.TotalEarning == null ? 0 : admin.TotalEarning;
                    admin.TotalEarning += siteFee.IsPercentage ? ((transaction.i.InvoiceAmount.Value * siteFee.SiteFee1) / 100) : siteFee.SiteFee1;
                    context.SaveChanges();

                    // Updating UserTransaction Table
                    PaymentHandler.UpdateUserTransactionForReleasePayment(new UserTransaction
                    {
                        Paykey = req.payKey,
                        ReleasedOn = DateTime.Now
                    });
                    Invoice invoice = context.Invoices.Where(m => m.InvoiceID == InvoiceID).FirstOrDefault();
                    // log alert into database
                    AccountActivityHandler.SaveAlert(new Alert
                    {
                        AlertForID = (int)invoice.RequestID,
                        AlertLink = "/Message/MyCustomOrder/" + invoice.RequestID + "/MyContracts",
                        AlertText = username + " has released the payment of an invoice \"" + invoice.Title + "\".",
                        UserID = invoice.SellerID.Value
                    });


                    // Send payment release notification Email to seller and admin

                    // Send Refund email to buyer and seller
                    EmailHandler.SendCustomReleasePaymentEmail(seller, transaction.i);
                    return new ActionOutput { Status = ActionStatus.Successfull, Message = "Payment has been released." };
                }
                else
                {
                    transaction.t.ErrorMessage = "";
                    foreach (var error in resp.error)
                    {
                        transaction.t.ErrorMessage += error.message + "-||-";
                    }
                    context.SaveChanges();
                    return new ActionOutput { Status = ActionStatus.Error, Message = transaction.t.ErrorMessage.Replace("-||-", "<br/>") };
                }

            }
        }

        public static void test()
        {
            EmailHandler.test(AppDomain.CurrentDomain.BaseDirectory);
        }

        public static ActionOutput MakeDonationUsingPaypal(decimal Amount, string ReturnUrl, string CancelUrl)
        {
            /*
            // # PayRequest
            // The code for the language in which errors are returned
            RequestEnvelope envelopeRequest = new RequestEnvelope();
            envelopeRequest.errorLanguage = "en_US";

            List<Receiver> listReceiver = new List<Receiver>();

            // Amount to be credited to the receiver's account
            Receiver receive = new Receiver(Amount);

            // A receiver's email address
            receive.email = Config.AdminPaypalBusinessAccount;
            listReceiver.Add(receive);
            ReceiverList listOfReceivers = new ReceiverList(listReceiver);

            // PayRequest which takes mandatory params:
            //  
            // * `Request Envelope` - Information common to each API operation, such
            // as the language in which an error message is returned.
            // * `Action Type` - The action for this request. Possible values are:
            // * PAY - Use this option if you are not using the Pay request in
            // combination with ExecutePayment.
            // * CREATE - Use this option to set up the payment instructions with
            // SetPaymentOptions and then execute the payment at a later time with
            // the ExecutePayment.
            // * PAY_PRIMARY - For chained payments only, specify this value to delay
            // payments to the secondary receivers; only the payment to the primary
            // receiver is processed.
            // * `Cancel URL` - URL to redirect the sender's browser to after
            // canceling the approval for a payment; it is always required but only
            // used for payments that require approval (explicit payments)
            // * `Currency Code` - The code for the currency in which the payment is
            // made; you can specify only one currency, regardless of the number of
            // receivers
            // * `Recevier List` - List of receivers
            // * `Return URL` - URL to redirect the sender's browser to after the
            // sender has logged into PayPal and approved a payment; it is always
            // required but only used if a payment requires explicit approval
            PayRequest requestPay = new PayRequest(envelopeRequest, "PAY", CancelUrl, "USD", listOfReceivers, ReturnUrl);
            //return requestPay;
            */


            ReceiverList receiverList = new ReceiverList();
            receiverList.receiver = new List<Receiver>();
            string action_type = "PAY"; // "PAY_PRIMARY";
            decimal amnt_to_admin = Amount;

            /*Total Amount to Admin Account */
            Receiver rec1 = new Receiver(Amount);
            rec1.email = Config.AdminPaypalBusinessAccount;
            //rec1.primary = true;

            /*Amount after deducting to Admin Commision to Seller */
            //Receiver rec2 = new Receiver(1);
            //rec2.email = Config.AdminPaypalBusinessAccount;

            receiverList.receiver.Add(rec1);
            //receiverList.receiver.Add(rec2);
            PayRequest req = new PayRequest(new RequestEnvelope("en_US"), action_type, CancelUrl, "USD", receiverList, ReturnUrl);

            // All set. Fire the request            
            AdaptivePaymentsService service = new AdaptivePaymentsService();

            PayResponse resp = null;
            //TransactionDetail details = new TransactionDetail();

            resp = service.Pay(req);
            String PayKey = resp.payKey;
            String PaymentStatus = resp.paymentExecStatus;
            ResponseEnvelope ResponseEnvelope = resp.responseEnvelope;
            PayErrorList errorList = resp.payErrorList;
            List<ErrorData> errorData = resp.error;
            if (errorData.Count > 0)
            {
                return new ActionOutput
                {
                    Status = ActionStatus.Error,
                    Message = errorData[0].message
                };
            }
            FundingPlan defaultFundingPlan = resp.defaultFundingPlan;
            WarningDataList warningDataList = resp.warningDataList;
            string redirectUrl = null;
            if (!(resp.responseEnvelope.ack == AckCode.FAILURE) &&
                !(resp.responseEnvelope.ack == AckCode.FAILUREWITHWARNING))
            {
                redirectUrl = ConfigurationManager.AppSettings["PAYPAL_REDIRECT_URL"] + "_ap-payment&paykey=" + resp.payKey;

            }
            return new ActionOutput
            {
                Status = ActionStatus.Successfull,
                Message = "Redirecting to paypal...",
                Results = new List<string> { redirectUrl, resp.payKey }
            };
        }

        public static ActionOutput SaveDonationTransaction(DonationTransaction transaction)
        {
            using (var context = new CentroEntities())
            {
                transaction.CreatedOn = DateTime.Now;
                context.DonationTransactions.AddObject(transaction);
                context.SaveChanges();
                return new ActionOutput { ID = transaction.ID, Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput UpdateDonationTransaction(DonationTransaction transaction)
        {
            using (var context = new CentroEntities())
            {
                var contests = context.DonationTransactions.Where(m => m.ContestID == transaction.ContestID).ToList();
                var old_paykey = contests.Where(m => m.Paykey == transaction.Paykey).FirstOrDefault();
                old_paykey.Message = transaction.Message;
                old_paykey.Status = transaction.Status;
                old_paykey.TransactionID = transaction.TransactionID;

                var contest = context.Contests.Where(m => m.ContestID == old_paykey.ContestID).FirstOrDefault();
                contest.Fund = 0;
                foreach (var item in contests)
                {
                    contest.Fund += item.Amount;
                }
                context.SaveChanges();
                return new ActionOutput { ID = transaction.ID, Status = ActionStatus.Successfull };
            }
        }
    }
}