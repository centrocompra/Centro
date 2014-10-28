using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Classes;
using System.Data.Objects;

namespace BusinessLayer.Handler
{
    public static class CartHandler
    {
        //public ActionOutput AddToCart(int shop_id, int product_id, int quantity)
        //{
        //    Product product = ProductsHandler.ProductById(product_id).Object;
        //    Cart cart = new Cart();
        //    ShopCart shopCart = new ShopCart();
        //    ShopCartItems shopCartItems = new ShopCartItems();

        //    /* Adding item to shopCartItems */
        //    shopCartItems.ShopID = shop_id;
        //    shopCartItems.ShipToCountryID = 0;
        //    shopCartItems.ShipFromCountryID = 0;
        //    shopCartItems.ProductID = product_id;
        //    shopCartItems.Quantity = quantity;
        //    shopCartItems.UnitPrice = product.UnitPrice.Value;
        //    shopCartItems.ShippingPrice = 0;
        //    List<ShopCartItems> items = new List<ShopCartItems>();
        //    items.Add(shopCartItems);

        //    shopCart.ItemTotalPrice = product.UnitPrice.Value * quantity;
        //    shopCart.ItemTotalShippingPrice = 0;
        //    shopCart.NoteForShop = "";
        //    shopCart.ShopID = shop_id;
        //    shopCart.ShopCartItems = items;
        //    shopCartItems.ShipFromCountryID = 0;
        //    shopCartItems.ShipToCountryID = 0;
        //    shopCartItems.
        //}
        public static ActionOutput SaveCart(string cart, int user_id)
        {
            using (var context = new CentroEntities())
            {
                var old_cart = context.UserCarts.Where(m => m.UserID == user_id).FirstOrDefault();
                if (old_cart != null)
                {
                    old_cart.CartData = cart;
                }
                else
                {
                    context.UserCarts.AddObject(new UserCart { CartData = cart, UserID = user_id, CreatedOn = DateTime.Now });
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput<UserCart> GetCart(int user_id)
        {
            using(var context = new CentroEntities())
            {
                return new ActionOutput<UserCart> { Object = context.UserCarts.Where(m => m.UserID == user_id).FirstOrDefault(), Status = ActionStatus.Successfull };
            }
        }



        public static SalesTax SalesTax(int StateID, int ShopID)
        {
            using (var context = new CentroEntities())
            {
                return context.SalesTaxes.Where(m => m.ShopID == ShopID && m.ToStateID == StateID && m.ToCountryID == 1).FirstOrDefault();
            }
        }
    }
}