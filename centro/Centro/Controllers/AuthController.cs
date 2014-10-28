using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Centro.Controllers
{
    public class AuthController : Controller
    {
        //
        // GET: /Auth/


        public ActionResult auth()
        {
            return View();
        }


        [HttpPost]
        public ActionResult auth(string username, string password)
        {
            if (username == "admin" && password == "admin@123")
            {
                FormsAuthentication.SetAuthCookie(username, true);
                return RedirectToAction("index", "home");
            }
            else return RedirectToAction("auth", "auth");
        }

    }
}
