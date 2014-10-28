using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Classes;

namespace BusinessLayer.Handler
{
    public static class InvoiceHandler
    {
        public static ActionOutput CreateInvoice(InvoiceViewModel model, string username)
        {
            using (var context = new CentroEntities())
            {
                Invoice invoice = new Invoice();
                invoice.BuyerID = model.BuyerID;
                invoice.CreatedOn = DateTime.Now;
                invoice.InvoiceAmount = model.InvoiceAmount;
                invoice.InvoiceNumber = model.InvoiceNumber;
                invoice.NoteForBuyer = model.NoteForBuyer;
                invoice.RequestID = model.RequestID;
                invoice.SellerID = model.SellerID;
                invoice.Status = (int)model.InvoiceStatus;
                invoice.TermsAndConditions = model.TermsAndCondition;
                invoice.Title = model.Title;
                context.Invoices.AddObject(invoice);
                context.SaveChanges();
                foreach (InvoiceItem item in model.InvoiceItems)
                {
                    item.InvoiceID = invoice.InvoiceID;
                    context.InvoiceItems.AddObject(item);
                }
                context.SaveChanges();

                // log alert into database
                AccountActivityHandler.SaveAlert(new Alert
                {
                    AlertForID = (int)model.RequestID,
                    AlertLink = "/Message/BuyerCustomOrder/" + model.RequestID,
                    AlertText = username + " has created an invoice \"" + invoice.Title + "\".",
                    UserID = model.BuyerID.Value
                });
                return new ActionOutput { ID = invoice.InvoiceID, Status = ActionStatus.Successfull, Message = "Invoice has been created." };
            }
        }

        public static ActionOutput<InvoiceViewModel> GetInvoiceByUserAndType(int user_id, MessagePlaceHolder type)
        {
            using (var context = new CentroEntities())
            {
                if (type == MessagePlaceHolder.Inbox)
                {
                    return new ActionOutput<InvoiceViewModel>
                    {
                        List = context.Invoices.Join(context.Users,
                                                        i => i.SellerID,
                                                        us => us.UserID,
                                                        (i, us) => new { i, us })
                                                .Where(i => i.i.BuyerID == user_id)
                                                .Select(i => new InvoiceViewModel
                                                {
                                                    BuyerID = user_id,
                                                    InvoiceAmount = i.i.InvoiceAmount,
                                                    InvoiceID = i.i.InvoiceID,
                                                    InvoiceNumber = i.i.InvoiceNumber,
                                                    InvoiceStatus = (InvoiceStatus)i.i.Status,
                                                    NoteForBuyer = i.i.NoteForBuyer,
                                                    RequestID = i.i.RequestID,
                                                    Seller = i.us.UserName,
                                                    SellerID = i.us.UserID,
                                                    TermsAndCondition = i.i.TermsAndConditions,
                                                    Title = i.i.Title,
                                                    CreatedOn = i.i.CreatedOn
                                                }).ToList(),
                        Status = ActionStatus.Successfull
                    };
                }
                else
                {
                    return new ActionOutput<InvoiceViewModel>
                    {
                        List = context.Invoices.Join(context.Users,
                                                        i => i.BuyerID,
                                                        us => us.UserID,
                                                        (i, us) => new { i, us })
                                                .Where(i => i.i.SellerID == user_id)
                                                .Select(i => new InvoiceViewModel
                                                {
                                                    Buyer = i.us.UserName,
                                                    BuyerID = user_id,
                                                    InvoiceAmount = i.i.InvoiceAmount,
                                                    InvoiceID = i.i.InvoiceID,
                                                    InvoiceNumber = i.i.InvoiceNumber,
                                                    InvoiceStatus = (InvoiceStatus)i.i.Status,
                                                    NoteForBuyer = i.i.NoteForBuyer,
                                                    RequestID = i.i.RequestID,
                                                    SellerID = user_id,
                                                    TermsAndCondition = i.i.TermsAndConditions,
                                                    Title = i.i.Title,
                                                    CreatedOn = i.i.CreatedOn
                                                }).ToList(),
                        Status = ActionStatus.Successfull
                    };
                }
            }
        }

