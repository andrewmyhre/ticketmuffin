using System.Web.Mvc;

namespace TicketMuffin.Web.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {
        //
        // GET: /Admin/Home/

        public ActionResult Index()
        {
            return View();
        }

    }
}
