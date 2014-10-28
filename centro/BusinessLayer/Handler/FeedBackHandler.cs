using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Classes;
using System.Data.Objects;


namespace BusinessLayer.Handler
{
    public static class FeedBackHandler
    {
        public static ActionOutput SaveFeedBack(Feedback obj)
        {
            using (var context = new CentroEntities())
            {
                obj.CreatedOn = DateTime.Now;
                obj.IsActive = true;
                context.Feedbacks.AddObject(obj);
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Feedback saved successfully" };
            }
        }

        public static ActionOutput<FeedBackViewModel> GetFeedBackByProductID(int productID)
        {
            using (var context = new CentroEntities())
            {
                List<FeedBackViewModel> lst = context.Feedbacks.Join(context.Users,
                                                                f => f.UserID,
                                                                u => u.UserID,
                                                                (f, u) => new { f, u })
                                                       .Where(f => f.f.FeedBackType == (int)FeedBackType.Product && f.f.IsActive && f.f.ProductID == productID)
                                                       .Select(m => new FeedBackViewModel
                                                            {
                                                                CreatedOn = m.f.CreatedOn,
                                                                FeedBackID = m.f.FeedBackID,
                                                                FeedBackType = m.f.FeedBackType,
                                                                Fullname = m.u.FirstName + " " + m.u.LastName,
                                                                IsActive = m.f.IsActive,
                                                                ProductID = m.f.ProductID,
                                                                Rating = m.f.Rating,
                                                                ShopID = m.f.ShopID,
                                                                Review = m.f.Review,
                                                                UserID = m.f.UserID,
                                                                UserImage = m.u.ProfilePicId != null ? context.Pictures.Where(a => a.PictureID == m.u.ProfilePicId).FirstOrDefault().SavedName : null,
                                                                Username = m.u.UserName
                                                            })
                                                       .ToList();
                return new ActionOutput<FeedBackViewModel>
                {
                    Status = ActionStatus.Successfull,
                    List = lst
                };
            }
        }

        //public static ActionOutput<FeedBackViewModel> GetFeedBackByShopID(int shopID)
        //{
        //    using (var context = new CentroEntities())
        //    {
        //        List<FeedBackViewModel> lst = context.Feedbacks.Join(context.Users,
        //                                                        f => f.UserID,
        //                                                        u => u.UserID,
        //                                                        (f, u) => new { f, u }).Join(context.PrototypeRequests,
        //                                                        fe => fe.f.RequestID,
        //                                                        r => r.RequestID,
        //                                                        (fe, r) => new { fe.f, fe.u, r }).Join(context.OrderItems,
        //                                                        m => m.f.OrderID, oi => oi.OrderID, (m, oi) => new { m.f,m.r,m.u, oi }).Join(context.Products,
        //                                                        n => n.oi.ProductID, p => p.ProductID, (n, p) => new { n.f,n.r,n.u,p}).Where(f => f.f.FeedBackType == (int)FeedBackType.Shop && f.f.IsActive && f.f.ShopID == shopID)
        //                                               .Select(m => new FeedBackViewModel
        //                                               {
        //                                                   CreatedOn = m.f.CreatedOn,
        //                                                   FeedBackID = m.f.FeedBackID,
        //                                                   FeedBackType = m.f.FeedBackType,
        //                                                   Fullname = m.u.FirstName + " " + m.u.LastName,
        //                                                   IsActive = m.f.IsActive,
        //                                                   ProductID = m.f.ProductID,
        //                                                   Rating = m.f.Rating,
        //                                                   ShopID = m.f.ShopID,
        //                                                   Review = m.f.Review,
        //                                                   UserID = m.f.UserID,
        //                                                   UserImage = m.u.ProfilePicId != null ? context.Pictures.Where(a => a.PictureID == m.u.ProfilePicId).FirstOrDefault().SavedName : null,
        //                                                   Username = m.u.UserName,
        //                                                   RequestID = m.f.RequestID,
        //                                                   RequestTitle = m.r.RequestTitle,
        //                                                   OrderID = m.f.OrderID,
        //                                                   ProductName=m.p.Title,
        //                                                   ProductImage=m.p.PrimaryPicture
        //                                               })
        //                                               .ToList();
        //        return new ActionOutput<FeedBackViewModel>
        //        {
        //            Status = ActionStatus.Successfull,
        //            List = lst
        //        };
        //    }
        //}

        public static ActionOutput GetShopTotalFeedBackAndAverageRating(int shopID)
        {
            using (var context = new CentroEntities())
            {
                List<Feedback> list = context.Feedbacks.Where(f => f.ShopID == shopID && f.FeedBackType == (int)FeedBackType.Shop).ToList();
                int feedback_count = list.Count();
                int total_rating = list.Sum(a => a.Rating);
                decimal avg_rating = 0;
                if (feedback_count > 0)
                    avg_rating = (decimal)total_rating / feedback_count;
                return new ActionOutput { Status = ActionStatus.Successfull, Results = new List<string> { feedback_count.ToString(), avg_rating.ToString() } };
            }
        }

        public static PagingResult<FeedBackListing_Result> GetFeedBackByShopID(int page_no, int per_page_result, string sortColumn, string sortOrder, string search, int shopId)
        {
            using (var context = new CentroEntities())
            {
                ObjectParameter output = new ObjectParameter("TotalCount", typeof(int));
                var list = context.FeedBackListing1(page_no, per_page_result, sortColumn, sortOrder, search,output,shopId).ToList();
                PagingResult<FeedBackListing_Result> pagingResult = new PagingResult<FeedBackListing_Result>();
                pagingResult.List = list;
                pagingResult.TotalCount = Convert.ToInt32(output.Value);
                pagingResult.Status = ActionStatus.Successfull;
                return pagingResult;
            }
        }

        //public static int GetShopAverageRating(int shopID)
        //{
        //    using (var context = new CentroEntities())
        //    {
        //        ActionOutput<FeedBackViewModel> result = GetFeedBackByShopID(shopID);
        //        int count = result.List.Count();
        //        int total_rating = result.List.Sum(a => a.Rating);
        //        int avg_rating = 0;
        //        if (count > 0)
        //        {
        //            avg_rating = total_rating / count;
        //        }
        //        return avg_rating;
        //    }
        //}

        public static ActionOutput HideFeedBack(int feed_back_id)
        {
            using (var context = new CentroEntities())
            {
                Feedback obj = context.Feedbacks.Where(f => f.FeedBackID.Equals(feed_back_id)).FirstOrDefault();
                if (obj != null)
                {
                    obj.IsActive = false;
                    context.SaveChanges();
                }
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Review has been hidden." };
            }
        }

        public static Feedback GetFeedBackByRequestID(long RequestID)
        {
            using (var context = new CentroEntities())
            {
                return context.Feedbacks.Where(m => m.RequestID == RequestID).FirstOrDefault();
            }
        }
    }
}
