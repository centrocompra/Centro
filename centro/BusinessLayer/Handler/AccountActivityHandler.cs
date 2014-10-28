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
    public static class AccountActivityHandler
    {
        public static void SaveActivity(AccountActivity obj)
        {
            using (var context = new CentroEntities())
            {
                obj.CreatedOn = DateTime.Now;
                context.AccountActivities.AddObject(obj);
                context.SaveChanges();
            }
        }

        public static ActionOutput<AccountActivity> GetAccountActivity(int AccountActivityID)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<AccountActivity> { Object = context.AccountActivities.Where(m => m.AccountActivityID == AccountActivityID).FirstOrDefault(), Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput<AccountActivity> GetMyAccountActivityList(int UserID)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<AccountActivity> { List = context.AccountActivities.Where(m => m.UserID == UserID).ToList(), Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput<AccountActivityViewModel> ActivityFeeds(int UserID)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<AccountActivityViewModel>
                {
                    List = context.AccountActivities.Join(context.Followers,
                                                            a => a.UserID,
                                                            f => f.UserID,
                                                            (a, f) => new { a, f })
                                                            .Where(m => m.f.FollowedByID == UserID)
                                                            .Select(m => new AccountActivityViewModel
                                                            {
                                                                AccountActivityID = m.a.AccountActivityID,
                                                                ActivityID = m.a.ActivityID,
                                                                ActivityType = m.a.ActivityType,
                                                                ActivityText = m.a.ActivityText,
                                                                UserID = m.a.UserID,
                                                                CreatedOn = m.a.CreatedOn,
                                                                Username = m.f.User1.UserName,
                                                                FollowingToName = m.a.User.UserName,
                                                                ActivityImage = m.a.ActivityImage,
                                                                ActivityLink = m.a.ActivityLink
                                                            })
                                                            .ToList(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput SaveFollower(int[] list, int FollowedByID, int UserID)
        {
            using (var context = new CentroEntities())
            {
                int[] ToDeleteIds = list;
                var Existing = context.Followers.Where(m => m.FollowedByID == FollowedByID && m.UserID == UserID).ToList();
                var ToDelete = Existing.Where(m => !ToDeleteIds.Contains(m.FollowType)).ToList();
                foreach (var delete in ToDelete)
                {
                    context.DeleteObject(delete);
                }
                int[] ExistingIds = Existing.Select(m => m.FollowType).ToArray();
                foreach (var item in list.Where(m => !ExistingIds.Contains(m)).ToList())
                {
                    context.Followers.AddObject(new Follower { FollowedByID = FollowedByID, FollowType = item, UserID = UserID });
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput RemoveFolower(int followedBYID, int UserID)
        {
            using (var context = new CentroEntities())
            {
                var Existing = context.Followers.Where(m => m.FollowedByID == followedBYID && m.UserID == UserID).ToList();
                foreach (var item in Existing)
                {
                    context.Followers.DeleteObject(item);
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput<Follower> GetFollower(int userid, int followed_userid)
        {
            using (var context = new CentroEntities())
            {
                var result = context.Followers.Where(m => m.FollowedByID == userid && m.UserID == followed_userid).ToList();
                return new ActionOutput<Follower>
                {
                    Status = ActionStatus.Successfull,
                    List = result
                };
            }
        }

        public static ActionOutput GetUserTotalFollowers(int userid)
        {
            using (var context = new CentroEntities())
            {
                var result = context.Followers.Where(m => m.UserID == userid).Select(m => m.FollowedByID).Distinct().ToList().Count;
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    ID = result
                };
            }
        }

        public static PagingResult<AccountActivityViewModel> ActivityFeeds(int page_no, int per_page_result, string sortOrder, string sortColumn, DateTime? FeedsAfter, int UserID)
        {
            using (var context = new CentroEntities())
            {
                var query = context.AccountActivities.Join(context.Followers,
                                                            a => a.UserID,
                                                            f => f.UserID,
                                                            (a, f) => new { a, f })
                                                            .Where(m => m.f.FollowedByID == UserID && m.a.ActivityType == m.f.FollowType);
                if (FeedsAfter.HasValue)
                    query = query.Where(m => m.a.CreatedOn >= FeedsAfter);

                var list = query.OrderByDescending(m => m.a.CreatedOn)
                                .Skip(page_no - 1)
                                .Take(per_page_result)
                                .Select(m => new AccountActivityViewModel
                                {
                                    AccountActivityID = m.a.AccountActivityID,
                                    ActivityID = m.a.ActivityID,
                                    ActivityType = m.a.ActivityType,
                                    ActivityText = m.a.ActivityText,
                                    UserID = m.a.UserID,
                                    CreatedOn = m.a.CreatedOn,
                                    Username = m.f.User1.UserName,
                                    FollowingToName = m.a.User.UserName,
                                    ActivityImage = m.a.ActivityImage,
                                    ActivityLink = m.a.ActivityLink
                                })
                                .ToList();

                int total = query.Select(m => m.a)
                                 .Distinct()
                                 .Count();

                PagingResult<AccountActivityViewModel> pagingResult = new PagingResult<AccountActivityViewModel>();
                pagingResult.List = list;
                pagingResult.TotalCount = total;
                pagingResult.Status = ActionStatus.Successfull;
                return pagingResult;
            }
        }


        public static ActionOutput<User> GetUserFollowers(int userid)
        {
            using (var context = new CentroEntities())
            {
                var result = (from f in context.Followers
                             join u in context.Users on f.FollowedByID equals u.UserID
                             join s in context.Shops on f.FollowedByID equals s.UserId into fus
                             from shop in fus.DefaultIfEmpty()
                             where f.UserID == userid
                             select new
                             {
                                 u,
                                 shop
                             }).Distinct().ToList();

                //var result = context.Followers.Join(context.Users,
                //                                   f => f.FollowedByID,
                //                                   u => u.UserID,
                //                                   (f, u) => new { f, u })
                //                                   .Join(context.Shops,
                //                                            u => u.f.FollowedByID,
                //                                            s => s.UserId,
                //                                            (u, s) => new { u.u, u.f, s })
                //                                    .Where(m => m.f.UserID == userid)
                //    //.Select(m => m.u).Distinct()
                //                                   .ToList();
                var users = result.Select(m => m.u).Distinct().ToList();
                foreach (var item in result)
                {
                    item.u.ShopID = item.shop != null ? item.shop.ShopID : 0;
                    item.u.ShopName = item.shop != null ? item.shop.ShopName : "";
                }
                List<int?> lstID = users.Select(p => p.ProfilePicId).ToList();
                var pic = context.Pictures.Where(m => lstID.Contains(m.PictureID)).ToList();
                foreach (var item in users)
                {
                    if (item.ProfilePicId != null)
                    {
                        var picUser = pic.Where(m => m.PictureID == item.ProfilePicId).FirstOrDefault();
                        item.ProfilePicUrl = picUser.SavedName != null ? "/Images/ProfileImage/" + item.UserName + "/thumb_" + picUser.SavedName : "";
                    }

                }

                return new ActionOutput<User>
                {
                    Status = ActionStatus.Successfull,
                    List = users
                };

            }
        }


        public static void SaveAlert(Alert obj)
        {
            using (var context = new CentroEntities())
            {
                obj.CreatedOn = DateTime.Now;
                context.Alerts.AddObject(obj);
                context.SaveChanges();
            }
        }

        public static ActionOutput<Alert> GetAlerts(int UserID, int take)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Alert>
                {
                    Status = ActionStatus.Successfull,
                    List = context.Alerts.Where(m => m.UserID == UserID)
                                         .OrderByDescending(m => m.CreatedOn)
                                         .Take(take)
                                         .ToList()
                };
            }
        }

        public static ActionOutput<Alert> GetAlerts(int UserID, int take, DateTime currentTime)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Alert>
                {
                    Status = ActionStatus.Successfull,
                    List = context.Alerts.Where(m => m.UserID == UserID && m.CreatedOn >= currentTime)
                                         .OrderByDescending(m => m.CreatedOn)
                                         .Take(take)
                                         .ToList()
                };
            }
        }

        public static PagingResult<Alert> GetAlerts(int UserID, int page_no, int size, string sortOrder, string sortColumn)
        {
            using (var context = new CentroEntities())
            {
                var query = context.Alerts.Where("it.UserID = " + UserID).OrderBy("it." + sortColumn + " " + sortOrder);

                return new PagingResult<Alert>
                {
                    Status = ActionStatus.Successfull,
                    TotalCount = query.Count(),
                    List = query.Skip(page_no - 1).Take(size).ToList()
                };
            }
        }

        public static void DeleteAllActivities(int ID)
        {
            using (var context = new CentroEntities())
            {
                var feeds = context.AccountActivities.Join(context.Followers,
                                                            a => a.UserID,
                                                            f => f.UserID,
                                                            (a, f) => new { a, f })
                                                            .Where(m => m.f.FollowedByID == ID && m.a.ActivityType == m.f.FollowType)
                                                            .Select(m => m.a).ToList();
                foreach (var item in feeds)
                {
                    context.DeleteObject(item);
                }
                context.SaveChanges();
            }
        }
    }
}