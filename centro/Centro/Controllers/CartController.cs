using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Classes;
using System.Web.Script.Serialization;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Handler;
using System.Net;
using System.Collections.Specialized;
using System.IO;

namespace Centro.Controllers
{
    ////[Authorize]
    public class CartController : FrontEndBaseController
    {
        [SkipAuthentication]
        public JsonResult AddToCart(int shop_id, int product_id, int quantity, bool update_quantity_from_dropDown = false)
        {
            if (SiteUserDetails.LoggedInUser == null)
            {
                Category cat = ProductsHandler.CategoryByProduct(product_id).Object;
                Shop shop = SellersHandler.ShopByShopId(shop_id).Object;
                CreateCustomCookie(Cookies.ReturnUrlCookie, false, "/Products/" + Utility.SpacesToHifen(shop.ShopName) + "/" + shop_id + "/" + cat.CategoryID + "/" + product_id, 20);
                return Json(new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { Url.Action("Signin", "Home") } });
            }
            int user_id = SiteUserDetails.LoggedInUser.Id;
            HttpCookie cartCookie = Request.Cookies[Cookies.CartCookie];

            if (cartCookie != null)
            {
                // Getting data from cart cookie
                Product product = ProductsHandler.ProductById(product_id).Object;
                Cart cart = new JavaScriptSerializer().Deserialize<Cart>(cartCookie.Value);
                cart.UserID = user_id;
                bool isNewShop = true;
                foreach (var shopcart in cart.ShopCart)
                {
                    shopcart.Tax = shopcart.Tax.HasValue ? shopcart.Tax : 0;
                    shopcart.ShopOwnerName = SellersHandler.ShopOwnerByShopId(shop_id).Object.UserName;
                    if (shopcart.ShopID == shop_id)
                    {
                        isNewShop = false;
                        shopcart.ItemTotalPrice = product.UnitPrice.Value * quantity;
                        shopcart.ItemTotalShippingPrice = 0;
                        shopcart.NoteForShop = "";
                        shopcart.ShipToCountryID = shopcart.ShipToCountryID.HasValue ? shopcart.ShipToCountryID.Value : 0;
                        shopcart.ShopID = shop_id;
                        //shopcart.TotalAmountToBePaid += shopcart.ItemTotalPrice;
                        //shopcart.TotalAmountToBePaid = Math.Round(shopcart.TotalAmountToBePaid, 2);
                        // check for new product of same shop
                        bool isNewProduct = true;
                        for (short i = 0; i < shopcart.ShopCartItems.Count; i++)
                        {
                            if (shopcart.ShopCartItems[i].ProductID == product_id)
                            {
                                isNewProduct = false;
                                if (update_quantity_from_dropDown && !shopcart.ShopCartItems[i].SendDownloadVia.HasValue)
                                    shopcart.ShopCartItems[i].Quantity = quantity;
                                else if (shopcart.ShopCartItems[i].SendDownloadVia.HasValue)
                                {
                                    shopcart.ShopCartItems[i].Quantity = quantity;
                                }
                                else if (shopcart.ShopCartItems[i].Quantity < product.Quantity)
                                {
                                    shopcart.ShopCartItems[i].Quantity++;
                                    shopcart.TotalAmountToBePaid += shopcart.ItemTotalPrice;
                                    shopcart.TotalAmountToBePaid = Math.Round(shopcart.TotalAmountToBePaid, 2);
                                }
                                break;
                            }
                        }
                        if (isNewProduct)
                        {
                            int cartOrder = shopcart.ShopCartItems.Max(m => m.CartOrder);
                            shopcart.ShopCartItems.Add(new ShopCartItems
                            {
                                CartOrder = cartOrder + 1,
                                ProductID = product_id,
                                Quantity = quantity,
                                QuantityInStock = product.Quantity,
                                TotalShippingPrice = 0,
                                UnitShippingPrice = 0,
                                UnitShippingAfterFirst = 0,
                                ShipToCountries = ProductsHandler.ShippingCountryByProductId(product_id).List,
                                ShopID = shop_id,
                                UnitPrice = product.UnitPrice.Value,
                                IsShippingAvailable = product.IsDownloadable ? false : true,
                                IsDownloadable = product.IsDownloadable,
                                SendDownloadVia = product.SendDownloadVia
                            });
                            //shopcart.ItemTotalPrice += product.UnitPrice.Value * quantity;
                        }
                    }
                }
                if (isNewShop)
                {
                    ShopCart shopcart = new ShopCart();
                    shopcart.ItemTotalPrice += product.UnitPrice.Value * quantity;
                    shopcart.ItemTotalShippingPrice = 0;
                    shopcart.NoteForShop = "";
                    shopcart.TotalAmountToBePaid += shopcart.ItemTotalPrice;
                    shopcart.TotalAmountToBePaid = Math.Round(shopcart.TotalAmountToBePaid, 2);
                    shopcart.ShopOwnerName = SellersHandler.ShopOwnerByShopId(shop_id).Object.UserName;
                    //shopcart.ShipFromCountryID = 0;
                    shopcart.ShipToCountryID = 0;
                    shopcart.ShopID = shop_id;
                    shopcart.ShopCartItems = new List<ShopCartItems>();
                    shopcart.ShopCartItems.Add(new ShopCartItems
                    {
                        CartOrder = 1, // first product in shop
                        ProductID = product_id,
                        Quantity = quantity,
                        QuantityInStock = product.Quantity,
                        TotalShippingPrice = 0,
                        UnitShippingPrice = 0,
                        UnitShippingAfterFirst = 0,
                        ShipToCountries = ProductsHandler.ShippingCountryByProductId(product_id).List,
                        ShopID = shop_id,
                        UnitPrice = product.UnitPrice.Value,
                        IsShippingAvailable = product.IsDownloadable ? false : true,
                        IsDownloadable = product.IsDownloadable,
                        SendDownloadVia = product.SendDownloadVia
                    });
                    cart.ShopCart.Add(shopcart);
                }

                UpdateCustomCookie(Cookies.CartCookie, false, new JavaScriptSerializer().Serialize(cart), 120);
                // Save cart to database
                CartHandler.SaveCart(new JavaScriptSerializer().Serialize(cart), SiteUserDetails.LoggedInUser.Id);
            }
            else
            {
                Cart cart = new Cart();
                Product product = ProductsHandler.ProductById(product_id).Object;
                cart.UserID = user_id;
                List<ShopCart> shopcart = new List<ShopCart>();
                List<ShopCartItems> items = new List<ShopCartItems>();
                items.Add(new ShopCartItems
                       {
                           CartOrder = 1, // fisrt product in first shop in cart
                           ProductID = product_id,
                           Quantity = quantity,
                           QuantityInStock = product.Quantity,
                           TotalShippingPrice = 0,
                           UnitShippingPrice = 0,
                           UnitShippingAfterFirst = 0,
                           ShipToCountries = ProductsHandler.ShippingCountryByProductId(product_id).List,
                           ShopID = shop_id,
                           UnitPrice = product.UnitPrice.Value,
                           IsShippingAvailable = product.IsDownloadable ? false : true,
                           IsDownloadable = product.IsDownloadable,
                           SendDownloadVia = product.SendDownloadVia
                       });
                shopcart.Add(new ShopCart
                {
                    ItemTotalPrice = product.UnitPrice.Value * quantity,
                    ItemTotalShippingPrice = 0,
                    NoteForShop = "",
                    ShipToCountryID = 0,
                    ShopID = shop_id,
                    ShopCartItems = items,
                    ShopOwnerName = SellersHandler.ShopOwnerByShopId(shop_id).Object.UserName,
                    Tax = 0,
                    TotalAmountToBePaid = Math.Round(product.UnitPrice.Value * quantity, 2)
                });
                cart.ShopCart = shopcart;
                CreateCustomCookie(Cookies.CartCookie, false, new JavaScriptSerializer().Serialize(cart), 120);
                // Save cart to database
                CartHandler.SaveCart(new JavaScriptSerializer().Serialize(cart), SiteUserDetails.LoggedInUser.Id);
            }
            return Json(new ActionOutput { Status = ActionStatus.Successfull, Message = "Item has been added to your cart.", Results = new List<string> { Url.Action("MyCart", "Cart") } });
        }