        public static ActionOutput<InvoiceViewModel> GetInvoiceByUserAndType(long request_id, int user_id, MessagePlaceHolder type)
        {
            using (var context = new CentroEntities())
            {
                if (type == MessagePlaceHolder.Inbox)
                {
                    return new ActionOutput<InvoiceViewModel>
                    {
                        List = context.Invoices.Join(context.Users,
                                                        i => i.SellerID,
                                                        us => us.UserID,
                                                        (i, us) => new { i, us })
                                                .Where(i => i.i.BuyerID == user_id && i.i.RequestID == request_id)
                                                .Select(i => new InvoiceViewModel
                                                {
                                                    BuyerID = user_id,
                                                    InvoiceAmount = i.i.InvoiceAmount,
                                                    InvoiceID = i.i.InvoiceID,
                                                    InvoiceNumber = i.i.InvoiceNumber,
                                                    InvoiceStatus = (InvoiceStatus)i.i.Status,
                                                    NoteForBuyer = i.i.NoteForBuyer,
                                                    RequestID = i.i.RequestID,
                                                    Seller = i.us.UserName,
                                                    SellerID = i.us.UserID,
                                                    TermsAndCondition = i.i.TermsAndConditions,
                                                    Title = i.i.Title,
                                                    CreatedOn = i.i.CreatedOn
                                                }).ToList(),
                        Status = ActionStatus.Successfull
                    };
                }
                else
                {
                    return new ActionOutput<InvoiceViewModel>
                    {
                        List = context.Invoices.Join(context.Users,
                                                        i => i.BuyerID,
                                                        us => us.UserID,
                                                        (i, us) => new { i, us })
                                                .Where(i => i.i.SellerID == user_id && i.i.RequestID == request_id)
                                                .Select(i => new InvoiceViewModel
                                                {
                                                    Buyer = i.us.UserName,
                                                    BuyerID = user_id,
                                                    InvoiceAmount = i.i.InvoiceAmount,
                                                    InvoiceID = i.i.InvoiceID,
                                                    InvoiceNumber = i.i.InvoiceNumber,
                                                    InvoiceStatus = (InvoiceStatus)i.i.Status,
                                                    NoteForBuyer = i.i.NoteForBuyer,
                                                    RequestID = i.i.RequestID,
                                                    SellerID = user_id,
                                                    TermsAndCondition = i.i.TermsAndConditions,
                                                    Title = i.i.Title,
                                                    CreatedOn = i.i.CreatedOn
                                                }).ToList(),
                        Status = ActionStatus.Successfull
                    };
                }
            }
        }

        public static ActionOutput<InvoiceViewModel> InvoiceViewModelById(int id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<InvoiceViewModel>
                {
                    Status = ActionStatus.Successfull,
                    Object = context.Invoices
                                    .Where(i => i.InvoiceID == id)
                                    .Select(m => new InvoiceViewModel
                                    {
                                        CreatedOn = m.CreatedOn,
                                        InvoiceAmount = m.InvoiceAmount,
                                        InvoiceID = m.InvoiceID,
                                        //InvoiceItems = context.InvoiceItems.Where(ii => ii.InvoiceID == id).ToList(),
                                        InvoiceViewItems = m.InvoiceItems.AsEnumerable(),
                                        InvoiceNumber = m.InvoiceNumber,
                                        InvoiceStatus = (InvoiceStatus)m.Status,
                                        NoteForBuyer = m.NoteForBuyer,
                                        RequestID = m.RequestID,
                                        TermsAndCondition = m.TermsAndConditions,
                                        Title = m.Title,
                                        BuyerID = m.BuyerID,
                                        SellerID = m.SellerID
                                    }).FirstOrDefault()
                };
            }
        }

        public static ActionOutput<Invoice> InvoiceById(int id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<Invoice>
                {
                    Status = ActionStatus.Successfull,
                    Object = context.Invoices
                                    .Where(i => i.InvoiceID == id)
                                    .FirstOrDefault()
                };
            }
        }

        public static ActionOutput<InvoiceViewModel> GetInvoiceByRequestId(long request_id)
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<InvoiceViewModel>
                {
                    Status = ActionStatus.Successfull,
                    List = context.Invoices.Include("InvoiceID")
                                    .Where(i => i.RequestID == request_id)
                                    .Select(m => new InvoiceViewModel
                                    {
                                        CreatedOn = m.CreatedOn,
                                        InvoiceAmount = m.InvoiceAmount,
                                        InvoiceID = m.InvoiceID,
                                        //InvoiceItems = context.InvoiceItems.Where(ii => ii.InvoiceID == id).ToList(),
                                        InvoiceViewItems = m.InvoiceItems.AsEnumerable(),
                                        InvoiceNumber = m.InvoiceNumber,
                                        InvoiceStatus = (InvoiceStatus)m.Status,
                                        NoteForBuyer = m.NoteForBuyer,
                                        RequestID = m.RequestID,
                                        TermsAndCondition = m.TermsAndConditions,
                                        Title = m.Title,
                                        BuyerID = m.BuyerID,
                                        SellerID = m.SellerID,
                                        EscrowedOn = context.UserTransactions.Where(i => i.InvoiceID == i.InvoiceID).FirstOrDefault().EscrowedOn,
                                        ReleasedOn = context.UserTransactions.Where(i => i.InvoiceID == i.InvoiceID).FirstOrDefault().ReleasedOn
                                    }).ToList()
                };
            }
        }

        public static ActionOutput DeleteInvoice(int id, string username)
        {
            using (var context = new CentroEntities())
            {
                Invoice invoice_obj = context.Invoices.Where(i => i.InvoiceID == id && i.Status == (int)InvoiceStatus.Pending).FirstOrDefault();
                int res = 0;
                if (invoice_obj != null)
                {
                    context.Invoices.DeleteObject(invoice_obj);
                    res = context.SaveChanges();
                }
                if (res > 0)
                {
                    // log alert into database
                    AccountActivityHandler.SaveAlert(new Alert
                    {
                        AlertForID = (int)invoice_obj.RequestID,
                        AlertLink = "/Message/BuyerCustomOrder/" + invoice_obj.RequestID,
                        AlertText = username + " has deleted an invoice \"" + invoice_obj.Title + "\".",
                        UserID = invoice_obj.BuyerID.Value
                    });
                    return new ActionOutput { Status = ActionStatus.Successfull, Message = "Invoice has been deleted." };
                }
                else
                {
                    return new ActionOutput { Status = ActionStatus.Error, Message = "Invoice not deleted!Try again." };
                }
            }
        }
    }
}
