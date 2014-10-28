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
    public class AdminHandler
    {
        public static ActionOutput<SiteFee> GetSiteFee()
        {
            using (var context = new CentroEntities())
            {
                return new ActionOutput<SiteFee>
                {
                    Status = ActionStatus.Successfull,
                    Object=context.SiteFees.FirstOrDefault()
                };
            }
        }

        public static ActionOutput SiteFeeAddOrUpdate(SiteFee obj)
        {
            using (var context = new CentroEntities())
            {
                var fee = context.SiteFees.FirstOrDefault();
                fee.SiteFee1 = obj.SiteFee1;
                context.SaveChanges();
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message="Site fee has been saved."
                };
            }
        }
    }
}