        public ActionResult MyCart()
        {
            HttpCookie cartCookie = Request.Cookies[Cookies.CartCookie];
            Cart cart = new JavaScriptSerializer().Deserialize<Cart>(cartCookie.Value);
            for (short i = 0; i < cart.ShopCart.Count; i++)
            {
                for (short j = 0; j < cart.ShopCart[i].ShopCartItems.Count; j++)
                {
                    var prodId = cart.ShopCart[i].ShopCartItems[j].ProductID;
                    var item = ProductsHandler.GetProductByProductID(prodId).Object;
                    if (item == null)
                    {
                        var shopitem = cart.ShopCart[i].ShopCartItems.Where(p => p.ProductID == prodId).FirstOrDefault();
                        cart.ShopCart[i].ShopCartItems.Remove(shopitem);
                    }
                }
            }
            UpdateCustomCookie(Cookies.CartCookie, false, new JavaScriptSerializer().Serialize(cart), 120);
            var paymentTempCookie = Request.Cookies[Cookies.PaymentTempCookie];
            if (paymentTempCookie != null)
            {
                paymentTempCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(paymentTempCookie);
            }
            return View(new JavaScriptSerializer().Deserialize<Cart>(cartCookie.Value));
        }

        public JsonResult GetUSASTatesTax(int StateID, int ShopID)
        {
            //string ApiKey = Config.SalesTaxAPIKey;
            //var request = WebRequest.Create("http://api.zip-tax.com/request/v20?key=" + ApiKey + "&postalcode=" + postalcode);  // Live Sales Tax API Code # R9JDJFJ                                              
            //string text;
            //var response = (HttpWebResponse)request.GetResponse();

            //using (var sr = new StreamReader(response.GetResponseStream()))
            //{
            //    text = sr.ReadToEnd();
            //}
            var tax = CartHandler.SalesTax(StateID, ShopID);
            return Json(new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { tax.Tax.ToString() } }, JsonRequestBehavior.AllowGet);
        }

        public Cart UpdateCart(HttpCookie cartCookie, int? ship_to_country, int? ship_to_state, int shop_id, decimal? state_tax, int? stateID = null)
        {
            Cart cart = new JavaScriptSerializer().Deserialize<Cart>(cartCookie.Value);
            //foreach (ShopCart shopcart in cart.ShopCart)
            for (short s = 0; s < cart.ShopCart.Count; s++)
            {
                if (cart.ShopCart[s].ShopID == shop_id)
                {
                    int min_cart_order = cart.ShopCart[s].ShopCartItems.Min(m => m.CartOrder);
                    for (short i = 0; i < cart.ShopCart[s].ShopCartItems.Count; i++)
                    {
                        if (ship_to_country != null) //  && !cart.ShopCart[s].ShopCartItems[i].SendDownloadVia.HasValue)
                        {
                            var shipToCountry = cart.ShopCart[s].ShopCartItems[i].ShipToCountries.Where(m => m.CountryID == ship_to_country).FirstOrDefault();
                            if (shipToCountry != null)
                            {
                                // product shipping is available in selected country
                                cart.ShopCart[s].ShopCartItems[i].IsShippingAvailable = true;
                                cart.ShopCart[s].ShipToCountryID = ship_to_country.Value;
                                cart.ShopCart[s].ShopCartItems[i].UnitShippingPrice = shipToCountry.ShippingCost;
                                cart.ShopCart[s].ShopCartItems[i].UnitShippingAfterFirst = shipToCountry.ShippingCostAfterFirst;
                                if (min_cart_order == cart.ShopCart[s].ShopCartItems[i].CartOrder)
                                {
                                    if (cart.ShopCart[s].ShopCartItems.Count() == 1)
                                        cart.ShopCart[s].ShopCartItems[i].TotalShippingPrice = cart.ShopCart[s].ShopCartItems[i].UnitShippingPrice + cart.ShopCart[s].ShopCartItems[i].UnitShippingAfterFirst * (cart.ShopCart[s].ShopCartItems[i].Quantity - 1);
                                    else
                                        cart.ShopCart[s].ShopCartItems[i].TotalShippingPrice = cart.ShopCart[s].ShopCartItems[i].UnitShippingAfterFirst * (cart.ShopCart[s].ShopCartItems[i].Quantity);
                                }
                                else
                                {
                                    cart.ShopCart[s].ShopCartItems[i].TotalShippingPrice = cart.ShopCart[s].ShopCartItems[i].UnitShippingPrice + cart.ShopCart[s].ShopCartItems[i].UnitShippingAfterFirst * (cart.ShopCart[s].ShopCartItems[i].Quantity - 1);
                                }
                            }
                            else
                            {
                                // product shipping is not available in selected country
                                cart.ShopCart[s].ShopCartItems[i].IsShippingAvailable = false;
                                cart.ShopCart[s].ShopCartItems[i].UnitShippingPrice = 0;
                                cart.ShopCart[s].ShopCartItems[i].UnitShippingAfterFirst = 0;
                                cart.ShopCart[s].ShopCartItems[i].TotalShippingPrice = 0;
                                cart.ShopCart[s].Tax = cart.ShopCart[s].Tax.HasValue ? cart.ShopCart[s].Tax : 0;
                            }
                        }
                        else if (cart.ShopCart[s].ShopCartItems[i].SendDownloadVia.HasValue)
                        {
                            //
                        }
                        else
                        {
                            // setting default options
                            cart.ShopCart[s].ShopCartItems[i].UnitShippingPrice = 0;
                            cart.ShopCart[s].ShopCartItems[i].UnitShippingAfterFirst = 0;
                            cart.ShopCart[s].ShopCartItems[i].TotalShippingPrice = 0;
                            cart.ShopCart[s].ShipToCountryID = null;
                            cart.ShopCart[s].ShopCartItems[i].IsShippingAvailable = true;
                            cart.ShopCart[s].Tax = cart.ShopCart[s].Tax.HasValue ? cart.ShopCart[s].Tax : 0;
                        }
                        if (ship_to_country.HasValue)
                        {
                            if (ship_to_country.Value != 1)
                            {
                                var tax = SellersHandler.TaxByShopAndCountry(shop_id, ship_to_country.Value).Object;
                                cart.ShopCart[s].Tax = tax != null ? tax.Tax : 0;
                            }
                            else
                            {
                                if (state_tax.HasValue)
                                {
                                    //cart.ShopCart[s].Tax = Utility.CustomRound(state_tax.Value);
                                    cart.ShopCart[s].ShipToStateID = stateID.Value;
                                    cart.ShopCart[s].Tax = Math.Round(state_tax.Value, 2);
                                }
                            }
                        }
                        else
                        {
                            cart.ShopCart[s].Tax = cart.ShopCart[s].Tax.HasValue ? cart.ShopCart[s].Tax : 0;
                        }
                    }
                    cart.ShopCart[s].ItemTotalPrice = 0;
                    cart.ShopCart[s].ItemTotalShippingPrice = 0;
                    cart.ShopCart[s].TotalAmountToBePaid = 0;
                    for (short i = 0; i < cart.ShopCart[s].ShopCartItems.Count; i++)
                    {
                        if ((cart.ShopCart[s].ShopCartItems[i].IsShippingAvailable || cart.ShopCart[s].ShopCartItems[i].IsDownloadable) && ship_to_country != null)
                        {
                            // setting total shipping price
                            cart.ShopCart[s].ItemTotalPrice += cart.ShopCart[s].ShopCartItems[i].UnitPrice * cart.ShopCart[s].ShopCartItems[i].Quantity;
                            cart.ShopCart[s].ItemTotalShippingPrice += cart.ShopCart[s].ShopCartItems[i].TotalShippingPrice;
                            cart.ShopCart[s].TotalAmountToBePaid = Math.Round(cart.ShopCart[s].ItemTotalPrice * ((cart.ShopCart[s].Tax.Value / 100) + 1) + cart.ShopCart[s].ItemTotalShippingPrice, 2);
                            //cart.ShopCart[s].TotalAmountToBePaid = Math.Round(cart.ShopCart[s].ItemTotalPrice + cart.ShopCart[s].Tax.Value + cart.ShopCart[s].ItemTotalShippingPrice, 2);
                        }
                    }
                }
                SiteFee siteFee = Config.SiteFee;
                if (siteFee.IsPercentage)
                    cart.ShopCart[s].TotalAmountToBePaid += Math.Round(((cart.ShopCart[s].ItemTotalPrice * Convert.ToDecimal(siteFee.SiteFee1)) / 100), 2);
                else
                    cart.ShopCart[s].TotalAmountToBePaid += siteFee.SiteFee1;
            }
            UpdateCustomCookie(Cookies.CartCookie, false, new JavaScriptSerializer().Serialize(cart), 120);
            // Save cart to database
            CartHandler.SaveCart(new JavaScriptSerializer().Serialize(cart), SiteUserDetails.LoggedInUser.Id);
            int items_in_cart = 0;
            foreach (var shop in cart.ShopCart)
            {
                foreach (var item in shop.ShopCartItems)
                {
                    items_in_cart += item.Quantity;
                }
            }
            return cart;
        }

        public JsonResult UpdateToCart(int? ship_to_country, int? ship_to_state, int shop_id, decimal? state_tax, int? stateID = null)
        {
            HttpCookie cartCookie = Request.Cookies[Cookies.CartCookie];
            if (cartCookie != null)
            {
                Cart cart = UpdateCart(cartCookie, ship_to_country, ship_to_state, shop_id, state_tax, stateID);
                int items_in_cart = cart.ShopCart.Sum(m => m.ShopCartItems.Sum(a => a.Quantity));
                return Json(new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { RenderRazorViewToString("_Cart", cart), items_in_cart.ToString() } }, JsonRequestBehavior.AllowGet);
            }
            return Json(new ActionOutput { Status = ActionStatus.Error }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteProductFromCart(int shop_id, int product_id)
        {
            HttpCookie cookie = Request.Cookies[Cookies.CartCookie];
            if (cookie != null)
            {
                Cart cart = new JavaScriptSerializer().Deserialize<Cart>(cookie.Value);
                for (short i = 0; i < cart.ShopCart.Count; i++)
                {
                    if (cart.ShopCart[i].ShopID == shop_id)
                    {
                        for (short j = 0; j < cart.ShopCart[i].ShopCartItems.Count; j++)
                        {
                            var item = cart.ShopCart[i].ShopCartItems.Where(p => p.ProductID == product_id).FirstOrDefault();
                            if (item != null)
                                cart.ShopCart[i].ShopCartItems.Remove(item);
                        }
                        if (cart.ShopCart[i].ShopCartItems.Count == 0)
                        {
                            // Remove empty shop cart
                            var shop = cart.ShopCart.Where(s => s.ShopID == shop_id).FirstOrDefault();
                            cart.ShopCart.Remove(shop);
                        }
                    }
                }
                UpdateCustomCookie(Cookies.CartCookie, false, new JavaScriptSerializer().Serialize(cart), 120);
                // Save cart to database
                CartHandler.SaveCart(new JavaScriptSerializer().Serialize(cart), SiteUserDetails.LoggedInUser.Id);
                return Json(new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { RenderRazorViewToString("_Cart", cart) } }, JsonRequestBehavior.AllowGet);
            }
            return Json(new ActionOutput { Status = ActionStatus.Error }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteShopFromCart(int shop_id)
        {
            HttpCookie cookie = Request.Cookies[Cookies.CartCookie];
            if (cookie != null)
            {
                Cart cart = new JavaScriptSerializer().Deserialize<Cart>(cookie.Value);
                for (short i = 0; i < cart.ShopCart.Count; i++)
                {
                    if (cart.ShopCart[i].ShopID == shop_id)
                    {
                        // Remove shop cart
                        var shop = cart.ShopCart.Where(s => s.ShopID == shop_id).FirstOrDefault();
                        cart.ShopCart.Remove(shop);
                    }
                }
                UpdateCustomCookie(Cookies.CartCookie, false, new JavaScriptSerializer().Serialize(cart), 120);
                // Save cart to database
                CartHandler.SaveCart(new JavaScriptSerializer().Serialize(cart), SiteUserDetails.LoggedInUser.Id);
                return Json(new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { RenderRazorViewToString("_Cart", cart) } }, JsonRequestBehavior.AllowGet);
            }
            return Json(new ActionOutput { Status = ActionStatus.Error }, JsonRequestBehavior.AllowGet);
        }
    }
}
