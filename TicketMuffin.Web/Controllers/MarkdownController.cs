using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Web.Code;

namespace GroupGiving.Web.Controllers
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
            viewModel.ParsedData = new anrControls.Markdown().Transform(data);
            return View(viewModel);
        }

        public ActionResult Sample()
        {
            return View();
        }

    }
}
