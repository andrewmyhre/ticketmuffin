using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GroupGiving.Web.Controllers
{
    public class AboutController : Controller
    {
        //
        // GET: /About/

        public ActionResult TermsAndConditions()
        {
            return View();
        }

        public ActionResult Privacy()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
    }
}
