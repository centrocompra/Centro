using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessLayer.Classes;
using BusinessLayer.Models.ViewModel;
using System.IO;
using System.Web.Security;
using System.Web.Script.Serialization;
using BusinessLayer.Handler;
using BusinessLayer.Models.DataModel;

namespace Centro.Controllers
{
    ////[Authorize]
//    [NoCache]    
    public class BaseController : Controller
    {
        protected SiteUserDetails SiteUserDetails { get; set; }

        //protected 

        /** 
         * Purpose  :   executed whenever an exception is encountered
         * Inputs   :   filter_context – details of the current context
         */
        protected override void OnException(ExceptionContext filter_context)
        {
            filter_context.ExceptionHandled = true;
            var error_id = LogExceptionToDatabase(filter_context.Exception);//log exception in database

            var msg = string.Empty;
            if (filter_context.Exception.GetType() == typeof(HttpRequestValidationException)) msg = "HTML tags and/or Malicious Code are not allowed. Please remove from text box below to continue.";


            //redirect control to ErrorResultJson action method if the request is an ajax request
            if (Request.IsAjaxRequest()) filter_context.Result = Json(new ActionOutput
            {
                Status = ActionStatus.Error,
                Message = (string.IsNullOrWhiteSpace(msg) ? "Error : " + error_id : msg)
            }, JsonRequestBehavior.AllowGet);

            //This needs to be changed to redirect the control to an error page.
            else filter_context.Result = null;

            base.OnException(filter_context);
        }

        /** 
        * Purpose  :   logges the exception in the database
        * Inputs   :   ex – details of the exception
        */
        public String LogExceptionToDatabase(Exception ex)
        {
            String ex_text = String.Empty;
            String ex_message = ex.ToString();
            try
            {
                using (CentroEntities obj_entity = new CentroEntities())
                {
                    ErrorLog obj_errorlog = new ErrorLog();
                    obj_errorlog.Message = ex.Message;
                    obj_errorlog.StackTrace = ex.StackTrace;
                    obj_errorlog.InnerException = ex.InnerException == null ? "" : ex.InnerException.Message;
                    obj_errorlog.LoggedInDetails = GetLoggedInUserJson();
                    obj_errorlog.LoggedOn = DateTime.Now;

                    //== Retrieve all the data from the query string, form collection and the route data and save it to database in serialized form
                    var form_data = Request.Form.AllKeys.Select(o => new { Name = o, Value = Request.Form[o] }).ToList();
                    var query_data = Request.QueryString.AllKeys.Select(o => new { Name = o, Value = Request.QueryString[o] }).ToList();
                    var route_data = RouteData.Values.Keys.Select(o => new { Name = o, Value = RouteData.Values[o] }).ToList();

                    obj_errorlog.FormData = new JavaScriptSerializer().Serialize(form_data);
                    obj_errorlog.QueryData = new JavaScriptSerializer().Serialize(query_data);
                    obj_errorlog.RouteData = new JavaScriptSerializer().Serialize(route_data);
                    //=======================================================================

                    ex_text = new JavaScriptSerializer().Serialize(obj_errorlog);
                    obj_entity.ErrorLogs.AddObject(obj_errorlog);
                    obj_entity.SaveChanges();

                    return obj_errorlog.ErrorLogID.ToString();
                }
            }
            catch (Exception)
            {
                LogExceptionToFile(ex_text, ex_message);
                return "0";
            }
        }

        /// <summary>
        /// This Will be used to log exception to a file
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="ex_message"></param>
        public void LogExceptionToFile(String ex, String ex_message)
        {
            System.IO.StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(Server.MapPath("~/Logs/logs.txt"), true);
                sw.WriteLine("/************************************ " + DateTime.Now + " ***********************************************");
                sw.WriteLine(ex_message);
                sw.WriteLine("http://jsonformat.com/");
                sw.WriteLine(ex); sw.WriteLine(""); sw.WriteLine("");
            }
            catch { }
            finally { sw.Close(); }
        }

        /** 
        * Purpose  :   created a cookie for the loggedin user's username
        * Inputs   :   user_name – username of the loggedin user
        */
        public string RenderRazorViewToString(string view_name, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, view_name);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Convert Loggedin user object to json string
        /// </summary>
        protected virtual string GetLoggedInUserJson()
        {
            return "";
        }

        /// <summary>
        /// To check unique email
        /// </summary>
        [SkipAuthentication]
        public JsonResult UniqueEmail(string EmailId)
        {
            int? currentuserid = SiteUserDetails.LoggedInUser != null ? (int?)SiteUserDetails.LoggedInUser.Id : null;
            return Json(Utility.UniqueEmail(EmailId, currentuserid));
        }
        
        /// <summary>
        /// To check unique username
        /// </summary>
        [SkipAuthentication]
        public JsonResult UniqueUsername(string Username)
        {
            
            return Json(Utility.UniqueUsername(Username));
        }

        /// <summary>
        /// To check unique shopname
        /// </summary>
        [SkipAuthentication]
        public JsonResult UniqueShopname(string shopname)
        {
            int user_id=SiteUserDetails.LoggedInUser.Id;
            return Json(Utility.UniqueShopname(shopname, user_id));
        }

        /// <summary>
        /// To check unique hubname
        /// </summary>
        [HttpPost]
        public JsonResult UniqueHubName(string Title,int? HubID)
        {
            return Json(Utility.UniqueHubName(Title, SiteUserDetails.LoggedInUser.Id, HubID));
        }

        protected void CreateCustomCookie(String cookie_name, Boolean is_persistent, String custom_data, int min = 30)
        {
            HttpCookie cookie = new HttpCookie(cookie_name, custom_data);
            if (is_persistent) cookie.Expires = DateTime.Now.AddMinutes(30);
            else cookie.Expires = DateTime.Now.AddMinutes(min);
            System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
        }

        protected void UpdateCustomCookie(String cookie_name, Boolean is_persistent, String custom_data, int min = 30)
        {
            HttpCookie old = System.Web.HttpContext.Current.Request.Cookies[cookie_name];
            old.Expires = DateTime.Now.AddDays(-10);
            System.Web.HttpContext.Current.Response.Cookies.Add(old);

            HttpCookie cookie = new HttpCookie(cookie_name, custom_data);
            if (is_persistent) cookie.Expires = DateTime.Now.AddMinutes(30);
            else cookie.Expires = DateTime.Now.AddMinutes(min);
            System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
        }
    }
}
