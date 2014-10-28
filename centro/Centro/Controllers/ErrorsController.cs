using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Centro.Controllers
{
    public class ErrorsController : FrontEndBaseController
    {
        public ActionResult Index(string id)
        {
            ViewBag.CentroUsers = SiteUserDetails.LoggedInUser != null ? SiteUserDetails : null;
            ViewBag.ErrorCode = id;
            return View();
        }

    }
}
