using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Classes;

namespace BusinessLayer.Handler
{
    public static class HubHandler
    {
        public static ActionOutput<HubTopic> GetHubTopics()
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<HubTopic>
                {
                    Status = ActionStatus.Successfull,
                    List = context.HubTopics.ToList()
                };
            }
        }

        public static ActionOutput SaveHub(Hub obj, int user_id)
        {
            using (var context = new CentroEntities())
            {
                obj.CreatedOn = DateTime.Now;
                obj.UserID = user_id;
                obj.TotalView = 0;
                context.Hubs.AddObject(obj);
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, ID = obj.HubID, Results = new List<string> { obj.HubID.ToString(),obj.Title} };
            }
        }

        public static ActionOutput UpdateHub(Hub obj, int user_id)
        {
            using (var context = new CentroEntities())
            {
                Hub hub_data = context.Hubs.Where(m => m.HubID == obj.HubID && m.UserID == user_id).FirstOrDefault();
                if (hub_data != null)
                {
                    hub_data.HubTopicID = obj.HubTopicID;
                    hub_data.HubTemplateID = obj.HubTemplateID;
                    hub_data.Title = obj.Title;
                    hub_data.HubURL = obj.HubURL;
                    hub_data.HubPicture = obj.HubPicture;
                    hub_data.Description = obj.Description;
                    hub_data.Keywords = obj.Keywords;
                    context.SaveChanges();
                    return new ActionOutput
                    {
                        Status = ActionStatus.Successfull
                    };
                }
                else
                {
                    return new ActionOutput
                    {
                        Status = ActionStatus.Error
                    };
                }
            }
        }

        public static ActionOutput SaveHubContents(List<HubContent> HubContent)
        {
            using (var context = new CentroEntities())
            {
                foreach (var item in HubContent)
                {
                    context.HubContents.AddObject(item);
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Hub has been saved." };
            }
        }

        public static ActionOutput<Hub> GetHubs(int user_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Hub>
                {
                    Status = ActionStatus.Successfull,
                    List = context.Hubs.Include("HubContents").Where(m => m.UserID == user_id).ToList()
                };
            }
        }

        public static ActionOutput<HubTopic> GetHubTopicByTopicID(int topic_id)
        {
            using (var context = new CentroEntities())
            {
                HubTopic hub_topic = context.HubTopics.Where(m => m.HubTopicID == topic_id).FirstOrDefault();
                return new ActionOutput<HubTopic> { Status = ActionStatus.Successfull, Object = hub_topic };
            }
        }

        public static ActionOutput<Hub> GetRandomHubs(int item_count, string sort_by = "")
        {
            using (var context = new CentroEntities())
            {
                List<Hub> list = new List<Hub>();
                if (sort_by.ToLower() == "mostrecent")
                {
                    list = context.Hubs.Include("HubContents").Where(m => m.HubStatus == (int)HubStatus.Active).OrderByDescending(m => m.CreatedOn).Take(item_count).ToList();
                }
                else if (sort_by.ToLower() == "mostreviewed")
                {
                    list = context.Hubs.Include("HubContents").Where(m => m.HubStatus == (int)HubStatus.Active).OrderByDescending(m => m.TotalView).Take(item_count).ToList();
                }
                else
                {
                    list = context.Hubs.Include("HubContents").Where(m => m.HubStatus == (int)HubStatus.Active).OrderBy(m => Guid.NewGuid()).Take(item_count).ToList();
                }
                foreach (var item in list)
                {
                    var shop=context.Shops.Where(m => m.UserId == item.UserID).FirstOrDefault();
                    item.HubShopName = shop != null ? shop.ShopName : item.HubOwnerUsername;
                }
                return new ActionOutput<Hub>
                {
                    Status = ActionStatus.Successfull,
                    List = list
                };
            }
        }

        public static ActionOutput<Hub> GetHubById(int id, int user_id)
        {
            using (var context = new CentroEntities())
            {
                Hub hub = context.Hubs.Where(m => m.UserID == user_id && m.HubID == id).FirstOrDefault();
                hub.HubItems = context.HubContents.Where(m => m.HubID == id).ToList();
                hub.HubTopicText = context.HubTopics.Where(m => m.HubTopicID == hub.HubTopicID).FirstOrDefault().HubTopic1;
                hub.HubOwnerUsername = context.Users.Where(m => m.UserID == hub.UserID).FirstOrDefault().UserName;
                return new ActionOutput<Hub>
                {
                    Status = ActionStatus.Successfull,
                    Object = hub
                };
            }
        }

        public static ActionOutput<Hub> GetHubByShopId(int id, int shop_id)
        {
            using (var context = new CentroEntities())
            {
                Hub hub = context.Hubs.Join(context.Shops,
                                            h => h.UserID,
                                            s => s.UserId,
                                            (h, s) => new { h, s })
                                      .Where(m => m.s.ShopID == shop_id && m.h.HubID == id)
                                      .Select(m => m.h)
                                      .FirstOrDefault();
                hub.HubItems = context.HubContents.Where(m => m.HubID == id).ToList();
                hub.HubTopicText = context.HubTopics.Where(m => m.HubTopicID == hub.HubTopicID).FirstOrDefault().HubTopic1;
                hub.HubOwnerUsername = context.Users.Where(m => m.UserID == hub.UserID).FirstOrDefault().UserName;
                return new ActionOutput<Hub>
                {
                    Status = ActionStatus.Successfull,
                    Object = hub
                };
            }
        }

        public static ActionOutput<Hub> GetHubByShopId(string title, int shop_id)
        {
            using (var context = new CentroEntities())
            {
                Hub hub = context.Hubs.Join(context.Shops,
                                            h => h.UserID,
                                            s => s.UserId,
                                            (h, s) => new { h, s })
                                      .Where(m => m.s.ShopID == shop_id && m.h.Title == title)
                                      .Select(m => m.h)
                                      .FirstOrDefault();
                hub.HubItems = context.HubContents.Where(m => m.HubID == hub.HubID).ToList();
                hub.HubTopicText = context.HubTopics.Where(m => m.HubTopicID == hub.HubTopicID).FirstOrDefault().HubTopic1;
                hub.HubOwnerUsername = context.Users.Where(m => m.UserID == hub.UserID).FirstOrDefault().UserName;
                return new ActionOutput<Hub>
                {
                    Status = ActionStatus.Successfull,
                    Object = hub
                };
            }
        }

        public static ActionOutput<Hub> GetHubByHubTitle(string title, int user_id)
        {
            using (var context = new CentroEntities())
            {
                Hub hub = context.Hubs.Where(h => h.Title == title && h.UserID == user_id).FirstOrDefault();
                hub.HubItems = context.HubContents.Where(h => h.HubID == hub.HubID).ToList();
                hub.HubTopicText = context.HubTopics.Where(m => m.HubTopicID == hub.HubTopicID).FirstOrDefault().HubTopic1;
                hub.HubOwnerUsername = context.Users.Where(m => m.UserID == hub.UserID).FirstOrDefault().UserName;
                return new ActionOutput<Hub>
                {
                    Status = ActionStatus.Successfull,
                    Object = hub
                };
            }
        }

        public static ActionOutput ActivateDeactivateHub(int HubID, int Status)
        {
            using (var context = new CentroEntities())
            {
                var hub = context.Hubs.Where(m => m.HubID == HubID).FirstOrDefault();
                hub.HubStatus = Status;
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = Status == (int)HubStatus.InActive ? "Hub has been deactivated" : "Hub has been activated" };
            }
        }

        public static ActionOutput DeleteHubContent(int hub_content_id)
        {
            using (var context = new CentroEntities())
            {
                HubContent obj = context.HubContents.Where(m => m.HubContentID == hub_content_id).FirstOrDefault();
                if (obj != null)
                {
                    context.HubContents.DeleteObject(obj);
                    context.SaveChanges();
                    return new ActionOutput
                    {
                        Status = ActionStatus.Successfull
                    };
                }
                else
                {
                    return new ActionOutput
                    {
                        Status = ActionStatus.Error
                    };
                }
            }
        }

        public static ActionOutput UpdateHubContent(HubContent obj, UserProductPicture pic_obj)
        {
            using (var context = new CentroEntities())
            {
                HubContent objExist = context.HubContents.Where(m => m.HubContentID == obj.HubContentID).FirstOrDefault();
                if (objExist != null)
                {
                    if (pic_obj == null)
                    {
                        objExist.ContentText = obj.ContentText;
                        if (string.IsNullOrEmpty(obj.SavedName))
                        {
                            objExist.SavedName = null;
                        }

                    }
                    else
                    {
                        objExist.ContentText = obj.ContentText;
                        objExist.DisplayName = pic_obj != null ? pic_obj.DisplayName : null;
                        objExist.MimeType = pic_obj != null ? pic_obj.MimeType : null;
                        objExist.SavedName = pic_obj != null ? pic_obj.SavedName : null;
                        objExist.SizeInBytes = pic_obj != null ? (int?)pic_obj.SizeInBytes : null;
                        objExist.SizeInKB = pic_obj != null ? (decimal?)pic_obj.SizeInKB : null;
                        objExist.SizeInMB = pic_obj != null ? (decimal?)pic_obj.SizeInMB : null;
                        objExist.Thumbnail = pic_obj != null ? pic_obj.Thumbnail : null;
                    }
                    context.SaveChanges();
                    return new ActionOutput
                    {
                        Status = ActionStatus.Successfull
                    };
                }
                else
                {
                    return new ActionOutput
                    {
                        Status = ActionStatus.Error
                    };
                }
            }
        }

        public static ActionOutput SaveHubComment(HubComment obj)
        {
            using (var context = new CentroEntities())
            {
                obj.CreatedOn = DateTime.Now;
                obj.IsDeleted = false;
                context.HubComments.AddObject(obj);
                context.SaveChanges();
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "Comment saved successfully",
                    ID = obj.CommentID
                    
                };
            }
        }

        public static ActionOutput<HubCommentViewModel> GetHubComments(int hubId)
        {
            using (var context = new CentroEntities())
            {
                List<HubCommentViewModel> list = (from hc in context.HubComments
                             join h in context.Hubs
                             on hc.HubID equals h.HubID
                             join u in context.Users
                             on hc.UserID equals u.UserID
                             join p in context.Pictures
                             on u.ProfilePicId equals p.PictureID into ijoin // left join
                             from lj in ijoin.DefaultIfEmpty()
                             where hc.HubID == hubId && !hc.IsDeleted
                             select new HubCommentViewModel
                             {
                                 CommentID = hc.CommentID,
                                 HubID = hc.HubID,
                                 UserID = hc.UserID,
                                 UserName = u.UserName,
                                 HubTitle = h.Title,
                                 CreatedOn = hc.CreatedOn,
                                 Comment = hc.Comment,
                                 UserImage = lj.SavedName,
                                 HubUserID = h.UserID
                             }).OrderByDescending(m => m.CreatedOn).ToList();


                return new ActionOutput<HubCommentViewModel>
                {
                    Status = ActionStatus.Successfull,
                    List = list
                };
            }
        }

        public static ActionOutput<HubCommentViewModel> GetHubCommentByCommentID(int hub_comment_id)
        {
            using (var context = new CentroEntities())
            {
                HubCommentViewModel obj = (from hc in context.HubComments
                                                  join h in context.Hubs
                                                  on hc.HubID equals h.HubID
                                                  join u in context.Users
                                                  on hc.UserID equals u.UserID
                                                  join p in context.Pictures
                                                  on u.ProfilePicId equals p.PictureID into ijoin // left join
                                                  from lj in ijoin.DefaultIfEmpty()
                                                  where hc.CommentID == hub_comment_id && !hc.IsDeleted
                                                  select new HubCommentViewModel
                                                  {
                                                      CommentID = hc.CommentID,
                                                      HubID = hc.HubID,
                                                      UserID = hc.UserID,
                                                      UserName = u.UserName,
                                                      HubTitle = h.Title,
                                                      CreatedOn = hc.CreatedOn,
                                                      Comment = hc.Comment,
                                                      UserImage = lj.SavedName,
                                                      HubUserID = h.UserID
                                                  }).OrderByDescending(m => m.CreatedOn).FirstOrDefault();

                return new ActionOutput<HubCommentViewModel>
                {
                    Status = ActionStatus.Successfull,
                    Object = obj
                };
            }
        }

        public static ActionOutput DeleteCommentByCommentID(int hub_comment_id)
        {
            using (var context = new CentroEntities())
            {
                HubComment obj = context.HubComments.Where(hc => hc.CommentID == hub_comment_id).FirstOrDefault();
                if (obj != null)
                {
                    obj.IsDeleted = true;
                    context.SaveChanges();
                    return new ActionOutput
                    {
                        Status = ActionStatus.Successfull,
                        Message = "Comment deleted successfully."
                    };
                }
                else
                {
                    return new ActionOutput
                    {
                        Status = ActionStatus.Error,
                        Message = "Comment not deleted."
                    };
                }

            }
        }

        public static ActionOutput<Hub> GetHubByHubTopicID(int hub_topic_id)
        {
            using (var context = new CentroEntities())
            {
                List<Hub> list = context.Hubs.Join(context.HubTopics,
                                                h => h.HubTopicID,
                                                hp => hp.HubTopicID,
                                                (h, hp) => new { h, hp })
                                                .Where(m => m.hp.HubTopicID == hub_topic_id)
                                                .Select(m => m.h).ToList();
                return new ActionOutput<Hub>
                {
                    Status = ActionStatus.Successfull,
                    List = list
                };

            }
        }

        public static void SetHubPageView(int HubID)
        {
            using (var context = new CentroEntities())
            {
                var counter = context.Hubs.Where(m => m.HubID == HubID).FirstOrDefault();
                counter.TotalView = counter.TotalView + 1;
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Return the total count of hub by user
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public static ActionOutput GetTotalHubOfUser(int user_id)
        {
            using (var context = new CentroEntities())
            {
                var result = context.Hubs.Where(m => m.UserID == user_id && m.HubStatus==1).Count();
                return new ActionOutput
                {
                    Results = new List<string> { result.ToString() }
                };
            }
        }

        public static PagingResult<Hub> GetHubListing(int page_no, int per_page_result, string sortOrder, string sortColumn, string search, int? TopicID, int? UserID, bool? AllActive=null)
        {
            using (var context = new CentroEntities())
            {
                List<Hub> list;
                int total;
                var query = context.Hubs//.Include("Category").Include("ContestParticipants")
                                   .OrderBy("it." + sortColumn + " " + sortOrder)
                                   .Where(m => (AllActive.HasValue && AllActive.Value) ? (m.HubStatus == (int)HubStatus.Active || m.HubStatus == (int)HubStatus.InActive) : m.HubStatus == (int)HubStatus.Active);
                                   

                if (TopicID.HasValue && TopicID.Value > 0)
                    query = query.Where(m => m.HubTopicID == TopicID.Value);
                if (UserID.HasValue && UserID.Value > 0)
                    query = query.Where(m => m.UserID == UserID.Value);
                if (!string.IsNullOrEmpty(search))
                    query = query.Where(m => m.Title.Contains(search) || m.Description.Contains(search) || m.Keywords.Contains(search));

                string q=query.ToTraceString();

                list = query.Skip(page_no - 1)
                            .Take(per_page_result)
                            .ToList();

                total = query.Count();

                PagingResult<Hub> pagingResult = new PagingResult<Hub>();
                pagingResult.List = list;
                pagingResult.TotalCount = total;
                pagingResult.Status = ActionStatus.Successfull;
                return pagingResult;
            }
        }
    }
}
