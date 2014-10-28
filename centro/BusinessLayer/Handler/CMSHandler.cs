using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Classes;
using System.Data.Objects;
using System.Web;
using System.Threading;

namespace BusinessLayer.Handler
{
    public class CMSHandler
    {
        public static List<CM> Pages()
        {
            using (var context = new CentroEntities())
            {
                return context.CMS.ToList();
            }
        }

        public static CM Page(int id)
        {
            using (var context = new CentroEntities())
            {
                return context.CMS.Where(m => m.CmsID == id).FirstOrDefault();
            }
        }

        public static CM Page(string page)
        {
            using (var context = new CentroEntities())
            {
                return context.CMS.Where(m => m.Page.ToLower() == page.Trim().ToLower()).FirstOrDefault();
            }
        }

        public static ActionOutput SavePageContent(CM model)
        {
            using (var context = new CentroEntities())
            {
                var page = context.CMS.Where(m => m.CmsID == model.CmsID).FirstOrDefault();
                page.PageContent = model.PageContent;
                context.SaveChanges();
                return new ActionOutput { Message = "Content has been modified.", Status = ActionStatus.Successfull };
            }
        }
    }
}
