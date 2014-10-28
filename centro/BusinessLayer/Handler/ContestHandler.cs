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
    public static class ContestHandler
    {
        public static ActionOutput Save(Contest obj)
        {
            using (var context = new CentroEntities())
            {
                obj.CreatedOn = DateTime.Now;
                obj.IsActive = true;
                context.Contests.AddObject(obj);
                context.SaveChanges();
                return new ActionOutput
                {
                    ID = obj.ContestID,
                    Status = ActionStatus.Successfull,
                    Message = "Contest has been created successfuly.",
                    Results = new List<string> { obj.ContestID.ToString(), obj.Title }
                };
            }
        }

        public static ActionOutput SaveAttachments(List<ProductFileViewModel> list, int contestID)
        {
            using (var context = new CentroEntities())
            {
                foreach (var item in list)
                {
                    context.ContestAttachments.AddObject(new ContestAttachment
                    {
                        ContestID = contestID,
                        CreatedOn = DateTime.Now,
                        DisplayName = item.DisplayName,
                        MimeType = item.MimeType,
                        SavedName = item.SavedName,
                        SizeInBytes = item.SizeInBytes,
                        SizeInKB = item.SizeInKB,
                        SizeInMB = item.SizeInMB
                    });
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput<Contest> GetContest()
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Contest>
                {
                    List = context.Contests.Include("Category").Include("ContestParticipants").Where(m => m.EndDate.Day >= DateTime.Now.Day && m.EndDate.Month >= DateTime.Now.Month && m.EndDate.Year >= DateTime.Now.Year).ToList(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput<Contest> GetContest(int id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Contest>
                {
                    Object = context.Contests.Include("User").Include("ContestAttachments").Include("ContestParticipants").Where(m => m.ContestID == id).FirstOrDefault(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput<ContestAttachment> GetContestAttachment(int id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<ContestAttachment>
                {
                    Object = context.ContestAttachments.Include("Contest").Where(m => m.AttachmentID == id).FirstOrDefault(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput SaveParticipate(ContestParticipant model)
        {
            using (var context = new CentroEntities())
            {
                if (model.ProductID <= 0)
                    return new ActionOutput { Status = ActionStatus.Error, Message = "Please select a product first." };
                var exists = context.ContestParticipants.Where(m => m.UserID == model.UserID && m.ShopID == model.ShopID && m.ContestID == model.ContestID).FirstOrDefault();
                if (exists != null)
                    return new ActionOutput { Status = ActionStatus.Error, Message = "You have already applied to this contest." };
                model.CreatedOn = DateTime.Now;
                context.ContestParticipants.AddObject(model);
                context.SaveChanges();
                return new ActionOutput { ID = model.ContestparticipantID, Status = ActionStatus.Successfull, Message = "Your item has been submitted for this contest." };
            }
        }

        public static ActionOutput<ContestParticipantsViewModel> GetContestParticipants(int id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<ContestParticipantsViewModel>
                {
                    Status = ActionStatus.Successfull,
                    List = context.ContestParticipants.Include("ContestParticipantVotes").Where(m => m.ContestID == id)
                                  .Select(m => new ContestParticipantsViewModel
                                  {
                                      ContestParticipant = m,
                                      ProductImage = m.Product.PrimaryPicture,
                                      Username = m.Product.Manufacturer,
                                      ProductTitle = m.Product.Title,
                                      ShopID = m.Product.ShopId,
                                      VoteUp = m.ContestParticipantVotes.Sum(a => a.VoteUP),
                                      VoteDown = m.ContestParticipantVotes.Sum(a => a.VoteDown),
                                      UserID = m.UserID
                                  }).ToList()
                };
            }
        }

        public static PagingResult<ContestParticipantsViewModel> GetContestParticipants(int page_no, int per_page_result, string sortOrder, string sortColumn, string search, int ContestID)
        {
            using (var context = new CentroEntities())
            {
                var query = context.ContestParticipants.Include("ContestParticipantVotes")
                                   .OrderBy("it." + sortColumn + " " + sortOrder)
                                   .Where("it.ContestID = " + ContestID);
                return new PagingResult<ContestParticipantsViewModel>
                {
                    Status = ActionStatus.Successfull,
                    List = query.OrderBy("it." + sortColumn + " " + sortOrder).Skip(page_no - 1)
                            .Take(per_page_result)
                            .Select(m => new ContestParticipantsViewModel
                                  {
                                      ContestParticipant = m,
                                      ProductImage = m.Product.PrimaryPicture,
                                      Username = m.Product.Manufacturer,
                                      ProductTitle = m.Product.Title,
                                      ShopID = m.Product.ShopId,
                                      VoteUp = m.ContestParticipantVotes.Sum(a => a.VoteUP),
                                      VoteDown = m.ContestParticipantVotes.Sum(a => a.VoteDown),
                                      UserID=m.UserID
                                  }).ToList(),
                    TotalCount=query.Count()
                };
            }
        }

        public static ActionOutput ParticipantVoteUp(int UserID, int ContestparticipantID, int ContestID)
        {
            using (var context = new CentroEntities())
            {
                var contest = context.ContestParticipants.Where(m => m.ContestID == ContestID & m.ContestparticipantID == ContestparticipantID).FirstOrDefault();
                if (contest.UserID == UserID)
                    return new ActionOutput { Status = ActionStatus.Error, Message = "You can't vote up your own entry" };

                var existing = context.ContestParticipantVotes.Where(m => m.UserID == UserID && m.ContestparticipantID == ContestparticipantID && m.VoteUP > 0).FirstOrDefault();
                if (existing != null)
                    return new ActionOutput { Status = ActionStatus.Error, Message = "You already voted up this entry" };

                var vote = context.ContestParticipantVotes.Where(m => m.ContestparticipantID == ContestparticipantID).ToList();
                var myvote = vote.Where(m => m.UserID == UserID).FirstOrDefault();
                if (myvote != null)
                { myvote.VoteDown = null; myvote.VoteUP = 1; }
                else
                    context.ContestParticipantVotes.AddObject(new ContestParticipantVote { ContestparticipantID = ContestparticipantID, CreatedOn = DateTime.Now, UserID = UserID, VoteUP = 1, VoteDown = null });
                context.SaveChanges();
                var participant = context.ContestParticipants.Where(m => m.ContestID == ContestID && m.ContestparticipantID == ContestparticipantID).FirstOrDefault();
                participant.Votes = vote.Sum(m => m.VoteUP).Value - vote.Sum(m => m.VoteDown).Value;
                context.SaveChanges();
                CalculateContestVotes(ContestID);
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Vote up successfuly." };
            }
        }

        private static void CalculateContestVotes(int ID)
        {
            using (var context = new CentroEntities())
            {
                var contest = context.Contests.Where(m => m.ContestID == ID).FirstOrDefault();
                var participantsvotes = context.ContestParticipants.Join(context.ContestParticipantVotes,
                                                                        p => p.ContestparticipantID,
                                                                        v => v.ContestparticipantID,
                                                                        (p, v) => new { p, v })
                                                                  .Where(m => m.p.ContestID == ID)
                                                                  .Select(m => m.v)
                                                                  .ToList();
                contest.Votes = participantsvotes.Sum(m => m.VoteUP);
                context.SaveChanges();
            }
        }

        public static ActionOutput ParticipantVoteDown(int UserID, int ContestparticipantID, int ContestID)
        {
            using (var context = new CentroEntities())
            {
                var contest = context.ContestParticipants.Where(m => m.ContestID == ContestID & m.ContestparticipantID == ContestparticipantID).FirstOrDefault();
                if (contest.UserID == UserID)
                    return new ActionOutput { Status = ActionStatus.Error, Message = "You can't vote down your own entry" };

                var existing = context.ContestParticipantVotes.Where(m => m.UserID == UserID && m.ContestparticipantID == ContestparticipantID && m.VoteDown > 0).FirstOrDefault();
                if (existing != null)
                    return new ActionOutput { Status = ActionStatus.Error, Message = "You already voted down this entry" };

                var vote = context.ContestParticipantVotes.Where(m => m.ContestparticipantID == ContestparticipantID).ToList();

                var myvote = vote.Where(m => m.UserID == UserID).FirstOrDefault();
                if (myvote != null)
                { myvote.VoteDown = 1; myvote.VoteUP = null; }
                else
                    context.ContestParticipantVotes.AddObject(new ContestParticipantVote { ContestparticipantID = ContestparticipantID, CreatedOn = DateTime.Now, UserID = UserID, VoteUP = null, VoteDown = 1 });
                context.SaveChanges();

                var participant = context.ContestParticipants.Where(m => m.ContestID == ContestID && m.ContestparticipantID == ContestparticipantID).FirstOrDefault();
                participant.Votes = vote.Sum(m => m.VoteUP).Value - vote.Sum(m => m.VoteDown).Value;
                context.SaveChanges();
                CalculateContestVotes(ContestID);
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Vote down successfuly." };
            }
        }

        public static ActionOutput<ContestParticipantsViewModel> GetContestParticipant(int id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<ContestParticipantsViewModel>
                {
                    Status = ActionStatus.Successfull,
                    Object = context.ContestParticipants.Where(m => m.ContestparticipantID == id)
                                  .Select(m => new ContestParticipantsViewModel
                                  {
                                      ContestParticipant = m,
                                      ProductImage = m.Product.PrimaryPicture,
                                      Username = m.Product.Manufacturer,
                                      ProductTitle = m.Product.Title
                                  }).FirstOrDefault()
                };
            }
        }

        public static ActionOutput SaveComment(ContestComment obj)
        {
            using (var context = new CentroEntities())
            {
                obj.CreatedOn = DateTime.Now;
                context.ContestComments.AddObject(obj);
                context.SaveChanges();
                return new ActionOutput { ID = obj.ContestCommentID, Status = ActionStatus.Successfull, Results = new List<string> { obj.Comment.Replace("\r\n", "<br/>") } };
            }
        }

        public static ActionOutput<ContestCommentsViewModel> GetComments(int ContestParticipantID)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<ContestCommentsViewModel>
                {
                    Status = ActionStatus.Successfull,
                    List = context.ContestComments
                                  .Join(context.Shops,
                                            c => c.UserID,
                                            s => s.UserId,
                                            (c, s) => new { c, s })
                                  .Where(m => m.c.ContestParticipantID == ContestParticipantID)
                                  .Select(m => new ContestCommentsViewModel
                                  {
                                      Comment = m.c.Comment,
                                      ContestCommentID = m.c.ContestCommentID,
                                      ContestParticipantID = m.c.ContestParticipantID,
                                      CreatedOn = m.c.CreatedOn,
                                      ShopID = m.s.ShopID,
                                      ShopName = m.s.ShopName,
                                      UserID = m.c.UserID,
                                      Username = m.c.Username
                                  })
                                  .ToList()
                };
            }
        }

        public static void Winners()
        {
            using (var context = new CentroEntities())
            {
                var list = context.Contests.Include("ContestParticipants")
                                           .Where(m => m.IsActive && m.EndDate.Day == DateTime.Now.Day && m.EndDate.Month == DateTime.Now.Month && m.EndDate.Year == DateTime.Now.Year)
                                           .ToList();
                if (list != null && list.Count() > 0)
                {
                    List<ContestParticipant> winnerlist = new List<ContestParticipant>();
                    for (int i = 0; i < list.Count(); i++)
                    {
                        var winner = list[i].ContestParticipants.OrderByDescending(m => m.Votes).FirstOrDefault();
                        if (winner != null)
                        {
                            list[i].WinnerID = winner.UserID;
                            list[i].IsActive = false;
                            winnerlist.Add(winner);
                            context.AccountActivities.AddObject(new AccountActivity
                            {
                                ActivityID = list[i].ContestID,
                                ActivityType = (int)FollowType.Contest,
                                ActivityText = ActivityText.ContestWinner.Replace("{UserName}", list[i].Username)
                                                                         .Replace("{ContestTitle}", list[i].Title)
                                                                         .Replace("{Link}", "/Contest/Entries/" + list[i].ContestID)
                                                                         .Replace("{Date}", DateTime.Now.ToShortTimeString() + " " + DateTime.Now.ToLongDateString()),
                                UserID = list[i].UserID,
                                CreatedOn = DateTime.Now,
                            });

                        }
                    }
                    context.SaveChanges();

                    // Send Email to winners
                    foreach (var item in winnerlist)
                    {
                        string title = list.Where(m => m.ContestID == item.ContestID).FirstOrDefault().Title;
                        var user = UsersHandler.GetUserByID(item.UserID).Object;
                        EmailHandler.SendWinner(item, title, user, AppDomain.CurrentDomain.BaseDirectory);
                    }
                }
            }
        }

        public static ActionOutput<ContestParticipant> GetWinnerContestParticipant(int ContestID)
        {
            using (var context = new CentroEntities())
            {
                var w = context.ContestParticipants.Join(context.Contests,
                                                        cp => cp.ContestID,
                                                        c => c.ContestID,
                                                        (cp, c) => new { cp, c })
                                                 .Where(m => m.c.ContestID == ContestID && m.c.WinnerID.HasValue)
                                                 .Select(m => m.cp)
                                                 .FirstOrDefault();
                return new ActionOutput<ContestParticipant> { Status = ActionStatus.Successfull, Object = w };
            }
        }

        public static void SetPageView(int ContestID)
        {
            using (var context = new CentroEntities())
            {
                var counter = context.Contests.Where(m => m.ContestID == ContestID).FirstOrDefault();
                counter.TotalViews = counter.TotalViews.HasValue ? counter.TotalViews.Value + 1 : 1;
                context.SaveChanges();
            }
        }

        public static ActionOutput<Contest> GetContestByCategory(int CategoryID)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Contest>
                {
                    List = context.Contests.Include("Category").Include("ContestParticipants").Where(m => m.EndDate.Day >= DateTime.Now.Day && m.EndDate.Month >= DateTime.Now.Month && m.EndDate.Year >= DateTime.Now.Year && m.CategoryID == CategoryID).ToList(),
                    Status = ActionStatus.Successfull
                };
            }
        }


        public static ActionOutput GetTotalContestByUser(int user_id)
        {
            using (var context = new CentroEntities())
            {
                var result = context.Contests.Join(context.ContestParticipants,
                                                    c => c.ContestID,
                                                    p => p.ContestID,
                                                    (c, p) => new { c, p })
                                             .Where(m => m.p.UserID == user_id)
                                             .Select(m => m.c)
                                             .Count();
                return new ActionOutput
                {
                    Results = new List<string> { result.ToString(), result + (result <= 1 ? " Contest" : " Contests") }
                };
            }
        }


        public static PagingResult<ContestViewModel> GetContestListing(int page_no, int per_page_result, string sortOrder, string sortColumn, string search, int? CategoryID, int? UserID)
        {
            using (var context = new CentroEntities())
            {   
                //string q=query.ToTraceString();
                if (UserID.HasValue)
                {
                    List<ContestViewModel> list;
                    int total;
                    var query = context.ContestParticipants.Include("Contest").Where("1=1");//.Include("Category").Include("ContestParticipants")

                    if (CategoryID.HasValue && CategoryID.Value > 0)
                        query = query.Where("it.Contest.CategoryID =" + CategoryID.Value);
                    if (UserID.HasValue && UserID.Value > 0)
                        query = query.Where("it.UserID = " + UserID.Value);
                    if (!string.IsNullOrEmpty(search))
                        query = query.Where("it.Contest.Title like '" + search + "%' ");

                    query = query.OrderBy("it." + sortColumn + " " + sortOrder);

                    list = query.Skip(page_no - 1)
                                .Take(per_page_result)
                                .Select(m => new ContestViewModel
                                {
                                    ContestID = m.Contest.ContestID,
                                    CategoryID = m.Contest.CategoryID,
                                    UserID = m.UserID,
                                    Username = m.Contest.Username,
                                    Title = m.Contest.Title,
                                    Description = m.Contest.Description,
                                    ContestImage = m.Contest.ContestImage,
                                    TermsAndCondition = m.Contest.TermsAndCondition,
                                    Fund = m.Contest.Fund,
                                    StartDate = m.Contest.StartDate,
                                    EndDate = m.Contest.EndDate,
                                    CreatedOn = m.Contest.CreatedOn,
                                    TotalViews = m.Contest.TotalViews,
                                    WinnerID = m.Contest.WinnerID,
                                    IsActive = m.Contest.IsActive,
                                    CategoryName = m.Contest.Category.Name,
                                    TotalEntries = m.Contest.ContestParticipants.Count(),
                                    Votes = m.Votes
                                }).ToList();

                    total = query.Count();

                    PagingResult<ContestViewModel> pagingResult = new PagingResult<ContestViewModel>();
                    pagingResult.List = list;
                    pagingResult.TotalCount = total;
                    pagingResult.Status = ActionStatus.Successfull;
                    return pagingResult;
                }
                else
                {
                    List<ContestViewModel> list;
                    int total;
                    var query = context.Contests.Include("ContestParticipants").Where("1=1");//.Include("Category").Include("ContestParticipants")

                    if (CategoryID.HasValue && CategoryID.Value > 0)
                        query = query.Where("it.CategoryID =" + CategoryID.Value);
                    
                    if (!string.IsNullOrEmpty(search))
                        query = query.Where("it.Contest.Title like '" + search + "%' ");

                    query = query.OrderBy("it." + sortColumn + " " + sortOrder);
                    list = query.Skip(page_no - 1)
                                .Take(per_page_result)
                                .Select(m => new ContestViewModel
                                {
                                    ContestID = m.ContestID,
                                    CategoryID = m.CategoryID,
                                    UserID = m.UserID,
                                    Username = m.Username,
                                    Title = m.Title,
                                    Description = m.Description,
                                    ContestImage = m.ContestImage,
                                    TermsAndCondition = m.TermsAndCondition,
                                    Fund = m.Fund,
                                    StartDate = m.StartDate,
                                    EndDate = m.EndDate,
                                    CreatedOn = m.CreatedOn,
                                    TotalViews = m.TotalViews,
                                    WinnerID = m.WinnerID,
                                    IsActive = m.IsActive,
                                    CategoryName = m.Category.Name,
                                    TotalEntries = m.ContestParticipants.Count(),
                                    Votes = m.Votes
                                }).Distinct().ToList();

                    total = query.Select(m=>m.ContestID).Distinct().Count();

                    PagingResult<ContestViewModel> pagingResult = new PagingResult<ContestViewModel>();
                    pagingResult.List = list;
                    pagingResult.TotalCount = total;
                    pagingResult.Status = ActionStatus.Successfull;
                    return pagingResult;
                }
            }
        }



        public static ActionOutput<ContestViewModel> GetRandomContest(int item_count)
        {
            using (var context = new CentroEntities())
            {
                List<ContestViewModel> list = new List<ContestViewModel>();
                list = context.Contests.Include("Category").Include("ContestParticipants").OrderBy(m => Guid.NewGuid()).Take(item_count)
                                       .Select(m => new ContestViewModel
                                                {
                                                    ContestID = m.ContestID,
                                                    CategoryID = m.CategoryID,
                                                    UserID = m.UserID,
                                                    Username = m.Username,
                                                    Title = m.Title,
                                                    Description = m.Description,
                                                    ContestImage = m.ContestImage,
                                                    TermsAndCondition = m.TermsAndCondition,
                                                    Fund = m.Fund,
                                                    StartDate = m.StartDate,
                                                    EndDate = m.EndDate,
                                                    CreatedOn = m.CreatedOn,
                                                    TotalViews = m.TotalViews,
                                                    WinnerID = m.WinnerID,
                                                    IsActive = m.IsActive,
                                                    CategoryName = m.Category.Name,
                                                    TotalEntries = m.ContestParticipants.Count()
                                                })
                                       .ToList();

                return new ActionOutput<ContestViewModel>
                {
                    Status = ActionStatus.Successfull,
                    List = list
                };
            }
        }



        public static ActionOutput VoteUP(int ID, int UserID)
        {
            using (var context = new CentroEntities())
            {
                var contest = context.Contests.Where(m => m.ContestID == ID).FirstOrDefault();
                if (contest.UserID == UserID)
                    return new ActionOutput { Status = ActionStatus.Error, Message = "You can not vote up your own contest." };

                var existingvote = context.ContestVotes.Where(m => m.ContestID == ID && m.UserID == UserID && m.VoteUP.HasValue && !m.VoteDown.HasValue).FirstOrDefault();
                if (existingvote != null)
                    return new ActionOutput { Status = ActionStatus.Error, Message = "You already voted up this contest." };

                context.ContestVotes.AddObject(new ContestVote { ContestID = ID, CreatedOn = DateTime.Now, UserID = UserID, VoteUP = 1, VoteDown = null });
                context.SaveChanges();

                var votes = context.ContestVotes.Where(m => m.ContestID == ID).ToList();
                contest.Votes = votes.Sum(m => m.VoteUP).Value - votes.Sum(m => m.VoteDown).Value;
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Vote up successfuly." };
            }
        }

        public static ActionOutput<ContestParticipantVote> ContestParticipantVotes(int UserID)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<ContestParticipantVote>
                {
                    Status = ActionStatus.Successfull,
                    List = context.ContestParticipantVotes.Where(m => m.ContestparticipantID == UserID).ToList()
                };
            }
        }

        public static ActionOutput<ContestParticipant> MyVotedContestParticipants(int UserID)
        {
            using (var context = new CentroEntities())
            {
                var users = context.ContestParticipants.Join(context.ContestParticipantVotes,
                                                            cp => cp.ContestparticipantID,
                                                            cv => cv.ContestparticipantID,
                                                            (cp, cv) => new { cp, cv }).Where(m => m.cv.UserID == UserID).Select(m => m.cp).ToList();
                return new ActionOutput<ContestParticipant>
                {
                    Status = ActionStatus.Successfull,
                    List = users
                };
            }
        }

        public static void UpdateTotalViews(int ContestParticipantID, string IP, int LoggedInUserID)
        {
            using (var context = new CentroEntities())
            {

            }
        }

        public static ContestRequest AddContestRequest(ContestRequest model)
        {
            using (var context = new CentroEntities())
            {
                var item = new ContestRequest();
                item.Title = model.Title;
                item.Email = model.Email;
                item.Criteria = model.Criteria;
                item.Synosis = model.Synosis;
                item.IsAccepted = model.IsAccepted;
                item.Date = model.Date;

                context.ContestRequests.AddObject(item);
                context.SaveChanges();

                model.ContestRequestId = item.ContestRequestId;
            }
            // Sending Email to admin
            EmailHandler.SendContestRequest(model);
            return model;
        }

        public static ActionOutput DeactivateContest(List<int> ContestIds)
        {
            using (var context = new CentroEntities())
            {
                var contests = context.Contests.Where(m => ContestIds.Contains(m.ContestID)).ToList();
                foreach (var item in contests)
                {
                    item.IsActive = false;
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Selected contests has been deactivated." };
            }
        }

        public static ActionOutput ActivateContest(List<int> ContestIds)
        {
            using (var context = new CentroEntities())
            {
                var contests = context.Contests.Where(m => ContestIds.Contains(m.ContestID)).ToList();
                foreach (var item in contests)
                {
                    item.IsActive = true;
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Selected contests has been activated." };
            }
        }

        public static ActionOutput SetWinners(int ContestID, int First, int? Second, int? Third)
        {
            using (var context = new CentroEntities())
            {
                var contest = context.Contests.Where(m => m.ContestID == ContestID).FirstOrDefault();
                contest.WinnerID = First;
                contest.FirstRunnerUp = Second;
                contest.SecondRunnerUp = Third;
                context.SaveChanges();
                // Send Email to winners
                //foreach (var item in winnerlist)
                //{
                //    string title = list.Where(m => m.ContestID == item.ContestID).FirstOrDefault().Title;
                //    var user = UsersHandler.GetUserByID(item.UserID).Object;
                //    EmailHandler.SendWinner(item, title, user, AppDomain.CurrentDomain.BaseDirectory);
                //}
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Winners have been set." };
            }
        }
    }
}