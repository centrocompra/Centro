using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Models.DataModel;
using BusinessLayer.Models.ViewModel;
using BusinessLayer.Classes;

namespace BusinessLayer.Handler
{
    public static class ReportsHandller
    {
        public static ActionOutput SaveReport(Report obj)
        {
            using (var context = new CentroEntities())
            {
                obj.IsDeleted = false;
                obj.IsRead = false;
                obj.CreatedOn = DateTime.Now;
                context.Reports.AddObject(obj);
                context.SaveChanges();
                return new ActionOutput
                {
                    Status = ActionStatus.Successfull,
                    Message = "Report saved successfully"
                };
            }
        }

        public static ActionOutput<ReportType> GetReportTypes()
        {
            List<ReportType> lstReportType = new List<ReportType>();
            lstReportType.Add(new ReportType { ReportTypeText = "Plagiarism", ReportTypeValue = "Plagiarism" });
            lstReportType.Add(new ReportType { ReportTypeText = "Inappropriate Material", ReportTypeValue = "Inappropriate Material" });
            lstReportType.Add(new ReportType { ReportTypeText = "Fraud", ReportTypeValue = "Fraud" });
            lstReportType.Add(new ReportType { ReportTypeText = "Other", ReportTypeValue = "Other”" });
            return new ActionOutput<ReportType>
            {
                Status = ActionStatus.Successfull,
                List = lstReportType
            };
        }


    }
}
