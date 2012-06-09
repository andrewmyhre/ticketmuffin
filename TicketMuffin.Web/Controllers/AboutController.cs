using System.Web.Mvc;

namespace TicketMuffin.Web.Controllers
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
