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
using System.Data.Objects;

namespace BusinessLayer.Handler
{
    public static class MessageHandler
    {
        public static ActionOutput AddAttachments(string username, List<ProductFileViewModel> files, int message_id)
        {
            using (var context = new CentroEntities())
            {
                foreach (ProductFileViewModel file in files.Where(p => p.ProductFileId == 0).ToList())
                {
                    Attachment p = new Attachment();
                    p.DisplayName = file.DisplayName;
                    p.MimeType = file.MimeType;
                    p.SavedName = file.SavedName;
                    p.SizeInBytes = file.SizeInBytes;
                    p.SizeInKB = file.SizeInKB;
                    p.SizeInMB = file.SizeInMB;
                    p.MessageID= message_id;
                    p.CreatedOn = DateTime.Now;
                    context.Attachments.AddObject(p);
                }
                context.SaveChanges();

                // Move temp files to user's folder
                foreach (ProductFileViewModel file in files)
                {
                    try
                    {
                        Utility.MoveFile("~/Temp/Attachments/" + username + "/" + file.SavedName, "~/Images/Attachments/" + username + "/", file.SavedName);
                    }
                    catch (Exception exc)
                    { }
                }
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Files have been saved." };
            }
        }

        public static ActionOutput PostMessage(Message message)
        {
            using (var context = new CentroEntities())
            {
                List<int> Receivers = context.Users.Where(m => message.Receivers.Contains(m.UserName)).Select(m => m.UserID).ToList();
                if (Receivers.Count == 0)
                    return new ActionOutput { ID = 0, Status = ActionStatus.Error, Message = "Invalid Recipients." };
                message.CreatedOn = DateTime.Now;                
                context.Messages.AddObject(message);
                context.SaveChanges();

                // Setting Receivers
                foreach (int id in Receivers)
                {
                    UsersMessage inbox = new UsersMessage();
                    inbox.MessageID = message.MessageID;
                    inbox.IsArchived = false;
                    inbox.IsRead = false;
                    inbox.PlaceHolderID = (int)MessagePlaceHolder.Inbox;
                    inbox.UserID = id;
                    context.UsersMessages.AddObject(inbox);
                }
                // Setting Sent for Sender
                UsersMessage sent = new UsersMessage();
                sent.MessageID = message.MessageID;
                sent.IsArchived = false;
                sent.IsRead = false;
                sent.PlaceHolderID = (int)MessagePlaceHolder.Sent;
                sent.UserID = message.AuthorID;
                context.UsersMessages.AddObject(sent);

                context.SaveChanges();
                return new ActionOutput { ID = message.MessageID, Status = ActionStatus.Successfull, Message = "Message has been sent." };
            }
        }

        public static PagingResult<MessagesListing_Result> GetMessageListing(int page_no, int per_page_result, string sortOrder, string sortColumn, string search, int user_id,int place_holder, bool? isRead, bool isArchived)
        {
            using (var context = new CentroEntities())
            {
                ObjectParameter output = new ObjectParameter("TotalCount", typeof(int));
                ObjectParameter TotalUnread = new ObjectParameter("TotalUnread", typeof(int));
                var list = context.MessagesListing(page_no, per_page_result, sortColumn, sortOrder, search, user_id, place_holder, isRead, isArchived, output, TotalUnread).ToList();
                PagingResult<MessagesListing_Result> pagingResult = new PagingResult<MessagesListing_Result>();
                pagingResult.List = list;
                pagingResult.TotalCount = Convert.ToInt32(output.Value);
                pagingResult.Status = ActionStatus.Successfull;
                pagingResult.Message = TotalUnread.Value.ToString();
                return pagingResult;
            }
        }

        public static ActionOutput<int> TotalUnreadMessage(int user_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<int>
                {
                    Status = ActionStatus.Successfull,
                    Object = context.UsersMessages.Where(m => m.UserID == user_id && m.PlaceHolderID == (int)MessagePlaceHolder.Inbox && !m.IsRead && !m.IsArchived)
                                                  .Count()
                };
            }
        }

