using System.Web.Mvc;
using TicketMuffin.Web.Code;

namespace TicketMuffin.Web.Controllers
{
    public class MarkdownController : Controller
    {
        //
        // GET: /Markdown/
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(string data)
        {
            MarkdownViewModel viewModel = new MarkdownViewModel();
            viewModel.RawData = data;
            viewModel.ParsedData = new Markdown().Transform(data);
            return View(viewModel);
        }

        public ActionResult Sample()
        {
            return View();
        }

    }
}
