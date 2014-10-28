using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Classes;
using System.Data.Objects;
using System.Web.Script.Serialization;
using System.Web.Mvc;

namespace BusinessLayer.Handler
{
    public class SellersHandler
    {
        public static int ShopCount(User user)
        {
            using (var context = new CentroEntities())
            {
                return context.Users.Where(m => m.CreatedOn.Month == user.CreatedOn.Month &&
                                                     m.CreatedOn.Year == user.CreatedOn.Year && user.CreatedOn > m.CreatedOn)
                                                 .Count();
            }
        }

        /// <summary>
        /// Create Shop
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ActionOutput CreateOrUpdateShop(Shop obj, int logged_in_user_id)
        {
            using (var context = new CentroEntities())
            {
                if (obj.ShopID <= 0)
                {
                    User user = context.Users.Where(m => m.UserID == logged_in_user_id).FirstOrDefault();

                    obj.CreatedBy = obj.UserId;
                    obj.CreatedOn = DateTime.Now;
                    obj.IsClosed = true;
                    obj.DeletedBy = null;
                    obj.DeletedOn = null;
                    obj.IsDeleted = false;
                    obj.IsClosed = true;
                    obj.UpdatedBy = null;
                    obj.UpdatedOn = null;
                    obj.ShopNumberID = obj.ShopNumberID;
                    context.Shops.AddObject(obj);
                    if (!String.IsNullOrEmpty(obj.ServiceId))
                    {
                        var list = obj.ServiceId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> serviceId = new List<int>();
                        foreach (var item in list)
                        {
                            serviceId.Add(Convert.ToInt32(item));
                        }
                        foreach (var item in serviceId)
                        {
                            ShopService s = new ShopService();
                            s.ShopID = obj.ShopID;
                            s.ServiceID = item;
                            s.ServiceType = (int)ServiceType.ProfessionalServices;
                            context.ShopServices.AddObject(s);
                        }
                    }

                    if (!String.IsNullOrEmpty(obj.Materials))
                    {
                        // Saving tags & materials
                        var masterTagsAll = context.Tags.ToList();
                        var masterShopMaterials = masterTagsAll.Where(m => m.TagType == (int)TagType.ProductMaterial).ToList();
                        var existingShopMaterials = context.ShopMaterials.Where(m => m.ShopId == obj.ShopID).ToList();
                        List<int> shopMaterials = new List<int>();
                        // Save product materials 
                        string[] materials = obj.Materials.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var material in materials)
                        {
                            var existingMaterial = masterShopMaterials.Where(t => t.TagText.Equals(material, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                            if (existingMaterial != null && !existingShopMaterials.Where(t => t.TagId == existingMaterial.TagID).Any())
                            {
                                shopMaterials.Add(existingMaterial.TagID);
                            }
                            else if (!existingShopMaterials.Where(t => t.TagId == existingMaterial.TagID).Any())
                            {
                                // Make entry in master table
                                Tag t = new Tag();
                                t.TagText = material;
                                t.TagType = (int)TagType.ProductMaterial;
                                context.Tags.AddObject(t);
                                context.SaveChanges();

                                shopMaterials.Add(t.TagID);
                            }
                        }
                        if (shopMaterials.Count > 0)
                        {
                            foreach (int id in shopMaterials)
                            {
                                context.ShopMaterials.AddObject(new ShopMaterial { TagId = id, ShopId = obj.ShopID });
                            }

                        }
                    }

                }
                else
                {
                    User user = context.Users.Where(m => m.UserID == logged_in_user_id).FirstOrDefault();

                    int count = context.Users.Where(m => m.CreatedOn.Month == user.CreatedOn.Month &&
                                                 m.CreatedOn.Year == user.CreatedOn.Year && user.CreatedOn > m.CreatedOn)
                                             .Count();

                    Shop shop = context.Shops.Where(s => s.ShopID == obj.ShopID).FirstOrDefault();
                    shop.ShopName = obj.ShopName;
                    shop.ShopTitle = obj.ShopTitle;
                    shop.ShopAnnouncement = obj.ShopAnnouncement;
                    shop.MessageForBuyers = obj.MessageForBuyers;
                    shop.ShopBanner = obj.ShopBanner;
                    shop.IsClosed = obj.IsClosed;
                    shop.UpdatedOn = DateTime.Now;
                    shop.UpdatedBy = logged_in_user_id;
                    shop.WelcomeMessage = obj.WelcomeMessage;
                    shop.PaymentPolicy = obj.PaymentPolicy;
                    shop.RefundPolicy = obj.RefundPolicy;
                    shop.SellerInformation = obj.SellerInformation;
                    shop.AdditionalInformation = obj.AdditionalInformation;
                    shop.DeliveryInformation = obj.DeliveryInformation;
                    shop.PrinterType = obj.PrinterType;
                    shop.Materials = obj.Materials;
                    shop.Dimensions = obj.Dimensions;
                    shop.ShopSpecialties = obj.ShopSpecialties;
                    shop.ShipToUSOnly = obj.ShipToUSOnly;
                    shop.AcceptJob = obj.AcceptJob;
                    obj.ServiceId = obj.ServiceId != null ? obj.ServiceId : "";
                    shop.ContactFirstName = obj.ContactFirstName;
                    shop.ContactLastName = obj.ContactLastName;
                    shop.ContactEmail = obj.ContactEmail;
                    shop.ContactAddress = obj.ContactAddress;
                    shop.ContactCity = obj.ContactCity;
                    shop.ContactState = obj.ContactState;
                    shop.ContactCountry = obj.ContactCountry;
                    shop.ShopNumberID = (shop.CreatedOn.Month < 10 ? ("0" + shop.CreatedOn.Month) : shop.CreatedOn.Month.ToString()) + (shop.CreatedOn.Year - 2000).ToString() + (count + 1).ToString();

                    if (obj.ServiceId != null)
                    {
                        var list = obj.ServiceId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> serviceId = new List<int>();
                        foreach (var item in list)
                        {
                            serviceId.Add(Convert.ToInt32(item));
                        }
                        var shop_service_list = context.ShopServices.Where(m => m.ShopID == obj.ShopID && m.ServiceType == (int)ServiceType.ProfessionalServices).ToList();
                        var service_to_be_removed = shop_service_list.Where(o => !serviceId.Contains(o.ServiceID) && o.ShopID == obj.ShopID && o.ServiceType == (int)ServiceType.ProfessionalServices).ToList();
                        foreach (var item in service_to_be_removed)
                        {
                            context.ShopServices.DeleteObject(item);

                        }
                        foreach (var item in serviceId)
                        {
                            ShopService s = context.ShopServices.Where(m => m.ShopID == obj.ShopID && m.ServiceID == item && m.ServiceType == (int)ServiceType.ProfessionalServices).FirstOrDefault();
                            if (s == null)
                            {
                                s = new ShopService();
                                s.ShopID = obj.ShopID;
                                s.ServiceID = item;
                                s.ServiceType = (int)ServiceType.ProfessionalServices;
                                context.ShopServices.AddObject(s);
                            }
                        }
                    }

                    if (String.IsNullOrEmpty(obj.Materials))
                    {
                        var existingShopMaterials = context.ShopMaterials.Where(m => m.ShopId == obj.ShopID).ToList();
                        foreach (var existingMaterial in existingShopMaterials)
                        {
                            context.ShopMaterials.DeleteObject(existingMaterial);

                        }

                    }

                    if (!String.IsNullOrEmpty(obj.Materials))
                    {
                        List<int> shopMaterials = new List<int>();
                        // Save product materials 
                        List<string> materials = obj.Materials.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        // Saving tags & materials
                        var masterTagsAll = context.Tags.ToList();
                        var masterShopMaterials = masterTagsAll.Where(m => m.TagType == (int)TagType.ProductMaterial).ToList();
                        var existingShopMaterials = context.ShopMaterials.Where(m => m.ShopId == obj.ShopID).ToList();

                        var materials_ro_removed = existingShopMaterials.Where(t => !materials.Contains(t.Tag.TagText, true)).ToList();

                        foreach (var existingMaterial in materials_ro_removed)
                        {
                            context.ShopMaterials.DeleteObject(existingMaterial);

                        }

                        foreach (var material in materials)
                        {
                            var existingMaterial = masterShopMaterials.Where(t => t.TagText.Equals(material, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                            if (existingMaterial != null && !existingShopMaterials.Where(t => t.TagId == existingMaterial.TagID).Any())
                            {
                                shopMaterials.Add(existingMaterial.TagID);
                            }
                            else if (existingMaterial == null || !existingShopMaterials.Where(t => t.TagId == existingMaterial.TagID).Any())
                            {
                                // Make entry in master table
                                Tag t = new Tag();
                                t.TagText = material;
                                t.TagType = (int)TagType.ProductMaterial;
                                context.Tags.AddObject(t);
                                context.SaveChanges();

                                shopMaterials.Add(t.TagID);
                            }
                        }
                        if (shopMaterials.Count > 0)
                        {
                            foreach (int id in shopMaterials)
                            {
                                context.ShopMaterials.AddObject(new ShopMaterial { TagId = id, ShopId = obj.ShopID });
                            }

                        }
                    }

                }
                if (context.SaveChanges() > 0)
                    return new ActionOutput { Status = ActionStatus.Successfull, ID = obj.ShopID, Message = "Request has been processed successfully." };
                return new ActionOutput { Status = ActionStatus.Error, ID = 0, Message = "Error Occured." };
            }
        }

        /// <summary>
        /// Get a Shop by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ActionOutput<Shop> ShopByShopId(int shop_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Shop> { Object = context.Shops.Where(s => s.ShopID == shop_id).FirstOrDefault(), Status = ActionStatus.Successfull };
            }
        }

        /// <summary>
        /// Get a Shop by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ActionOutput<Shop> ShopByShopName(string shopname)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Shop> { Object = context.Shops.Where(s => s.ShopName == shopname).FirstOrDefault(), Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput<User> ShopOwnerByShopName(string shopname)
        {
            using (var context = new CentroEntities())
            {
                var user = context.Users.Join(context.Shops,
                                            u => u.UserID,
                                            s => s.UserId,
                                            (u, s) => new { u, s }).Where(m => m.s.ShopName == shopname).Select(m => m.u).FirstOrDefault();

                var pic = context.Users.Include("Picture").Where(m => m.UserID == user.UserID).FirstOrDefault();
                if (pic.Picture != null) { user.ProfilePicUrl = pic.Picture.SavedName; }

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
                return new ActionOutput<User>
                {
                    Object = user,
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput<User> UserByUsername(string username)
        {
            using (var context = new CentroEntities())
            {
                var user = context.Users.Where(m => m.UserName == username).FirstOrDefault();

                var pic = context.Users.Include("Picture").Where(m => m.UserID == user.UserID).FirstOrDefault();
                if (pic.Picture != null) { user.ProfilePicUrl = pic.Picture.SavedName; }

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
                return new ActionOutput<User>
                {
                    Object = user,
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput<User> ShopOwnerByProductId(int product_id)
        {
            using (var context = new CentroEntities())
            {
                var user = context.Users.Join(context.Shops,
                                            u => u.UserID,
                                            s => s.UserId,
                                            (u, s) => new { u, s })
                                            .Join(context.Products,
                                            u => u.s.ShopID,
                                            p => p.ShopId,
                                            (u, p) => new { u, p })
                                            .Where(m => m.p.ProductID == product_id).Select(m => m.u.u).FirstOrDefault();

                //var pic = context.Pictures.Join(context.Users,
                //                                        p => p.PictureID,
                //                                        u => u.ProfilePicId,
                //                                        (p, u) => new { p }).Where(m => m.p.PictureID == user.ProfilePicId).FirstOrDefault();

                //var pic = context.Pictures.Where(o => o.User.UserID.Equals(user.UserID)).FirstOrDefault();
                var pic = context.Users.Include("Picture").Where(m => m.UserID == user.UserID).FirstOrDefault();
                if (pic.Picture != null) { user.ProfilePicUrl = pic.Picture.SavedName; }

                //user.ProfilePicUrl = pic != null ? pic.SavedName : null;
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
                return new ActionOutput<User>
                {
                    Object = user,
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput<User> ShopOwnerByShopId(int shop_id)
        {
            using (var context = new CentroEntities())
            {
                var user = context.Users.Join(context.Shops,
                                            u => u.UserID,
                                            s => s.UserId,
                                            (u, s) => new { u, s }).Where(m => m.s.ShopID == shop_id).Select(m => m.u).FirstOrDefault();

                //var pic = context.Pictures.Join(context.Users,
                //                                        p => p.PictureID,
                //                                        u => u.ProfilePicId,
                //                                        (p, u) => new { p }).Where(m => m.p.PictureID == user.ProfilePicId).FirstOrDefault();

                //var pic = context.Pictures.Where(o => o.User.UserID.Equals(user.UserID)).FirstOrDefault();
                var pic = context.Users.Include("Picture").Where(m => m.UserID == user.UserID).FirstOrDefault();
                if (pic.Picture != null) { user.ProfilePicUrl = pic.Picture.SavedName; }

                //user.ProfilePicUrl = pic != null ? pic.SavedName : null;
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
                return new ActionOutput<User>
                {
                    Object = user,
                    Status = ActionStatus.Successfull
                };
            }
        }

        /// <summary>
        /// Get a Shop by user_id
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public static ActionOutput<Shop> ShopByUserId(int user_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Shop> { Object = context.Shops.Where(s => s.UserId == user_id).FirstOrDefault(), Status = ActionStatus.Successfull };
            }
        }

        /// <summary>
        /// Create or save shop section
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ActionOutput CreateOrUpdateSection(ShopSection obj)
        {
            using (var context = new CentroEntities())
            {
                if (obj.ShopSectionID <= 0)
                {
                    // Get the Max Display Order
                    int? order = context.ShopSections.Select(m => (int?)m.DisplayOrder).Max();
                    obj.IsDeleted = false;
                    obj.DisplayOrder = order.HasValue ? order.Value + 1 : 1;
                    context.ShopSections.AddObject(obj);
                }
                else
                {
                    var section = context.ShopSections.Where(s => s.ShopSectionID == obj.ShopSectionID).FirstOrDefault();
                    section.DisplayOrder = obj.DisplayOrder;
                    section.SectionName = obj.SectionName;
                }
                if (context.SaveChanges() > 0)
                    return new ActionOutput { Status = ActionStatus.Successfull, ID = obj.ShopSectionID, Message = "Request has been processed successfully." };
                return new ActionOutput { Status = ActionStatus.Error, ID = 0, Message = "Error Occured." };
            }
        }

        public static ActionOutput ShipToUsaOnly(Shop obj)
        {
            using (var context = new CentroEntities())
            {
                var shop = context.Shops.Where(m => m.ShopID == obj.ShopID).FirstOrDefault();
                shop.ShipToUSOnly = obj.ShipToUSOnly;
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull };
            }
        }

        /// <summary>
        /// Get all sections of a shop
        /// </summary>
        /// <param name="shop_id"></param>
        /// <returns></returns>
        public static ActionOutput<ShopSection> ShopSectionsByShopId(int shop_id)
        {
            using (var context = new CentroEntities())
            {
                var section = context.ShopSections.Where(s => s.ShopId == shop_id && !s.IsDeleted).ToList();
                return new ActionOutput<ShopSection> { Status = ActionStatus.Successfull, List = section, Message = "Request has been processed successfully." };
            }
        }

        /// <summary>
        /// Set Display order
        /// </summary>
        /// <param name="shop_id"></param>
        /// <returns></returns>
        public static ActionOutput SetShopSectionDisplayOrder(int shop_id, Dictionary<int, int> positions)
        {
            using (var context = new CentroEntities())
            {
                var sections = context.ShopSections.Where(m => m.ShopId == shop_id && !m.IsDeleted).ToList();
                foreach (var section in sections)
                {
                    var item = positions.Where(m => m.Key == section.ShopSectionID).FirstOrDefault();
                    section.DisplayOrder = item.Value;
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Request has been processed successfully." };
            }
        }

        /// <summary>
        /// soft delete the section
        /// </summary>
        /// <param name="section_id"></param>
        /// <returns></returns>
        public static ActionOutput DeleteShopSection(int section_id)
        {
            using (var context = new CentroEntities())
            {
                var section = context.ShopSections.Where(s => s.ShopSectionID == section_id).FirstOrDefault();
                //section.IsDeleted = true;
                context.DeleteObject(section);
                try
                {
                    context.SaveChanges();
                    return new ActionOutput { Status = ActionStatus.Successfull, Message = "Request has been processed successfully." };
                }
                catch (Exception ex)
                {
                    return new ActionOutput { Status = ActionStatus.Error, Message = "Section '" + section.SectionName + "' is associated with some items, please remove the dependency and try deleting again!" };
                }
            }
        }

        /// <summary>
        /// set the section name
        /// </summary>
        /// <param name="section_id"></param>
        /// <param name="section_name"></param>
        /// <returns></returns>
        public static ActionOutput UpdateSection(int section_id, string section_name)
        {
            using (var context = new CentroEntities())
            {
                var section = context.ShopSections.Where(s => s.ShopSectionID == section_id).FirstOrDefault();
                section.SectionName = section_name;
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Request has been processed successfully." };
            }
        }

        /// <summary>
        /// Get the complated step of shop signup
        /// </summary>
        /// <param name="shop_id"></param>
        /// <returns></returns>
        public static ActionOutput<ShopSignUpStepCompleted> GetShopSignUpStepCompleted(int shop_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<ShopSignUpStepCompleted>
                {
                    Object = context.ShopSignUpStepCompleteds.Where(s => s.ShopId == shop_id).FirstOrDefault(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        /// <summary>
        /// Set the step completed for shop signup
        /// </summary>
        /// <param name="shop_id"></param>
        /// <returns></returns>
        public static ActionOutput SetShopDetailsStepCompleted(int shop_id)
        {
            using (var context = new CentroEntities())
            {
                #region Shop Details
                bool shopExists = false;
                var shop = context.Shops.Join(context.ShopSections,
                                                s => s.ShopID,
                                                sc => sc.ShopId,
                                                (s, sc) => new
                                                {
                                                    a = s.AdditionalInformation,
                                                    b = "aaaaaaaaaaaa",//s.DeliveryInformation,
                                                    c = "aaaaaaaaaaaa",//s.MessageForBuyers,
                                                    d = s.PaymentPolicy,
                                                    e = s.RefundPolicy,
                                                    f = "aaaaaaaaaaaa",//s.SellerInformation,
                                                    g = "aaaaaaaaaaaa",//s.ShopAnnouncement,
                                                    // h = s.ShopBanner,
                                                    i = s.ShopName,
                                                    j = "aaaaaaaaaaaa",//s.ShopTitle,
                                                    k = "aaaaaaaaaaaa",//s.WelcomeMessage,
                                                    sc.IsDeleted,
                                                    sc.ShopId
                                                })
                                        .Where(s => s.IsDeleted == false && s.ShopId == shop_id)
                                        .FirstOrDefault();
                ShopSignUpStepCompleted step;
                var data = context.ShopSignUpStepCompleteds.Where(s => s.ShopId == shop_id).FirstOrDefault();
                if (shop != null && !string.IsNullOrWhiteSpace(shop.a) && !string.IsNullOrWhiteSpace(shop.b) && !string.IsNullOrWhiteSpace(shop.c) && !string.IsNullOrWhiteSpace(shop.d) &&
                        !string.IsNullOrWhiteSpace(shop.e) && !string.IsNullOrWhiteSpace(shop.f) && !string.IsNullOrWhiteSpace(shop.g) /*&& !string.IsNullOrWhiteSpace(shop.h)*/ &&
                        !string.IsNullOrWhiteSpace(shop.i) && !string.IsNullOrWhiteSpace(shop.j) && !string.IsNullOrWhiteSpace(shop.k))
                {
                    shopExists = true;
                    step = new ShopSignUpStepCompleted();
                    step.ShopId = shop_id;
                    step.StepCompleted = (int)ShopSignupStepCompleted.ShopDetails;
                    if (data == null)
                    {
                        context.ShopSignUpStepCompleteds.AddObject(step);
                        context.SaveChanges();
                    }
                }
                else
                {
                    if (data != null)
                    {
                        context.DeleteObject(data);
                        context.SaveChanges();
                    }
                    shopExists = false;
                }
                #endregion
                #region Get Paid
                //int salestax = context.SalesTaxes.Where(m => m.ShopID == shop_id && m.ToCountryID >= 1).Count();
                bool paypal = context.Users.Join(context.Shops, u => u.UserID, s => s.UserId, (u, s) => new { u, s })
                                          .Where(m => m.s.ShopID == shop_id && !string.IsNullOrEmpty(m.u.PaypalID))
                                          .Any();
                if (/*salestax > 0 && items > 0 && */paypal && shopExists)
                {
                    if (data != null)
                    {
                        data.StepCompleted = (int)ShopSignupStepCompleted.GetPaid;
                        context.SaveChanges();
                    }
                }
                //else if (items > 0)
                //{
                //    if (data != null)
                //    {
                //        data.StepCompleted = (int)ShopSignupStepCompleted.ListItems;
                //        context.SaveChanges();
                //    }
                //}
                else
                {
                    if (data != null)
                    {
                        data.StepCompleted = (int)ShopSignupStepCompleted.ShopDetails;
                        context.SaveChanges();
                    }
                }
                #endregion
                #region List Items
                int items = context.Products.Where(p => p.ShopId == shop_id).Count();
                if (items >= 0 && shopExists && paypal)
                {
                    if (data != null)
                    {
                        data.StepCompleted = (int)ShopSignupStepCompleted.ListItems;
                        context.SaveChanges();
                    }
                }
                #endregion
                return new ActionOutput { Status = ActionStatus.Successfull };
            }
        }

        /// <summary>
        /// Get the list of countries
        /// </summary>
        /// <returns></returns>
        public static ActionOutput<Country> CountryGetAll()
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Country> { List = context.Countries.ToList(), Status = ActionStatus.Successfull };
            }
        }

        /// <summary>
        /// Get the list of states by country Id
        /// </summary>
        /// <returns></returns>
        public static ActionOutput<StateProvince> GetStateByCountryId(int countryId)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<StateProvince> { List = context.StateProvinces.Where(o => o.CountryID == countryId).ToList(), Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput<StateProvince> GetSalesTaxStates(int countryId, int shopid)
        {
            using (var context = new CentroEntities())
            {
                var list = context.SalesTaxes.Join(context.StateProvinces,
                                                    st => st.ToStateID,
                                                    sp => sp.StateID,
                                                    (st, sp) => new { st, sp })
                                           .Where(m => m.st.ShopID == shopid && m.st.ToCountryID == countryId)
                                           .Select(m => m.sp)
                                           .ToList();

                return new ActionOutput<StateProvince> { List = list, Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput<StateProvince> GetStateById(int state_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<StateProvince> { Object = context.StateProvinces.Where(o => o.StateID == state_id).FirstOrDefault(), Status = ActionStatus.Successfull };
            }
        }

        /// <summary>
        /// Get the list of city by Stateid
        /// </summary>
        /// <returns></returns>
        public static ActionOutput<City> GetCityByStateId(int stateId)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<City> { List = context.Cities.Where(o => o.StateId == stateId).ToList(), Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput<City> GetCityById(int city_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<City> { Object = context.Cities.Where(o => o.CityID == city_id).FirstOrDefault(), Status = ActionStatus.Successfull };
            }
        }

        /// <summary>
        /// Get the list of states by country
        /// </summary>
        /// <returns></returns>
        public static ActionOutput<StateProvince> StatesByCountry(int country_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<StateProvince> { List = context.StateProvinces.Where(s => s.CountryID == country_id).ToList(), Status = ActionStatus.Successfull };
            }
        }

        /// <summary>
        /// Get the list of country by country shortcode
        /// </summary>
        /// <returns></returns>
        public static ActionOutput<Country> GetCountryByShortCode(string code)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Country> { Object = context.Countries.Where(s => s.CountryShortCode == code).FirstOrDefault(), Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput<Country> GetCountryByID(int country_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Country> { Object = context.Countries.Where(s => s.CountryID == country_id).FirstOrDefault(), Status = ActionStatus.Successfull };
            }
        }

        /// <summary>
        /// Get the list of categories
        /// </summary>
        /// <returns></returns>
        public static ActionOutput<Category> CategoriesGetAll()
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Category> { List = context.Categories.Where(c => !c.IsDeleted && c.Published).ToList(), Status = ActionStatus.Successfull };
            }
        }

        /// <summary>
        /// Get the list of SubCategory
        /// </summary>
        /// <returns></returns>
        public static ActionOutput<SubCategory> SubCategoriesGet(int? CategoryID=null)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<SubCategory>
                {
                    List = CategoryID.HasValue ? context.SubCategories.Where(m => m.CategoryID == CategoryID.Value).ToList() : context.SubCategories.ToList(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        /// <summary>
        /// Get the list of Types
        /// </summary>
        /// <returns></returns>
        public static ActionOutput<BusinessLayer.Models.DataModel.Type> TypesGet(int? SubCategoryID=null)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<BusinessLayer.Models.DataModel.Type>
                {
                    List = SubCategoryID.HasValue ? context.Types.Where(m => m.SubCategoryID == SubCategoryID.Value).ToList() : context.Types.ToList(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput DeleteBanner(int shop_id)
        {
            using (var context = new CentroEntities())
            {
                string banner = "";
                var q = context.Shops.Where(m => m.ShopID == shop_id).FirstOrDefault();
                banner = q.ShopBanner;
                q.ShopBanner = "";
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = banner };
            }
        }

        public static ActionOutput SalesTax(List<SalesTax> list, bool isUS)
        {
            using (var context = new CentroEntities())
            {
                int shop_id = list.FirstOrDefault().ShopID;
                if (isUS)
                {
                    var oldlist = context.SalesTaxes.Where(m => m.ToCountryID == 1 && m.ShopID == shop_id).ToList();
                    foreach (var item in oldlist)
                    {
                        context.DeleteObject(item);
                    }
                    context.SaveChanges();
                }
                else
                {
                    //m.ToCountryID != 1 && 
                    var oldlist = context.SalesTaxes.Where(m => m.ShopID == shop_id).ToList();
                    foreach (var item in oldlist)
                    {
                        context.DeleteObject(item);
                    }
                    context.SaveChanges();
                }
                foreach (SalesTax item in list)
                {
                    context.SalesTaxes.AddObject(item);
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Sales tax has been saved." };
            }
        }

        public static ActionOutput DeleteSalesTax(int sales_tax_id)
        {
            using (var context = new CentroEntities())
            {
                var sales = context.SalesTaxes.Where(m => m.SalesTaxID == sales_tax_id).FirstOrDefault();
                context.DeleteObject(sales);
                context.SaveChanges();
                return new ActionOutput { Message = "Tax entry has been deleted.", Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput OpenShop(int shop_id, int user_id)
        {
            using (var context = new CentroEntities())
            {
                var shop = context.Shops.Join(context.Users,
                                                s => s.UserId,
                                                u => u.UserID,
                                                (s, u) => new { s, u })
                                        .Where(m => m.s.ShopID == shop_id && !string.IsNullOrEmpty(m.u.PaypalID)).FirstOrDefault();
                if (shop == null)
                    return new ActionOutput { Status = ActionStatus.Error, Message = "Paypal id is missing." };
                shop.s.IsClosed = false;
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Shop is now open." };
            }
        }

        public static Boolean IsShopClosed(int shop_id)
        {
            using (var context = new CentroEntities())
            {
                var shop = context.Shops.Where(m => m.ShopID == shop_id).FirstOrDefault();
                if (shop == null)
                    return true;
                return shop.IsClosed;
            }
        }

        public static ActionOutput<SalesTax> TaxByShopAndCountry(int shop_id, int country_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<SalesTax>
                {
                    Object = context.SalesTaxes.Where(s => s.ShopID == shop_id && s.ToCountryID == country_id).FirstOrDefault(),
                    Status = ActionStatus.Successfull
                };

            }
        }

        public static ActionOutput<BillingAddress> PrimaryBillingAddress(int user_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<BillingAddress>
                {
                    Object = context.BillingAddresses.Where(m => m.UserID == user_id && m.IsPrimary).FirstOrDefault(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput<ShippingAddress> PrimaryShippingAddress(int user_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<ShippingAddress>
                {
                    Object = context.ShippingAddresses.Where(m => m.UserID == user_id && m.IsPrimary).FirstOrDefault(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        /// <summary>
        /// Get a Shop Services by Shop_id
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public static ActionOutput<ShopServices> ShopServicesByShopId(int? shop_id)
        {
            using (var context = new CentroEntities())
            {
                List<ShopServices> service_list1 = Enum.GetValues(typeof(Services)).Cast<Services>().Select(v => new ShopServices
                {
                    ServiceName = v.ToEnumDescription(),
                    ServiceId = v
                }).ToList();
                if (shop_id.HasValue)
                {
                    var list = context.ShopServices.Where(s => s.ShopID == shop_id.Value && s.ServiceType == (int)ServiceType.ProfessionalServices).Select(z => z.ServiceID).ToList();

                    List<ShopServices> shopServiceList = new List<ShopServices>();
                    foreach (var service in service_list1)
                    {
                        if (list.Contains((int)service.ServiceId))
                            service.IsChecked = true;
                        else
                            service.IsChecked = false;
                        shopServiceList.Add(service);
                    }
                    return new ActionOutput<ShopServices>
                    {
                        List = shopServiceList,
                        Status = ActionStatus.Successfull
                    };
                }
                else
                {
                    return new ActionOutput<ShopServices>
                    {
                        List = service_list1,
                        Status = ActionStatus.Successfull
                    };
                }
            }
        }

        /// <summary>
        /// Return a paged result for shops by service id
        /// </summary>
        public static PagingResult<GetShopListingByServiceId_Result> GetShopListingByServiceID(int service_id, int page_no, int per_page_result, string sortColumn, string sortOrder)
        {
            using (var context = new CentroEntities())
            {
                ObjectParameter output = new ObjectParameter("TotalCount", typeof(int));
                var list = context.GetShopListingByServiceId(service_id, page_no, per_page_result, sortColumn, sortOrder, output).ToList();
                PagingResult<GetShopListingByServiceId_Result> pagingResult = new PagingResult<GetShopListingByServiceId_Result>();

                pagingResult.List = list;
                pagingResult.TotalCount = Convert.ToInt32(output.Value);
                pagingResult.Status = ActionStatus.Successfull;
                return pagingResult;
            }
        }

        /// <summary>
        /// Get a Shop Speciality by Shop_id
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public static ActionOutput<ShopSpeciality> ShopSpecialityByShopId(int? shop_id, String ShopSpeciality = "")
        {
            using (var context = new CentroEntities())
            {
                List<ShopSpeciality> speciality_list1 = Enum.GetValues(typeof(Speciality)).Cast<Speciality>().Select(v => new ShopSpeciality
                {
                    SpecialityName = v.ToEnumDescription(),
                    SpecialityId = v
                }).ToList();
                if (shop_id.HasValue)
                {
                    List<Int32> speciality_id_list = new List<Int32>();
                    if (!String.IsNullOrEmpty(ShopSpeciality))
                    {
                        List<String> temp_speciality_list = new List<String>();
                        temp_speciality_list = ShopSpeciality.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        foreach (var item in temp_speciality_list)
                        {
                            speciality_id_list.Add(Convert.ToInt32(item));
                        }
                    }
                    List<ShopSpeciality> shop_speciality_list = new List<ShopSpeciality>();
                    foreach (var speciality in speciality_list1)
                    {
                        if (speciality_id_list.Contains(Convert.ToInt32(speciality.SpecialityId)))
                            speciality.IsChecked = true;
                        else
                            speciality.IsChecked = false;
                        shop_speciality_list.Add(speciality);
                    }
                    return new ActionOutput<ShopSpeciality>
                    {
                        List = shop_speciality_list,
                        Status = ActionStatus.Successfull
                    };
                }
                else
                {
                    return new ActionOutput<ShopSpeciality>
                    {
                        List = speciality_list1,
                        Status = ActionStatus.Successfull
                    };
                }
            }
        }

        /// <summary>
        /// Create or Update Custom request
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ActionOutput SendOrUpdateCustomRequest(PrototypeRequest obj, int logged_in_user_id)
        {
            using (var context = new CentroEntities())
            {
                if (obj.RequestID <= 0)
                {
                    obj.CreatedBy = obj.BuyerId.Value;
                    obj.CreatedOn = DateTime.Now;
                    obj.IsDeleted = false;
                    obj.UpdatedBy = obj.BuyerId;
                    obj.UpdatedOn = DateTime.Now;
                    context.PrototypeRequests.AddObject(obj);
                }
                else
                {
                    var prototype_request = context.PrototypeRequests.Where(o => o.RequestID.Equals(obj.RequestID)).FirstOrDefault();
                    prototype_request.RequestStatus = obj.RequestStatus;
                    prototype_request.UpdatedBy = logged_in_user_id;
                    prototype_request.UpdatedOn = DateTime.Now;
                    prototype_request.RequestTitle = obj.RequestTitle;
                    prototype_request.Description = obj.Description;
                    prototype_request.Material = obj.Material;
                    prototype_request.Dimensions = obj.Dimensions;
                    prototype_request.MinBudget = obj.MinBudget;
                    prototype_request.MaxBudget = obj.MaxBudget;
                    prototype_request.CategoryId = obj.CategoryId;
                    prototype_request.Requirements = obj.Requirements;
                    prototype_request.IdealStartDate = obj.IdealStartDate;
                }
                if (context.SaveChanges() > 0)
                {
                    if (obj.RequestStatus == (int)CustomRequestStatus.Draft)
                    {
                        return new ActionOutput { ID = (int)obj.RequestID, Status = ActionStatus.Successfull, Message = "Your Request has been saved successfully.", Results = new List<string> { obj.RequestID.ToString() } };
                    }
                    else
                    {
                        return new ActionOutput { ID = (int)obj.RequestID, Status = ActionStatus.Successfull, Message = "Request has been submitted successfully.", Results = new List<string> { obj.RequestID.ToString() } };
                    }
                }
                return new ActionOutput { Status = ActionStatus.Error, ID = 0, Message = "Error Occured.", Results = new List<string> { "0" } };
            }
        }


        public static ActionOutput UpdateRequestAttachments(string username, List<FileAttachmentViewModel> files, long request_id, string request_user)
        {
            using (var context = new CentroEntities())
            {
                var file_ids = context.RequestAttachments.Where(p => p.RequestId == request_id).Select(p => p.AttachmentID).ToList();
                var current_file_list = files.Where(p => p.AttachmentID != 0).Select(p => p.AttachmentID).ToList();
                var files_to_be_removed = file_ids.Where(o => !current_file_list.Contains(o)).ToList();
                foreach (var item in files_to_be_removed)
                {
                    var file = context.RequestAttachments.Where(p => p.AttachmentID == item).FirstOrDefault();
                    context.RequestAttachments.DeleteObject(file);

                }
                foreach (FileAttachmentViewModel file in files.Where(p => p.AttachmentID == 0).ToList())
                {
                    RequestAttachment p = new RequestAttachment();
                    p.DisplayName = file.DisplayName;
                    p.MimeType = file.MimeType;
                    p.SavedName = file.SavedName;
                    p.SizeInBytes = file.SizeInBytes;
                    p.SizeInKb = file.SizeInKB;
                    p.SizeInMb = file.SizeInMB;
                    p.RequestId = request_id;
                    p.CreatedOn = DateTime.Now;
                    context.RequestAttachments.AddObject(p);
                }
                context.SaveChanges();


                // Move temp files to user's folder
                foreach (FileAttachmentViewModel file in files)
                {
                    try
                    {
                        Utility.MoveFile("~/Temp/RequestAttachments/" + username + "/" + file.SavedName, "~/Images/Attachments/" + request_user + "/", file.SavedName);
                    }
                    catch (Exception exc)
                    { }
                }
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Files have been saved." };
            }
        }

        /// <summary>
        /// Returne a paged result for requests by buyer or seller id
        /// </summary>
        public static PagingResult<GetRequestListingByBuyerOrSellerID_Result> GetRequestListingByBuyerOrSellerIDPaging(int? buyer_id, int? seller_id, int page_no, int per_page_result, string sortColumn, string sortOrder, bool? draft = false)
        {
            using (var context = new CentroEntities())
            {
                ObjectParameter output = new ObjectParameter("TotalCount", typeof(int));
                var list = context.GetRequestListingByBuyerOrSellerID(buyer_id, seller_id, page_no, per_page_result, sortColumn, sortOrder, draft, output).ToList();
                PagingResult<GetRequestListingByBuyerOrSellerID_Result> pagingResult = new PagingResult<GetRequestListingByBuyerOrSellerID_Result>();
                pagingResult.List = list;
                pagingResult.TotalCount = Convert.ToInt32(output.Value);
                pagingResult.Status = ActionStatus.Successfull;
                return pagingResult;
            }
        }

        public static PagingResult<ServiceContracts_Result> GetServiceContracts(int page_no, int per_page_result, string sortColumn, string sortOrder, string type, int id)
        {
            using (var context = new CentroEntities())
            {
                ObjectParameter output = new ObjectParameter("TotalCount", typeof(int));
                var list = context.ServiceContracts(page_no, per_page_result, sortColumn, sortOrder, type, id, output).ToList();
                PagingResult<ServiceContracts_Result> pagingResult = new PagingResult<ServiceContracts_Result>();
                pagingResult.List = list;
                pagingResult.TotalCount = list.Count() > 0 ? Convert.ToInt32(output.Value) : 0;
                pagingResult.Status = ActionStatus.Successfull;
                return pagingResult;
            }
        }

        public static PagingResult<ServiceContracts_Result> GetPastServiceContracts(int page_no, int per_page_result, string sortColumn, string sortOrder, string type, int id)
        {
            using (var context = new CentroEntities())
            {
                ObjectParameter output = new ObjectParameter("TotalCount", typeof(int));
                var list = context.PastServiceContracts(page_no, per_page_result, sortColumn, sortOrder, type, id, output).ToList();
                PagingResult<ServiceContracts_Result> pagingResult = new PagingResult<ServiceContracts_Result>();
                pagingResult.List = list;
                pagingResult.TotalCount = list.Count() > 0 ? Convert.ToInt32(output.Value) : 0;
                pagingResult.Status = ActionStatus.Successfull;
                return pagingResult;
            }
        }

        public static ActionOutput<StateProvince> GetStateByZipCode(int zipcode)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<StateProvince>
                {
                    Object = context.StateProvinces.Where(m => m.PostalCode == zipcode).FirstOrDefault(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        /// <summary>
        /// Get a request by request id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ActionOutput<PrototypeRequest> GetRequestByRequestId(long request_id, int logged_in_user_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<PrototypeRequest> { Object = context.PrototypeRequests.Where(s => s.RequestID == request_id && (s.BuyerId == logged_in_user_id || s.SellerId == logged_in_user_id)).FirstOrDefault(), Status = ActionStatus.Successfull };
            }
        }

        /// <summary>
        /// get Request Attachment
        /// </summary>
        /// <param name="request_id"></param>
        /// <returns></returns>
        public static ActionOutput<RequestAttachment> GetRequestAttachments(long request_id)
        {
            using (var context = new CentroEntities())
            {
                List<RequestAttachment> attachment_list = new List<RequestAttachment>();
                attachment_list = context.RequestAttachments.Where(r => r.RequestId.Equals(request_id)).ToList();
                return new ActionOutput<RequestAttachment> { Status = ActionStatus.Successfull, List = attachment_list };
            }
        }

        public static GetRequestInfoByRequestId_Result GetRequestInformation(long RequestId)
        {
            using (var context = new CentroEntities())
            {
                return context.GetRequestInfoByRequestId(RequestId).FirstOrDefault();
            }
        }

        public static ActionOutput UpdateRequestStatus(long RequestId, CustomRequestStatus status, int user_id, string username)
        {
            using (var context = new CentroEntities())
            {
                var obj = context.PrototypeRequests.Where(o => o.RequestID.Equals(RequestId) && o.RequestStatus != (int)CustomRequestStatus.Draft && (o.SellerId == user_id || o.BuyerId == user_id)).FirstOrDefault();
                obj.RequestStatus = (int)status;
                obj.UpdatedBy = user_id;
                obj.UpdatedOn = DateTime.Now;
                context.SaveChanges();

                // log alert into database
                AccountActivityHandler.SaveAlert(new Alert
                {
                    AlertForID = (int)RequestId,
                    AlertLink = (obj.SellerId == user_id ? "/Message/BuyerCustomOrder/" + RequestId : "/Message/MyCustomOrder/" + RequestId) + "/MyContracts",
                    AlertText = username + " has changed the status of custom order to " + status.ToEnumDescription(),
                    UserID = obj.SellerId.Value == user_id ? obj.BuyerId.Value : obj.SellerId.Value
                });
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Request Updated Successfully.", };
            }
        }

        public static ActionOutput DeleteRequest(long RequestId, int user_id)
        {
            using (var context = new CentroEntities())
            {
                var obj = context.PrototypeRequests.Where(o => o.RequestID.Equals(RequestId) && o.RequestStatus == (int)CustomRequestStatus.Draft && o.BuyerId == user_id).FirstOrDefault();

                context.PrototypeRequests.DeleteObject(obj);
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Request Deleted Successfully." };
            }
        }

        public static ActionOutput DeleteSalesTaxByShopId(int shop_id)
        {
            using (var context = new CentroEntities())
            {
                var list = context.SalesTaxes.Where(m => m.ShopID == shop_id).ToList();
                foreach (var item in list)
                {
                    context.DeleteObject(item);
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Sales tax has been saved." };
            }
        }

        public static ActionOutput SaveShopAvailablity(List<ShopAvailablity> slots, int shop_id, string TimeZone)
        {
            using (var context = new CentroEntities())
            {
                var old_list = context.ShopAvailablities.Where(m => m.ShopID == shop_id).ToList();
                if (old_list != null)
                {
                    foreach (var item in old_list) { context.DeleteObject(item); }
                    context.SaveChanges();
                }
                foreach (var newItem in slots)
                {
                    context.ShopAvailablities.AddObject(newItem);
                }
                var shop = context.Shops.Where(m => m.ShopID == shop_id).FirstOrDefault();
                shop.TimeZone = TimeZone;
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Availablity slots has been saved." };
            }
        }

        public static ActionOutput<ShopAvailablity> GetShopAvailablity(int shop_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<ShopAvailablity>
                {
                    Status = ActionStatus.Successfull,
                    List = context.ShopAvailablities.Where(m => m.ShopID == shop_id).ToList()
                };
            }
        }

        public static PagingResult<OrderViewModel> GetSellerOrderViewModel(int shop_id, OrderStatus order_status, int page_start, int page_size, string type)
        {
            using (var context = new CentroEntities())
            {
                var List = context.Orders.Include("Feedbacks").Join(context.OrderItems,
                                                        o => o.OrderID,
                                                        oi => oi.OrderID,
                                                        (o, oi) => new { o, oi })
                                              .Join(context.Users,
                                                        m => m.o.UserID,
                                                        u => u.UserID,
                                                        (m, u) => new { m.o, m.oi, m, u })
                                              .Join(context.Shops,
                                                        ss => ss.oi.ShopID,
                                                        s => s.ShopID,
                                                        (ss, s) => new { ss.o, ss.oi, ss.m, ss.u, s })
                                              .Join(context.Users,
                                                        all => all.s.UserId,
                                                        su => su.UserID,
                                                        (all, su) => new { all.m, all.o, all.oi, all.s, all.u, su })
                    //.Join(context.Pictures,
                    //       all => all.u.ProfilePicId,
                    //       p => p.PictureID,
                    //       (all, p) => new { all.m, all.o, all.oi, all.s, all.u, all.su, p })
                                                .Where(i => i.oi.ShopID == shop_id && i.o.OrderStatusId == (int)order_status).OrderByDescending(i => i.o.CreatedOn).Skip(page_start - 1).Take(page_size)
                                                .Select(o => new OrderViewModel
                                                {
                                                    OrderId = o.o.OrderID,
                                                    ShopId = o.oi.ShopID,
                                                    ShopOwnerId = o.s.UserId,
                                                    ShopName = o.s.ShopName,
                                                    ShopwOwnerUsername = o.su.UserName,
                                                    BuyerName = o.u.UserName,
                                                    OrderAmount = o.o.TotalAmountToBePaid,
                                                    OrderCreatedOn = o.o.CreatedOn,
                                                    ShippingStatus = (ShippingStatus)o.o.ShippingStatusId,
                                                    OrderStatus = (OrderStatus)o.o.OrderStatusId,
                                                    Type = type,
                                                    BuyerId = o.u.UserID,
                                                    //BuyerProfilePic=o.p.SavedName,
                                                    TrackingID = o.o.TrackingID,
                                                    ItemTotalPrice = o.o.ItemTotalPrice,
                                                    ItemTotalShippingPrice = o.o.ItemTotalShippingPrice,
                                                    AdminCommission = o.o.AdminCommission,
                                                    IsPercentage = o.o.IsPercentage,
                                                    Tax = o.o.Tax,
                                                    IsFeedbackGiven = o.o.Feedbacks.Count() > 0 ? true : false
                                                }).Distinct().OrderByDescending(m => m.OrderCreatedOn).ToList();
                var users = List.Select(s => s.BuyerId).ToList();
                var UserProfilePics = context.Users.Include("Picture").Where(m => users.Contains(m.UserID)).ToList();
                foreach (var item in List)
                {
                    var pic = UserProfilePics.Where(m => m.UserID == item.BuyerId).FirstOrDefault();
                    item.BuyerProfilePic = pic.ProfilePicId != null ? pic.Picture.SavedName : null;
                }

                return new PagingResult<OrderViewModel>
                {
                    List = List,
                    TotalCount = context.Orders.Join(context.OrderItems,
                            o => o.OrderID,
                            oi => oi.OrderID,
                            (o, oi) => new { o, oi }).Where(i => i.oi.ShopID == shop_id && i.o.OrderStatusId == (int)OrderStatus.Completed).Count(),
                    Status = ActionStatus.Successfull
                };
                #region Old
                //}
                //else if (order_status == OrderStatus.Canceled)
                //{
                //    return new PagingResult<OrderViewModel>
                //    {
                //        List = context.Orders.Join(context.OrderItems,
                //                                        o => o.OrderID,
                //                                        oi => oi.OrderID,
                //                                        (o, oi) => new { o, oi })
                //                               .Join(context.Users,
                //                               m => m.o.UserID,
                //                               u => u.UserID,
                //                               (m, u) => new { m.o, m.oi, m, u }
                //                               )
                //                               .Join(context.Shops,
                //                               ss => ss.oi.ShopID,
                //                               s => s.ShopID,
                //                               (ss, s) => new { ss.o, ss.oi, ss.m, ss.u, s })
                //                                .Where(i => i.oi.ShopID == shop_id && i.o.OrderStatusId == (int)OrderStatus.Canceled).OrderByDescending(i => i.o.CreatedOn).Skip(page_start - 1).Take(page_size)
                //                                .Select(o => new OrderViewModel
                //                                {
                //                                    OrderId = o.o.OrderID,
                //                                    ShopId = o.oi.ShopID,
                //                                    ShopOwnerId = o.s.UserId,
                //                                    ShopName = o.s.ShopName,
                //                                    BuyerName = o.u.UserName,
                //                                    OrderAmount = o.o.TotalAmountToBePaid,
                //                                    OrderCreatedOn = o.o.CreatedOn,
                //                                    ShippingStatus = (ShippingStatus)o.o.ShippingStatusId,
                //                                    OrderStatus = (OrderStatus)o.o.OrderStatusId,
                //                                    Type = type
                //                                }).Distinct().OrderByDescending(m => m.OrderCreatedOn).ToList(),
                //        TotalCount = context.Orders.Join(context.OrderItems,
                //                o => o.OrderID,
                //                oi => oi.OrderID,
                //                (o, oi) => new { o, oi }).Where(i => i.oi.ShopID == shop_id && i.o.OrderStatusId == (int)OrderStatus.Canceled).Count(),
                //        Status = ActionStatus.Successfull
                //    };
                //}
                //else
                //{
                //    return new PagingResult<OrderViewModel>
                //      {
                //          List = context.Orders.Join(context.OrderItems,
                //                                          o => o.OrderID,
                //                                          oi => oi.OrderID,
                //                                          (o, oi) => new { o, oi })
                //                                 .Join(context.Users,
                //                                 m => m.o.UserID,
                //                                 u => u.UserID,
                //                                 (m, u) => new { m.o, m.oi, m, u }
                //                                 )
                //                                 .Join(context.Shops,
                //                                 ss => ss.oi.ShopID,
                //                                 s => s.ShopID,
                //                                 (ss, s) => new { ss.o, ss.oi, ss.m, ss.u, s })
                //                                  .Where(i => i.oi.ShopID == shop_id && i.o.OrderStatusId == (int)OrderStatus.Pending).OrderByDescending(i => i.o.CreatedOn).Skip(page_start - 1).Take(page_size)
                //                                  .Select(o => new OrderViewModel
                //                                  {
                //                                      OrderId = o.o.OrderID,
                //                                      ShopId = o.oi.ShopID,
                //                                      ShopOwnerId = o.s.UserId,
                //                                      ShopName = o.s.ShopName,
                //                                      BuyerName = o.u.UserName,
                //                                      OrderAmount = o.o.TotalAmountToBePaid,
                //                                      OrderCreatedOn = o.o.CreatedOn,
                //                                      ShippingStatus = (ShippingStatus)o.o.ShippingStatusId,
                //                                      OrderStatus = (OrderStatus)o.o.OrderStatusId,
                //                                      Type = type
                //                                  }).Distinct().OrderByDescending(m => m.OrderCreatedOn).ToList(),
                //          TotalCount = context.Orders.Join(context.OrderItems,
                //                                          o => o.OrderID,
                //                                          oi => oi.OrderID,
                //                                          (o, oi) => new { o, oi }).Where(i => i.oi.ShopID == shop_id && i.o.OrderStatusId == (int)OrderStatus.Pending).Count(),
                //          Status = ActionStatus.Successfull
                //      };

                //}
                #endregion
            }

        }

        public static PagingResult<OrderViewModel> GetBuyerOrderViewModel(int buyer_id, OrderStatus order_status, int page_start, int page_size, string type)
        {
            using (var context = new CentroEntities())
            {
                return new PagingResult<OrderViewModel>
                {
                    List = context.Orders.Include("Feedbacks").Join(context.Users,
                                                   o => o.SellerId,
                                                   u => u.UserID,
                                                   (o, u) => new { o, u })
                                         .Join(context.Shops,
                                                   ss => ss.o.SellerId,
                                                   s => s.UserId,
                                                   (ss, s) => new { ss.o, ss.u, s })
                                         .Join(context.Pictures,
                                                    all => all.u.ProfilePicId,
                                                    p => p.PictureID,
                                                    (all, p) => new { all.o, all.s, all.u, p })
                                            .Where(i => i.o.UserID == buyer_id && i.o.OrderStatusId == (int)order_status).OrderByDescending(i => i.o.CreatedOn).Skip(page_start - 1).Take(page_size)
                                            .Select(o => new OrderViewModel
                                            {
                                                OrderId = o.o.OrderID,
                                                ShopId = o.s.ShopID,
                                                ShopOwnerId = o.s.UserId,
                                                ShopName = o.s.ShopName,
                                                ShopwOwnerUsername = o.u.UserName,
                                                BuyerName = o.u.UserName,
                                                BuyerId = buyer_id,
                                                OrderAmount = o.o.TotalAmountToBePaid,
                                                OrderCreatedOn = o.o.CreatedOn,
                                                ShippingStatus = (ShippingStatus)o.o.ShippingStatusId,
                                                OrderStatus = (OrderStatus)o.o.OrderStatusId,
                                                Type = type,
                                                SellerProfilePic = o.p.SavedName,
                                                TrackingID = o.o.TrackingID,
                                                ItemTotalPrice = o.o.ItemTotalPrice,
                                                ItemTotalShippingPrice = o.o.ItemTotalShippingPrice,
                                                AdminCommission = o.o.AdminCommission,
                                                Tax = o.o.Tax,
                                                IsPercentage = o.o.IsPercentage,
                                                IsFeedbackGiven = o.o.Feedbacks.Count() > 0 ? true : false
                                            }).Distinct().ToList(),
                    TotalCount = context.Orders.Join(context.OrderItems,
                            o => o.OrderID,
                            oi => oi.OrderID,
                            (o, oi) => new { o, oi }).Where(i => i.o.UserID == buyer_id && i.o.OrderStatusId == (int)order_status).Count(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput<Order> OrderById(int id)
        {
            using (var context = new CentroEntities())
            {
                Order order = context.Orders.Include("BillingAddress").Include("ShippingAddress").Where(m => m.OrderID == id).FirstOrDefault();
                order.BuyerName = context.Users.Where(u => u.UserID == order.UserID).FirstOrDefault().UserName;
                order.OrderList = context.OrderItems.Join(context.Products,
                                                            o => o.ProductID,
                                                            p => p.ProductID,
                                                            (o, p) => new { o, p })
                                                    .Join(context.Shops,
                                                            m => m.o.ShopID,
                                                            s => s.ShopID,
                                                            (m, s) => new { m, s })
                                                    .Join(context.Users,
                                                            m => m.s.UserId,
                                                            u => u.UserID,
                                                            (m, u) => new { m.m, m.s, u })
                                                    .Select(m => new OrderItemViewModel
                                                    {
                                                        OrderItemsID = m.m.o.OrderItemsID,
                                                        OrderID = m.m.o.OrderID,
                                                        ShopID = m.m.o.ShopID,
                                                        ShopName = m.s.ShopName,
                                                        ProductID = m.m.o.ProductID.Value,
                                                        Quantity = m.m.o.Quantity,
                                                        UnitPrice = m.m.o.UnitPrice,
                                                        UnitShippingPrice = m.m.o.UnitShippingPrice.Value,
                                                        UnitShippingAfterFirst = m.m.o.UnitShippingAfterFirst.Value,
                                                        TotalShippingPrice = m.m.o.TotalShippingPrice.Value,
                                                        IsShippingAvailable = m.m.o.IsShippingAvailable,
                                                        IsDownloabale = m.m.o.IsDownloabale,
                                                        CreatedOn = m.m.o.CreatedOn,
                                                        ProductTitle = m.m.p.Title,
                                                        PrimaryImage = m.m.p.PrimaryPicture,
                                                        ShopOwnerName = m.u.UserName
                                                    })
                                                    .Where(m => m.OrderID == id).ToList();
                return new ActionOutput<Order> { Object = order, Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput UpdateOrder(int OrderID, int OrderStatus, int ShippingStatus, string TrackingID)
        {
            using (var context = new CentroEntities())
            {
                var order = context.Orders.Where(m => m.OrderID == OrderID).FirstOrDefault();
                order.OrderStatusId = OrderStatus;
                order.ShippingStatusId = ShippingStatus;
                order.TrackingID = TrackingID;
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Status Updated." };
            }
        }

        public static ActionOutput<int> TotalUnreadCustomRequest(int user_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<int>
                {
                    Status = ActionStatus.Successfull,
                    Object = context.PrototypeRequests.Where(m => m.SellerId == user_id && !m.IsRead && m.RequestStatus != (int)CustomRequestStatus.Draft)
                                                  .Count()
                };
            }
        }

        public static ActionOutput MarkRequestAsReadUnread(int user_id, long request_id, bool read)
        {
            using (var context = new CentroEntities())
            {
                var msg = context.PrototypeRequests.Where(m => m.RequestID == request_id && (m.SellerId == user_id || m.BuyerId == user_id)).FirstOrDefault();
                msg.IsRead = read;
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput<User> RequestCreatedBy(long request_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<User>
                {
                    Status = ActionStatus.Successfull,
                    Object = context.PrototypeRequests.Join(context.Users,
                                                            p => p.BuyerId,
                                                            u => u.UserID,
                                                            (p, u) => new { p, u })
                                                    .Where(m => m.p.RequestID == request_id)
                                                    .Select(m => m.u).FirstOrDefault()
                };
            }
        }

        public static ActionOutput<Shop> GetShopByOrderID(int orderID)
        {
            using (var context = new CentroEntities())
            {
                Shop obj = (from p in context.Shops
                            join o in context.OrderItems
                                on p.ShopID equals o.ShopID
                            where o.OrderID.Equals(orderID)
                            select p).FirstOrDefault();
                return new ActionOutput<Shop> { Status = ActionStatus.Successfull, Object = obj };
            }
        }

        public static ActionOutput<Shop> GetShopByRequestID(int requestID)
        {
            using (var context = new CentroEntities())
            {
                Shop obj = (from p in context.Shops
                            join pr in context.PrototypeRequests
                                on p.ShopID equals pr.ShopId
                            where pr.RequestID.Equals(requestID)
                            select p).FirstOrDefault();
                return new ActionOutput<Shop> { Status = ActionStatus.Successfull, Object = obj };
            }
        }

        /// <summary>
        /// /// <summary>
        /// To get the list of ship to countries
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public static ActionOutput<Country> GetShipToCountriesByProductID(int productID)
        {
            using (var context = new CentroEntities())
            {
                var result = context.ShipingCountries.Join(context.Countries,
                                                       sc => sc.ShipsTo,
                                                       c => c.CountryID,
                                                       (sc, c) => new { sc, c }).
                                                       Where(m => m.sc.ProductId == productID)
                                                       .Select(o => o.c).ToList();
                return new ActionOutput<Country>
                {
                    Status = ActionStatus.Successfull,
                    List = result
                };

            }
        }

        /// <summary>
        /// To get the list of countries which ship the product
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public static ActionOutput<Country> GetShipFromCountriesByProductID(int productID)
        {
            using (var context = new CentroEntities())
            {
                var result = context.ShipingCountries.Join(context.Countries,
                                                       sc => sc.ShipsFromId,
                                                       c => c.CountryID,
                                                       (sc, c) => new { sc, c }).
                                                       Where(m => m.sc.ProductId == productID)
                                                       .Select(o => o.c).Distinct().ToList();
                return new ActionOutput<Country>
                {
                    Status = ActionStatus.Successfull,
                    List = result
                };

            }
        }

        public static ActionOutput SaveOrUpdateJob(BuyerJob obj, int logged_in_user_id, bool? MyJobs = null)
        {
            bool IsUpdated = false;
            using (var context = new CentroEntities())
            {
                if (obj.JobID <= 0)
                {
                    if (obj.IsPrivate)
                    {
                        var Seller = UsersHandler.UserByUsername(obj.Seller.Replace(";", "")).Object;
                        if (Seller == null)
                            return new ActionOutput { Status = ActionStatus.Error, Message = "Seller does not exists.", Results = new List<string> { obj.JobID.ToString() } };
                        obj.JobSentTo = Seller.UserID;

                        if (obj.JobSentTo == logged_in_user_id)
                            return new ActionOutput { Status = ActionStatus.Error, Message = "You can not create a job for yourself.", Results = new List<string> { obj.JobID.ToString() } };

                        Shop shop = SellersHandler.ShopByUserId(Seller.UserID).Object;
                        if (shop == null)
                            return new ActionOutput { Status = ActionStatus.Error, Message = obj.Seller.Replace(";", "") + " does not have any shop yet.", Results = new List<string> { obj.JobID.ToString() } };
                        else if (!shop.AcceptJob)
                            return new ActionOutput { Status = ActionStatus.Error, Message = "Seller is not accepting jobs now.", Results = new List<string> { obj.JobID.ToString() } };

                    }
                    obj.Specialties = string.Join(", ", obj.Specialties.Split(','));
                    obj.BuyerID = logged_in_user_id;
                    obj.CreatedOn = DateTime.Now;
                    obj.IsActive = true;
                    obj.IsDeleted = false;
                    context.BuyerJobs.AddObject(obj);
                }
                else
                {
                    IsUpdated = true;
                    var job = context.BuyerJobs.Where(o => o.JobID == obj.JobID).FirstOrDefault();
                    job.RequestStatus = obj.RequestStatus;
                    job.JobTitle = obj.JobTitle;
                    job.JobDescription = obj.JobDescription;
                    job.Material = obj.Material;
                    job.Dimensions = obj.Dimensions;
                    job.MinBudget = obj.MinBudget;
                    job.MaxBudget = obj.MaxBudget;
                    job.CategoryId = obj.CategoryId;
                    job.Requirements = obj.Requirements;
                    job.IdealStartDate = obj.IdealStartDate;
                    if (MyJobs.HasValue && MyJobs.Value)
                    {
                        job.IsActive = true;// obj.IsActive;
                        job.CreatedOn = DateTime.Now;
                    }
                    job.Specialties = string.Join(", ", obj.Specialties.Split(','));
                }
                if (context.SaveChanges() > 0)
                {
                    if (obj.RequestStatus == (int)CustomRequestStatus.Draft)
                    {
                        return new ActionOutput { ID = obj.JobID, Status = ActionStatus.Successfull, Message = "Your Job has been saved successfully.", Results = new List<string> { obj.JobID.ToString() } };
                    }
                    else
                    {
                        return new ActionOutput { ID = obj.JobID, Status = ActionStatus.Successfull, Message = "Job has been " + (IsUpdated ? "updated" : "created") + " successfully.", Results = new List<string> { obj.JobID.ToString() } };
                    }
                }
                return new ActionOutput { Status = ActionStatus.Error, ID = 0, Message = "Error Occured.", Results = new List<string> { "0" } };
            }
        }

        public static ActionOutput UpdateJobAttachments(string username, List<FileAttachmentViewModel> files, int job_id)
        {
            using (var context = new CentroEntities())
            {
                var file_ids = context.BuyerJobAttachments.Where(p => p.JobId == job_id).Select(p => p.AttachmentID).ToList();
                var current_file_list = files.Where(p => p.AttachmentID != 0).Select(p => p.AttachmentID).ToList();
                var files_to_be_removed = file_ids.Where(o => !current_file_list.Contains(o)).ToList();
                foreach (var item in files_to_be_removed)
                {
                    var file = context.BuyerJobAttachments.Where(p => p.AttachmentID == item).FirstOrDefault();
                    context.BuyerJobAttachments.DeleteObject(file);
                }
                foreach (FileAttachmentViewModel file in files.Where(p => p.AttachmentID == 0).ToList())
                {
                    BuyerJobAttachment p = new BuyerJobAttachment();
                    p.DisplayName = file.DisplayName;
                    p.MimeType = file.MimeType;
                    p.SavedName = file.SavedName;
                    p.SizeInBytes = file.SizeInBytes;
                    p.SizeInKb = file.SizeInKB;
                    p.SizeInMb = file.SizeInMB;
                    p.JobId = job_id;
                    p.CreatedOn = DateTime.Now;
                    context.BuyerJobAttachments.AddObject(p);
                }
                context.SaveChanges();

                // Move temp files to user's folder
                foreach (FileAttachmentViewModel file in files)
                {
                    try
                    {
                        Utility.MoveFile("~/Temp/RequestAttachments/" + username + "/" + file.SavedName, "~/Images/Attachments/" + username + "/", file.SavedName);
                    }
                    catch (Exception exc)
                    { }
                }
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Files have been saved." };
            }
        }

        public static PagingResult<JobsViewModel> GetJobs(int page_no, int per_page_result, string sortColumn, string sortOrder, int? UserID, JobsFilter filter, string MyJobs = null)
        {
            using (var context = new CentroEntities())
            {
                var query = context.BuyerJobs.Include("BuyerJobAttachments").Include("User")
                                   .OrderBy("it." + sortColumn + " " + sortOrder)
                                   .Where(m => m.IsActive == true && !m.IsDeleted);
                if (string.IsNullOrEmpty(MyJobs)) query = query.Where(m => !m.IsPrivate);

                if (UserID.HasValue && UserID.Value > 0)
                    query = query.Where(m => m.BuyerID == UserID.Value);

                if (filter != null)
                {
                    var Categories = Enum.GetValues(typeof(Services)).Cast<Services>().Select(m => new SelectListItem { Text = m.ToEnumDescription().ToLower(), Value = ((int)m).ToString() }).ToList();
                    if (filter.SearchType != "All")
                    {
                        int id = Convert.ToInt32(filter.SearchType);
                        //var FilterCategories= Categories.Where(m=>m.Text.Contains(filter.SearchType)).Select(m=> Convert.ToInt32(m.Value)).ToList();
                        //query = query.Where(m => FilterCategories.Contains(m.CategoryId));
                        query = query.Where(m => m.CategoryId == id);
                    }
                    else if (!string.IsNullOrEmpty(filter.Keyword))
                    {
                        var FilterCategories = Categories.Where(m => m.Text.Contains(filter.Keyword)).Select(m => Convert.ToInt32(m.Value)).ToList();
                        query = query.Where(m => m.JobTitle.Contains(filter.Keyword) || m.JobDescription.Contains(filter.Keyword)
                                             || m.Requirements.Contains(filter.Keyword) || m.Specialties.Contains(filter.Keyword)
                                             || FilterCategories.Contains(m.CategoryId));
                    }
                    if (filter.MinBudget > 0)// && filter.MaxBudget > filter.MinBudget)
                        query = query.Where(m => m.MinBudget >= filter.MinBudget/* && m.MaxBudget <= filter.MaxBudget*/);
                }

                var list = query.Select(m => new JobsViewModel
                                                {
                                                    JobID = m.JobID,
                                                    BuyerID = m.BuyerID,
                                                    CategoryID = m.CategoryId,
                                                    JobTitle = m.JobTitle,
                                                    JobDescription = m.JobDescription,
                                                    Requirements = m.Requirements,
                                                    Speciality = m.Specialties,
                                                    MinBudget = m.MinBudget,
                                                    MaxBudget = m.MaxBudget,
                                                    Dimensions = m.Dimensions,
                                                    Material = m.Material,
                                                    IsDeleted = m.IsDeleted,
                                                    RequestStatus = m.RequestStatus,
                                                    CreatedOn = m.CreatedOn,
                                                    AwardedTo = m.AwardedTo,
                                                    IsRead = m.IsRead,
                                                    IsActive = m.IsActive,
                                                    IsFavorite = m.IsFavorite,
                                                    Specialties = m.Specialties,
                                                    IsPrivate = m.IsPrivate,
                                                    JobSentTo = m.JobSentTo,
                                                    UserID = m.User1.UserID,
                                                    FirstName = m.User1.FirstName,
                                                    LastName = m.User1.LastName,
                                                    EmailId = m.User1.EmailId,
                                                    UserName = m.User1.UserName,
                                                    ProfilePicId = m.User1.ProfilePicId,
                                                    CountryId = m.User1.CountryId,
                                                    StateId = m.User1.StateId,
                                                    CityId = m.User1.CityId,
                                                    TotalEarning = m.User1.TotalEarning,
                                                    LastLoginOn = m.User1.LastLoginOn,
                                                    PostalCode = m.User1.PostalCode,
                                                    StreetAddress1 = m.User1.StreetAddress1,
                                                    StreetAddress2 = m.User1.StreetAddress2
                                                }).Skip(page_no - 1).Take(per_page_result).ToList();

                return new PagingResult<JobsViewModel>
                {
                    List = list,
                    TotalCount = query.Count(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static PagingResult<JobsViewModel> GetMyJobs(int page_no, int per_page_result, string sortColumn, string sortOrder, int? UserID, JobsFilter filter, string MyJobs = null)
        {
            using (var context = new CentroEntities())
            {
                var iquery = context.BuyerJobs.Include("BuyerJobAttachments").Include("User")
                                   .OrderBy("it." + sortColumn + " " + sortOrder);
                var query = filter == null ? iquery.Where(m => m.IsActive || !m.IsActive /*m.IsActive && !m.IsDeleted*/) : iquery.Where(m => m.IsDeleted == filter.IsAwarded);
                if (string.IsNullOrEmpty(MyJobs)) query = query.Where(m => !m.IsPrivate);

                if (UserID.HasValue && UserID.Value > 0)
                    query = query.Where(m => m.BuyerID == UserID.Value);

                if (filter != null)
                {
                    if (!string.IsNullOrEmpty(filter.Keyword))
                        query = query.Where(m => m.JobTitle.Contains(filter.Keyword) || m.JobDescription.Contains(filter.Keyword));
                    if (filter.MinBudget > 0 && filter.MaxBudget > filter.MinBudget)
                        query = query.Where(m => m.MinBudget >= filter.MinBudget && m.MaxBudget <= filter.MaxBudget);
                }

                var list = query.Select(m => new JobsViewModel
                                        {
                                            JobID = m.JobID,
                                            BuyerID = m.BuyerID,
                                            JobTitle = m.JobTitle,
                                            CategoryID = m.CategoryId,
                                            JobDescription = m.JobDescription,
                                            Requirements = m.Requirements,
                                            Speciality = m.Specialties,
                                            MinBudget = m.MinBudget,
                                            MaxBudget = m.MaxBudget,
                                            Dimensions = m.Dimensions,
                                            Material = m.Material,
                                            IsDeleted = m.IsDeleted,
                                            RequestStatus = m.RequestStatus,
                                            CreatedOn = m.CreatedOn,
                                            AwardedTo = m.AwardedTo,
                                            IsRead = m.IsRead,
                                            IsActive = m.IsActive,
                                            IsFavorite = m.IsFavorite,
                                            Specialties = m.Specialties,
                                            IsPrivate = m.IsPrivate,
                                            JobSentTo = m.JobSentTo,
                                            UserID = m.User1.UserID,
                                            FirstName = m.User1.FirstName,
                                            LastName = m.User1.LastName,
                                            EmailId = m.User1.EmailId,
                                            UserName = m.User1.UserName,
                                            ProfilePicId = m.User1.ProfilePicId,
                                            CountryId = m.User1.CountryId,
                                            StateId = m.User1.StateId,
                                            CityId = m.User1.CityId,
                                            TotalEarning = m.User1.TotalEarning,
                                            LastLoginOn = m.User1.LastLoginOn,
                                            PostalCode = m.User1.PostalCode,
                                            StreetAddress1 = m.User1.StreetAddress1,
                                            StreetAddress2 = m.User1.StreetAddress2,
                                            TotalApplicants = m.JobApplications.Count()
                                        }).Skip(page_no - 1).Take(per_page_result).ToList();

                return new PagingResult<JobsViewModel>
                {
                    List = list,
                    TotalCount = query.Count(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        //public static PagingResult<JobsViewModel> GetJobs(int page_no, int per_page_result, string sortColumn, string sortOrder, int UserID, 
        //                                                        string Keyword, decimal MinBudget, decimal MaxBudget)
        //{
        //    using (var context = new CentroEntities())
        //    {
        //        var query = context.BuyerJobs.Include("BuyerJobAttachments").Include("User")
        //                           .OrderBy("it." + sortColumn + " " + sortOrder)
        //                           .Where(m => m.IsActive && !m.IsDeleted/* && !m.IsPrivate*/);
        //        query = query.Where(m => m.BuyerID != UserID); // get No job from loggedin user
        //        if (!string.IsNullOrEmpty(Keyword))
        //            query = query.Where(m => m.JobTitle.Contains(Keyword) || m.JobDescription.Contains(Keyword));
        //        if (MinBudget > 0 && MaxBudget > MinBudget)
        //            query = query.Where(m => m.MinBudget >= MinBudget && m.MaxBudget <= MaxBudget);

        //        var list = query.Select(m => new JobsViewModel
        //                                        {
        //                                            JobID = m.JobID,
        //                                            BuyerID = m.BuyerID,
        //                                            JobTitle = m.JobTitle,
        //                                            JobDescription = m.JobDescription,
        //                                            MinBudget = m.MinBudget,
        //                                            MaxBudget = m.MaxBudget,
        //                                            Dimensions = m.Dimensions,
        //                                            Material = m.Material,
        //                                            IsDeleted = m.IsDeleted,
        //                                            RequestStatus = m.RequestStatus,
        //                                            CreatedOn = m.CreatedOn,
        //                                            AwardedTo = m.AwardedTo,
        //                                            IsRead = m.IsRead,
        //                                            IsActive = m.IsActive,
        //                                            IsFavorite = m.IsFavorite,
        //                                            Specialties = m.Specialties,
        //                                            IsPrivate = m.IsPrivate,
        //                                            JobSentTo = m.JobSentTo,
        //                                            UserID = m.User.UserID,
        //                                            FirstName = m.User.FirstName,
        //                                            LastName = m.User.LastName,
        //                                            EmailId = m.User.EmailId,
        //                                            UserName = m.User.UserName,
        //                                            ProfilePicId = m.User.ProfilePicId,
        //                                            CountryId = m.User.CountryId,
        //                                            StateId = m.User.StateId,
        //                                            CityId = m.User.CityId,
        //                                            TotalEarning = m.User.TotalEarning,
        //                                            LastLoginOn = m.User.LastLoginOn,
        //                                            PostalCode = m.User.PostalCode,
        //                                            StreetAddress1 = m.User.StreetAddress1,
        //                                            StreetAddress2 = m.User.StreetAddress2
        //                                        }).Skip(page_no - 1).Take(per_page_result).ToList();

        //        return new PagingResult<JobsViewModel>
        //        {
        //            List = list,
        //            TotalCount = query.Count(),
        //            Status = ActionStatus.Successfull
        //        };
        //    }
        //}

        public static PagingResult<JobsViewModel> GetJobs(int page_no, int per_page_result, string sortColumn, string sortOrder, int? BuyerID, int? JobSentTo, bool isFilter)
        {
            using (var context = new CentroEntities())
            {
                var qry = context.BuyerJobs.Join(context.JobApplications,
                                                  j => j.JobID,
                                                  a => a.JobID,
                                                  (j, a) => new { j, a })
                                          .Join(context.Users,
                                                j => j.j.BuyerID,
                                                u => u.UserID,
                                                (j, u) => new { j.a, j.j, u })
                                          .Where(m => m.a.ShowIn == (int)JobShowIn.InterviewInvited && !m.a.RequestAccepted.HasValue);
                if (JobSentTo.HasValue && JobSentTo.Value > 0)
                    qry = qry.Where(m => m.a.SellerID == JobSentTo.Value).OrderByDescending(m => m.j.CreatedOn);

                var count1 = qry.Count();

                var query = context.BuyerJobs.Include("BuyerJobAttachments").Include("User1")
                                   .OrderBy("it." + sortColumn + " " + sortOrder)
                                   .Where(m => m.IsActive && !m.IsDeleted && m.IsPrivate);
                if (BuyerID.HasValue && BuyerID.Value > 0)
                    query = query.Where(m => m.BuyerID == BuyerID.Value);
                else if (JobSentTo.HasValue && JobSentTo.Value > 0)
                    query = query.Where(m => m.JobSentTo == JobSentTo.Value && !m.IsApplied);

                var count2 = query.Count();

                var list1 = qry.Select(m => new JobsViewModel
                                                {
                                                    JobID = m.j.JobID,
                                                    BuyerID = m.j.BuyerID,
                                                    CategoryID = m.j.CategoryId,
                                                    JobTitle = m.j.JobTitle,
                                                    JobDescription = m.j.JobDescription,
                                                    Requirements = m.j.Requirements,
                                                    Speciality = m.j.Specialties,
                                                    MinBudget = m.j.MinBudget,
                                                    MaxBudget = m.j.MaxBudget,
                                                    Dimensions = m.j.Dimensions,
                                                    Material = m.j.Material,
                                                    IsDeleted = m.j.IsDeleted,
                                                    RequestStatus = m.j.RequestStatus,
                                                    CreatedOn = m.j.CreatedOn,
                                                    AwardedTo = m.j.AwardedTo,
                                                    IsRead = m.j.IsRead,
                                                    IsActive = m.j.IsActive,
                                                    IsFavorite = m.j.IsFavorite,
                                                    Specialties = m.j.Specialties,
                                                    IsPrivate = m.j.IsPrivate,
                                                    JobSentTo = m.j.JobSentTo,
                                                    UserID = m.u.UserID,
                                                    FirstName = m.u.FirstName,
                                                    LastName = m.u.LastName,
                                                    EmailId = m.u.EmailId,
                                                    UserName = m.u.UserName,
                                                    ProfilePicId = m.u.ProfilePicId,
                                                    CountryId = m.u.CountryId,
                                                    StateId = m.u.StateId,
                                                    CityId = m.u.CityId,
                                                    TotalEarning = m.u.TotalEarning,
                                                    LastLoginOn = m.u.LastLoginOn,
                                                    PostalCode = m.u.PostalCode,
                                                    StreetAddress1 = m.u.StreetAddress1,
                                                    StreetAddress2 = m.u.StreetAddress2
                                                }).Skip(page_no - 1).Take(per_page_result).ToList();

                var list2 = query.Select(m => new JobsViewModel
                                                {
                                                    JobID = m.JobID,
                                                    BuyerID = m.BuyerID,
                                                    CategoryID = m.CategoryId,
                                                    JobTitle = m.JobTitle,
                                                    JobDescription = m.JobDescription,
                                                    Requirements = m.Requirements,
                                                    Speciality = m.Specialties,
                                                    MinBudget = m.MinBudget,
                                                    MaxBudget = m.MaxBudget,
                                                    Dimensions = m.Dimensions,
                                                    Material = m.Material,
                                                    IsDeleted = m.IsDeleted,
                                                    RequestStatus = m.RequestStatus,
                                                    CreatedOn = m.CreatedOn,
                                                    AwardedTo = m.AwardedTo,
                                                    IsRead = m.IsRead,
                                                    IsActive = m.IsActive,
                                                    IsFavorite = m.IsFavorite,
                                                    Specialties = m.Specialties,
                                                    IsPrivate = m.IsPrivate,
                                                    JobSentTo = m.JobSentTo,
                                                    UserID = m.User1.UserID,
                                                    FirstName = m.User1.FirstName,
                                                    LastName = m.User1.LastName,
                                                    EmailId = m.User1.EmailId,
                                                    UserName = m.User1.UserName,
                                                    ProfilePicId = m.User1.ProfilePicId,
                                                    CountryId = m.User1.CountryId,
                                                    StateId = m.User1.StateId,
                                                    CityId = m.User1.CityId,
                                                    TotalEarning = m.User1.TotalEarning,
                                                    LastLoginOn = m.User1.LastLoginOn,
                                                    PostalCode = m.User1.PostalCode,
                                                    StreetAddress1 = m.User1.StreetAddress1,
                                                    StreetAddress2 = m.User1.StreetAddress2
                                                }).Skip(page_no - 1).Take(per_page_result).ToList();
                return new PagingResult<JobsViewModel>
                {
                    List = list1.Concat(list2).ToList(),
                    TotalCount = count1 + count2,
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static PagingResult<JobsViewModel> JobsApplied(int page_no, int per_page_result, int SellerID)
        {
            using (var context = new CentroEntities())
            {
                var query = context.BuyerJobs.Include("User1")
                                             .Join(context.JobApplications,
                                                    j => j.JobID,
                                                    a => a.JobID,
                                                    (j, a) => new { j, a })
                                             .OrderBy(m => m.j.CreatedOn)
                                             .Where(m => m.j.IsActive && !m.j.IsDeleted && m.a.SellerID == SellerID
                                                        && !m.a.RequestSent.HasValue)
                                             .Select(m => new JobsViewModel
                                             {
                                                 JobID = m.j.JobID,
                                                 BuyerID = m.j.BuyerID,
                                                 JobTitle = m.j.JobTitle,
                                                 JobDescription = m.j.JobDescription,
                                                 Requirements = m.j.Requirements,
                                                 Speciality = m.j.Specialties,
                                                 MinBudget = m.j.MinBudget,
                                                 MaxBudget = m.j.MaxBudget,
                                                 Dimensions = m.j.Dimensions,
                                                 Material = m.j.Material,
                                                 IsDeleted = m.j.IsDeleted,
                                                 RequestStatus = m.j.RequestStatus,
                                                 CreatedOn = m.j.CreatedOn,
                                                 AwardedTo = m.j.AwardedTo,
                                                 IsRead = m.j.IsRead,
                                                 IsActive = m.j.IsActive,
                                                 IsFavorite = m.j.IsFavorite,
                                                 Specialties = m.j.Specialties,
                                                 IsPrivate = m.j.IsPrivate,
                                                 JobSentTo = m.j.JobSentTo,
                                                 UserID = m.j.User1.UserID,
                                                 FirstName = m.j.User1.FirstName,
                                                 LastName = m.j.User1.LastName,
                                                 EmailId = m.j.User1.EmailId,
                                                 UserName = m.j.User1.UserName,
                                                 ProfilePicId = m.j.User1.ProfilePicId,
                                                 CountryId = m.j.User1.CountryId,
                                                 StateId = m.j.User1.StateId,
                                                 CityId = m.j.User1.CityId,
                                                 TotalEarning = m.j.User1.TotalEarning,
                                                 LastLoginOn = m.j.User1.LastLoginOn,
                                                 PostalCode = m.j.User1.PostalCode,
                                                 StreetAddress1 = m.j.User1.StreetAddress1,
                                                 StreetAddress2 = m.j.User1.StreetAddress2,
                                                 CategoryID = m.j.CategoryId
                                             });

                return new PagingResult<JobsViewModel>
                {
                    List = query.Skip(page_no - 1).Take(per_page_result).ToList(),
                    TotalCount = query.Count(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput ApplyForJob(int JobID, decimal BidAmount, int UserID, string username)
        {
            using (var context = new CentroEntities())
            {
                var job = context.BuyerJobs.Where(m => m.JobID == JobID).FirstOrDefault();
                if (job.BuyerID == UserID)
                    return new ActionOutput { Message = "You can not apply for your own job", Status = ActionStatus.Error };
                var application = context.JobApplications.Where(m => m.JobID == JobID && m.SellerID == UserID).FirstOrDefault();
                if (application != null)
                    return new ActionOutput { Message = "You have already applied to this job.", Status = ActionStatus.Error };
                context.JobApplications.AddObject(new JobApplication
                {
                    JobID = JobID,
                    JobOwnerID = job.BuyerID,
                    BidAmount = BidAmount,
                    SellerID = UserID,
                    ShowIn = (int)JobShowIn.JobsApplied,
                    CreatedOn = DateTime.Now
                });
                if (job.IsPrivate) job.IsApplied = true;
                context.SaveChanges();
                // log alert into database
                AccountActivityHandler.SaveAlert(new Alert
                {
                    AlertForID = job.JobID,
                    AlertLink = "/User/MyJob/" + job.JobID + "/" + Utility.SpacesToHifen(job.JobTitle),
                    AlertText = username + " has applied on your job \"" + job.JobTitle + "\".",
                    UserID = job.BuyerID
                });
                return new ActionOutput { Message = "You have successfuly applied to this job.", Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput<BuyerJob> GetJob(int id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<BuyerJob>
                {
                    Status = ActionStatus.Successfull,
                    Object = context.BuyerJobs.Include("BuyerJobAttachments").Include("User1").Where(m => m.JobID == id).FirstOrDefault()
                };
            }
        }

        public static ActionOutput<BuyerJobAttachment> GetJobAttachment(int id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<BuyerJobAttachment>
                {
                    Status = ActionStatus.Successfull,
                    Object = context.BuyerJobAttachments.Where(m => m.AttachmentID == id).FirstOrDefault()
                };
            }
        }

        public static ActionOutput<JobApplication> GetJobApplicant(int JobID, int UserID)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<JobApplication>
                {
                    Status = ActionStatus.Successfull,
                    Object = context.JobApplications.Where(m => m.JobID == JobID && m.SellerID == UserID).FirstOrDefault()
                };
            }
        }

        public static ActionOutput<ShopViewModel> GetJobApplicants(int JobID)
        {
            using (var context = new CentroEntities())
            {
                var shops = context.Shops.Include("User3")
                                         .Join(context.JobApplications,
                                            s => s.UserId,
                                            a => a.SellerID,
                                            (s, a) => new { s, a })
                                        .Where(m => m.a.JobID == JobID)
                                        .Select(m => new ShopViewModel
                                        {
                                            ShopName = m.s.ShopName,
                                            ShopOwnerName = m.s.User3.UserName,
                                            ShopID = m.s.ShopID,
                                            UserID = m.s.UserId,
                                            BidAmount = m.a.BidAmount,
                                            JobApplicationID = m.a.JobApplicationID,
                                            RequestSent = m.a.RequestSent,
                                            RequestAccepted = m.a.RequestAccepted,
                                            AvgRating=m.s.Feedbacks.Select(f=> (int?)f.Rating).Average() ?? 0,
                                            FeedbackCount = m.s.Feedbacks.Count()
                                        })
                                        .ToList();

                return new ActionOutput<ShopViewModel> { List = shops, Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput<ShopViewModel> GetJobApplicants(DateTime From, DateTime To, int UserID)
        {
            using (var context = new CentroEntities())
            {
                var shops = context.Shops.Include("User3").Join(context.JobApplications,
                                            s => s.UserId,
                                            a => a.SellerID,
                                            (s, a) => new { s, a })
                                        .Where(m => m.a.CreatedOn >= From && m.a.CreatedOn <= To && m.a.JobOwnerID == UserID)
                                        .Select(m => new ShopViewModel
                                        {
                                            ShopName = m.s.ShopName,
                                            ShopOwnerName = m.s.User3.UserName,
                                            ShopID = m.s.ShopID,
                                            UserID = m.s.UserId,
                                            BidAmount = m.a.BidAmount,
                                            JobApplicationID = m.a.JobApplicationID,
                                            RequestSent = m.a.RequestSent,
                                            RequestAccepted = m.a.RequestAccepted
                                        })
                                        .ToList();
                return new ActionOutput<ShopViewModel> { List = shops, Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput SetSentTo(int JobID, int UserID)
        {
            using (var context = new CentroEntities())
            {
                var jobapp = context.JobApplications.Where(m => m.JobID == JobID && m.SellerID == UserID).FirstOrDefault();
                //var job=context.BuyerJobs.Where(m=>m.JobID==JobID).
                jobapp.RequestSent = true;
                jobapp.ShowIn = (int)JobShowIn.InterviewInvited;
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Request has been sent." };
            }
        }

        public static ActionOutput AwardJobTo(int JobID, int UserID, int ShopID, string username)
        {
            using (var context = new CentroEntities())
            {
                PrototypeRequest req;
                var job = context.BuyerJobs.Include("BuyerJobAttachments").Where(m => m.JobID == JobID).FirstOrDefault();
                var jobowner = context.Users.Where(m => m.UserID == job.BuyerID).FirstOrDefault();
                job.AwardedTo = UserID;
                job.IsActive = false;

                // Check for existing contract between buyer/seller
                req = context.PrototypeRequests.Where(m => m.ShopId == ShopID && m.BuyerId == job.BuyerID).FirstOrDefault();
                if (req != null)
                {
                    // Delete this job and redirect to existing contract page with msg and set status to working
                    req.RequestStatus = (int)CustomRequestStatus.Working;
                    //context.DeleteObject(job);
                }
                else
                {
                    // make entry in PrototypeRequest 
                    req = new PrototypeRequest
                    {
                        BuyerId = job.BuyerID,
                        CreatedBy = job.BuyerID,
                        CreatedOn = DateTime.Now,
                        Description = job.JobDescription,
                        Dimensions = job.Dimensions,
                        IsDeleted = false,
                        IsRead = false,
                        Material = job.Material,
                        MaxBudget = job.MaxBudget,
                        MinBudget = job.MinBudget,
                        RequestStatus = (int)CustomRequestStatus.Accepted,
                        RequestTitle = job.JobTitle,
                        SellerId = UserID,
                        ShopId = ShopID,
                        UpdatedBy = null,
                        UpdatedOn = null,
                        IdealStartDate = job.IdealStartDate
                    };
                    context.PrototypeRequests.AddObject(req);
                    context.SaveChanges();
                    // Copy All attachments
                    foreach (var item in job.BuyerJobAttachments)
                    {
                        context.RequestAttachments.AddObject(new RequestAttachment
                        {
                            CreatedOn = DateTime.Now,
                            DisplayName = item.DisplayName,
                            IsClientApproved = null,
                            IsContractorApproved = null,
                            MimeType = item.MimeType,
                            RequestId = req.RequestID,
                            SavedName = item.SavedName,
                            SizeInBytes = item.SizeInBytes,
                            SizeInKb = item.SizeInKb,
                            SizeInMb = item.SizeInMb
                        });
                    }
                    // Delete the job
                    //context.DeleteObject(job);
                }
                //context.BuyerJobs.DeleteObject(job);
                job.IsDeleted = true;
                job.IsActive = false;

                var jobApplicant = context.JobApplications.Where(m => m.JobID == JobID && m.SellerID == UserID).FirstOrDefault();
                if (jobApplicant != null)
                {
                    jobApplicant.RequestAccepted = true;
                }
                context.SaveChanges();

                // Send Email to Seller for job confirmation.
                User seller = context.Users.Where(m => m.UserID == UserID).FirstOrDefault();
                //// log alert into database
                //AccountActivityHandler.SaveAlert(new Alert
                //{
                //    AlertForID = job.JobID,
                //    AlertLink = "/Buyer/Job/" + job.JobID + "/" + Utility.SpacesToHifen(job.JobTitle),
                //    AlertText = jobowner.UserName + " has awarded a job \"" + job.JobTitle + "\" to you.",
                //    UserID = UserID
                //});
                EmailHandler.JobConfirmation(req, seller);
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "Job has been awarded to selected seller.",
                    Results = new List<string> { req.RequestID.ToString() }
                };
            }
        }

        public static ActionOutput DeleteJobEntry(int JobApplicationID)
        {
            using (var context = new CentroEntities())
            {
                var entry = context.JobApplications.Where(m => m.JobApplicationID == JobApplicationID).FirstOrDefault();
                context.DeleteObject(entry);
                context.SaveChanges();
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "Entry has been deleted."
                };
            }
        }

        public static ActionOutput<JobApplication> GetMyJobApplicantions(int UserID)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<JobApplication> { List = context.JobApplications.Where(m => m.SellerID == UserID).ToList(), Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput WithdrawApplication(int JobID, int UserID)
        {
            using (var context = new CentroEntities())
            {
                var application = context.JobApplications.Where(m => m.JobID == JobID && m.SellerID == UserID).FirstOrDefault();
                var job = context.BuyerJobs.Where(m => m.JobID == JobID).FirstOrDefault();
                job.IsApplied = false;
                context.DeleteObject(application);
                context.SaveChanges();
                return new ActionOutput { Message = "You have successfuly withdraw your application for this job.", Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput DeleteJob(int JobID)
        {
            using (var context = new CentroEntities())
            {
                var job = context.BuyerJobs.Where(m => m.JobID == JobID).FirstOrDefault();
                context.DeleteObject(job);
                try
                {
                    context.SaveChanges();
                    return new ActionOutput { Message = "You have successfuly deleted your job.", Status = ActionStatus.Successfull };
                }
                catch
                {
                    return new ActionOutput { Message = "People have applied to this job, so it can't be deleted.", Status = ActionStatus.Error };
                }
            }
        }

        public static ActionOutput ActivateDeactivateJob(int JobID, bool IsActive)
        {
            using (var context = new CentroEntities())
            {
                var job = context.BuyerJobs.Where(m => m.JobID == JobID).FirstOrDefault();
                job.IsActive = IsActive;
                context.SaveChanges();
                return new ActionOutput { Message = "You have successfuly " + (IsActive ? "activated" : "deactivated") + " your job.", Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput<PrototypeRequest> GetCountractByBuyerAndSeller(int BuyerID, int SellerID)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<PrototypeRequest> { Object = context.PrototypeRequests.Where(m => m.BuyerId == BuyerID && m.SellerId == SellerID).FirstOrDefault(), Status = ActionStatus.Successfull };
            }
        }

        public static PagingResult<OrderViewModel> GetAllOrders(int page_no, int per_page_result, string sortColumn, string sortOrder, string search)
        {
            using (var context = new CentroEntities())
            {
                var query = context.Orders.OrderBy("it." + sortColumn + " " + sortOrder)
                                           .Join(context.Users,
                                               o => o.UserID,
                                               u => u.UserID,
                                               (o, u) => new { o, u })
                                           .Join(context.Shops,
                                               ss => ss.o.UserID,
                                               s => s.UserId,
                                               (ss, s) => new { ss.o, ss.u, s });

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(i => i.u.UserName.StartsWith(search) || i.u.FirstName.StartsWith(search) || i.u.LastName.StartsWith(search) || i.s.ShopName.StartsWith(search));

                var NewQuery = query.OrderByDescending(m => m.o.CreatedOn).Skip(page_no - 1).Take(per_page_result)
                                                      .Select(o => new OrderViewModel
                                                                  {
                                                                      OrderId = o.o.OrderID,
                                                                      ShopId = o.s.ShopID,
                                                                      ShopOwnerId = o.s.UserId,
                                                                      ShopName = o.s.ShopName,
                                                                      BuyerName = o.u.UserName,
                                                                      BuyerId = o.o.UserID,
                                                                      OrderAmount = o.o.TotalAmountToBePaid,
                                                                      OrderCreatedOn = o.o.CreatedOn,
                                                                      ShippingStatus = (ShippingStatus)o.o.ShippingStatusId,
                                                                      OrderStatus = (OrderStatus)o.o.OrderStatusId
                                                                  });
                return new PagingResult<OrderViewModel>
                {
                    List = NewQuery.ToList(),
                    TotalCount = query.Count(),
                    Status = ActionStatus.Successfull
                };
            }
        }


        public static void ExpireJobs(int days)
        {
            using (var context = new CentroEntities())
            {
                DateTime compareTo = DateTime.Now.AddDays(-days);
                var jobs = context.BuyerJobs.Where(m => m.CreatedOn <= compareTo).ToList();
                if (jobs != null && jobs.Count() > 0)
                {
                    foreach (var item in jobs)
                    {
                        item.IsActive = false;
                    }
                    context.SaveChanges();
                }
            }
        }

        public static ActionOutput MoveTo(List<long> RequestIds, string MoveTo)
        {
            using (var context = new CentroEntities())
            {
                var requests = context.PrototypeRequests.Where(m => RequestIds.Contains(m.RequestID)).ToList();
                foreach (var item in requests)
                {
                    item.RequestStatus = MoveTo.ToLower() == "past" ? (int)CustomRequestStatus.Completed : (int)CustomRequestStatus.Working;
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Contracts have been moved to " + MoveTo };
            }
        }

        public static ActionOutput UpdateShopSkills(int ShopID, string Skills)
        {
            using (var context = new CentroEntities())
            {
                var shop = context.Shops.Where(m => m.ShopID == ShopID).FirstOrDefault();
                shop.Skills = Skills;
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull };
            }
        }

        public static void UpdateShopServices(int ShopID, int[] ShopServices)
        {
            using (var context = new CentroEntities())
            {
                var services = context.ShopServices.Where(m => m.ShopID == ShopID).ToList();
                foreach (var item in services)
                {
                    context.DeleteObject(item);
                }
                if (ShopServices != null)
                {
                    foreach (var item in ShopServices)
                    {
                        context.ShopServices.AddObject(new ShopService { ServiceID = item, ShopID = ShopID, ServiceType = (int)ServiceType.ProfessionalServices });
                    }
                }
                context.SaveChanges();
            }
        }

        public static void UpdateShopAcceptProject(int ShopID, bool AcceptProject)
        {
            using (var context = new CentroEntities())
            {
                var shop = context.Shops.Where(m => m.ShopID == ShopID).FirstOrDefault();
                shop.AcceptJob = AcceptProject;
                context.SaveChanges();
            }
        }

        public static ActionOutput CancelOrder(int OrderId)
        {
            using (var context = new CentroEntities())
            {
                var order = context.Orders.Where(m => m.OrderID == OrderId).FirstOrDefault();
                if (order.ShippingStatusId != (int)ShippingStatus.NotShipped)
                    return new ActionOutput { Status = ActionStatus.Error, Message = "Order can not be canceled." };
                order.OrderStatusId = (int)OrderStatus.Canceled;
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Order has been canceled." };
            }
        }

        public static ActionOutput<SalesTax> GetSalesTax(int ShopID)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<SalesTax> { Status = ActionStatus.Successfull, List = context.SalesTaxes.Where(m => m.ShopID == ShopID).ToList() };
            }
        }

        public static ActionOutput UpdateCustomRequestTitle(long RequestID, string Title)
        {
            using (var context = new CentroEntities())
            {
                var request = context.PrototypeRequests.Where(m => m.RequestID == RequestID).FirstOrDefault();
                request.RequestTitle = Title;
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Title has been updated." };
            }
        }

        public static ActionOutput UpdateNextStep(long ID, int NextStep)
        {
            using (var context = new CentroEntities())
            {
                var contract = context.PrototypeRequests.Where(m => m.RequestID == ID).FirstOrDefault();
                if (contract != null)
                {
                    contract.NextStep = NextStep;
                    context.SaveChanges();
                    return new ActionOutput { Status = ActionStatus.Successfull, Message = "Next step has been updated." };
                }
                return new ActionOutput { Status = ActionStatus.Error, Message = "Contract not found." };
            }
        }

        public static ActionOutput<decimal?> TotalSpent(int UserID)
        {
            using (var context = new CentroEntities())
            {
                var data = context.Orders.Where(m => m.UserID == UserID);
                return new ActionOutput<decimal?>
                {
                    Status = ActionStatus.Successfull,
                    Object = data.Any() ? (decimal?)data.Sum(m => m.TotalAmountToBePaid) : null
                };
            }
        }

        public static ActionOutput<JobsViewModel> GetRandomJobs(int item_count)
        {
            using (var context = new CentroEntities())
            {
                List<JobsViewModel> list = new List<JobsViewModel>();
                list = context.BuyerJobs.OrderBy(m => Guid.NewGuid()).Take(item_count)
                                       .Select(m => new JobsViewModel
                                       {
                                           AwardedTo = m.AwardedTo,
                                           BuyerID = m.BuyerID,
                                           CategoryID = m.CategoryId,
                                           CreatedOn = m.CreatedOn,
                                           IsActive = m.IsActive,
                                           IsDeleted = m.IsDeleted,
                                           IsFavorite = m.IsFavorite,
                                           IsPrivate = m.IsPrivate,
                                           IsRead = m.IsRead,
                                           JobDescription = m.JobDescription,
                                           JobID = m.JobID,
                                           JobSentTo = m.JobSentTo,
                                           JobTitle = m.JobTitle,
                                           MaxBudget = m.MaxBudget,
                                           Requirements = m.Requirements
                                       })
                                       .ToList();

                return new ActionOutput<JobsViewModel>
                {
                    Status = ActionStatus.Successfull,
                    List = list
                };
            }
        }

        public static ActionOutput<Category> Category(string name)
        {
            using (var context = new CentroEntities())
            {
                var cat = context.Categories.Where(m => m.Name.Equals(name)).FirstOrDefault();
                return new ActionOutput<Category> { Object = cat, Status = ActionStatus.Successfull };
            }
        }
    }
}