        public static ActionOutput<MessageViewModel> MessageById(int message_id, int? reply_message_id)
        {
            using (var context = new CentroEntities())
            {
                List<MessageViewModel> list;
                if (reply_message_id.HasValue)
                {
                    list = context.Messages.Join(context.Users,
                                                m => m.AuthorID,
                                                u => u.UserID,
                                                (m, u) => new { m, u })
                                         .Where(m => m.m.ReplyMessageID == reply_message_id || m.m.MessageID == reply_message_id.Value)
                                         .Select(m => new MessageViewModel
                                         {
                                             CreatedOn = m.m.CreatedOn,
                                             Body = m.m.Body,
                                             MessageID = m.m.MessageID,
                                             SenderID = m.m.AuthorID,
                                             SenderUsername = m.u.UserName,
                                             Subject = m.m.Subject,
                                             ReplyMessageID = m.m.ReplyMessageID,                                             
                                             Attachments = m.m.Attachments.Where(a => a.MessageID == m.m.MessageID).Select(a => new ProductFileViewModel
                                             {
                                                 DisplayName = a.DisplayName,
                                                 MimeType = a.MimeType,
                                                 ProductFileId = a.AttachmentID,
                                                 SavedName = a.SavedName,
                                                 SizeInBytes = a.SizeInBytes,
                                                 SizeInKB = a.SizeInKB,
                                                 SizeInMB = a.SizeInMB
                                             }).AsQueryable()
                                         }).ToList();
                    
                }
                else
                {
                    list = context.Messages.Join(context.Users,
                                                m => m.AuthorID,
                                                u => u.UserID,
                                                (m, u) => new { m, u })
                                         .Where(m => m.m.MessageID== message_id || m.m.ReplyMessageID==message_id)
                                         .Select(m => new MessageViewModel
                                         {
                                             CreatedOn = m.m.CreatedOn,
                                             Body = m.m.Body,
                                             MessageID = m.m.MessageID,
                                             SenderID = m.m.AuthorID,
                                             SenderUsername = m.u.UserName,
                                             Subject = m.m.Subject,
                                             ReplyMessageID = m.m.ReplyMessageID,
                                             Attachments = m.m.Attachments.Where(a => a.MessageID == m.m.MessageID).Select(a => new ProductFileViewModel
                                             {
                                                 DisplayName = a.DisplayName,
                                                 MimeType = a.MimeType,
                                                 ProductFileId = a.AttachmentID,
                                                 SavedName = a.SavedName,
                                                 SizeInBytes = a.SizeInBytes,
                                                 SizeInKB = a.SizeInKB,
                                                 SizeInMB = a.SizeInMB
                                             }).AsQueryable()
                                         }).ToList();
                }
                
                return new ActionOutput<MessageViewModel> { List = list, Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput<UsersMessage> MessageReceiversByMessageReplyID(int message_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<UsersMessage>
                {
                    List = context.Messages.Join(context.UsersMessages,
                                                        m => m.ReplyMessageID,
                                                        r => r.MessageID,
                                                        (m, r) => new { m, r })
                                           .Where(m => m.m.MessageID == message_id)
                                           .Select(m => m.r)
                                           .ToList(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput<UsersMessage> MessageReceiversByMessageId(int message_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<UsersMessage>
                {
                    Status = ActionStatus.Successfull,
                    List = context.UsersMessages.Where(m => m.MessageID == message_id).ToList()
                };
            }
        }

        public static ActionOutput ReplyToAll(Message message, List<int> Receivers, int sender, int original_message_id=0)
        {
            using (var context = new CentroEntities())
            {
                message.CreatedOn = DateTime.Now;
                context.Messages.AddObject(message);
                context.SaveChanges();

                foreach (int id in Receivers.Where(m=> m!=sender).ToList())
                {
                    UsersMessage um = new UsersMessage();
                    um.IsArchived = false;
                    um.IsRead = false;
                    um.MessageID = message.MessageID;
                    um.PlaceHolderID = (int)MessagePlaceHolder.Inbox;
                    um.UserID = id;
                    context.UsersMessages.AddObject(um);
                }

                UsersMessage umm = new UsersMessage();
                umm.IsArchived = false;
                umm.IsRead = false;
                umm.MessageID = message.MessageID;
                umm.PlaceHolderID = (int)MessagePlaceHolder.Sent;
                umm.UserID = sender;
                context.UsersMessages.AddObject(umm);
                context.SaveChanges();
                return new ActionOutput { ID = message.MessageID, Status = ActionStatus.Successfull, Message = "Reply has been sent." };
            }
        }

        public static ActionOutput<Attachment> AttachmentById(int attachment_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Attachment>
                {
                    Object = context.Attachments.Where(m => m.AttachmentID == attachment_id).FirstOrDefault(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput MarkAsReadUnread(int user_id, int message_id, bool read)
        {
            using (var context = new CentroEntities())
            {
                var msg = context.UsersMessages.Where(m => m.MessageID == message_id && m.UserID==user_id).FirstOrDefault();
                msg.IsRead = read;
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput<UsersMessage> MessageReceiversByMessageID(int message_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<UsersMessage>
                {
                    List = context.Messages.Join(context.UsersMessages,
                                                        m => m.MessageID,
                                                        r => r.MessageID,
                                                        (m, r) => new { m, r })
                                           .Where(m => m.m.MessageID == message_id)
                                           .Select(m => m.r)
                                           .ToList(),
                    Status = ActionStatus.Successfull
                };
            }
        }

        public static ActionOutput<int?> ReplyMessageIdByMessageId(int message_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<int?>
                {
                    Status = ActionStatus.Successfull,
                    Object = context.Messages.Where(m => m.MessageID == message_id).FirstOrDefault().ReplyMessageID
                };
            }
        }

        public static ActionOutput<string> MessageAuthorByMessageId(int message_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<string>
                {
                    Status = ActionStatus.Successfull,
                    Object = context.Messages.Join(context.Users,
                                                    m => m.AuthorID,
                                                    u => u.UserID,
                                                    (m, u) => new { m, u })
                                            .Where(m => m.m.MessageID == message_id)
                                            .Select(m => m.u.UserName)
                                            .FirstOrDefault()
                };
            }
        }

        public static ActionOutput DeleteMessages(List<int> Ids, int user_id)
        {
            using (var context = new CentroEntities())
            {
                var usersmessages = context.UsersMessages.Where(m => Ids.Contains(m.MessageID) && m.UserID == user_id).ToList();
                foreach (var msg in usersmessages)
                {
                    context.DeleteObject(msg);
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput MarkAsArchive(List<int> Ids, int user_id)
        {
            using (var context = new CentroEntities())
            {
                var usersmessages = context.UsersMessages.Where(m => Ids.Contains(m.MessageID) && m.UserID == user_id).ToList();
                foreach (var msg in usersmessages)
                {
                    msg.PlaceHolderID=(int)MessagePlaceHolder.Archive;
                    msg.IsArchived = true;
                }
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull };
            }
        }

        public static ActionOutput<UsersMessage> UsersMessagesByMessageId(int id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<UsersMessage>
                {
                    Status = ActionStatus.Successfull,
                    List = context.UsersMessages.Where(m => m.MessageID == id).ToList()
                };
            }
        }

        public static ActionOutput setDocStatus(int ID, int Type, int user_id)
        {
            using (var context = new CentroEntities())
            {
                RequestAttachment attachment;
                if(Type==1) // client approved
                    attachment = context.RequestAttachments.Join(context.PrototypeRequests,
                                                                a => a.RequestId,
                                                                r => r.RequestID,
                                                                (a, r) => new { a, r })
                                                         .Where(m => m.a.AttachmentID == ID && (m.r.SellerId == user_id || m.r.BuyerId == user_id))
                                                         .Select(m => m.a).FirstOrDefault();
                else
                attachment= context.RequestAttachments.Join(context.PrototypeRequests,
                                                                a => a.RequestId,
                                                                r => r.RequestID,
                                                                (a, r) => new { a, r })
                                                         .Where(m => m.a.AttachmentID == ID && (m.r.SellerId == user_id || m.r.BuyerId == user_id))
                                                         .Select(m => m.a).FirstOrDefault();
                if (attachment != null)
                {
                    bool? old;
                    if (Type == 1)
                    {
                        old = attachment.IsClientApproved;
                        attachment.IsClientApproved = attachment.IsClientApproved.HasValue && attachment.IsClientApproved.Value ? false : true;
                    }
                    else
                    {
                        old = attachment.IsContractorApproved;
                        attachment.IsContractorApproved = attachment.IsContractorApproved.HasValue && attachment.IsContractorApproved.Value ? false : true;
                    }
                    context.SaveChanges();
                    return new ActionOutput { Status = ActionStatus.Successfull };
                }
                return new ActionOutput { Status = ActionStatus.Error, Message = "Unauthorize access." };
            }
        }

        public static ActionOutput DeleteContractDoc(int FileID, int UserID)
        {
            using (var context = new CentroEntities())
            {
                var doc = context.RequestAttachments.Where(m => m.AttachmentID == FileID).FirstOrDefault();
                if (doc!=null && (doc.IsClientApproved.HasValue && doc.IsClientApproved.Value) || (doc.IsContractorApproved.HasValue && doc.IsContractorApproved.Value))
                    return new ActionOutput { Status = ActionStatus.Error, Message = "Document can not be deleted because it either approved by Client or Contrator." };
                if(doc==null)
                    return new ActionOutput { Status = ActionStatus.Error, Message = "Document not found." };
                context.DeleteObject(doc);
                context.SaveChanges();
                return new ActionOutput { Status = ActionStatus.Successfull, Message = "Document has been deleted." };
            }
        }
    }
}