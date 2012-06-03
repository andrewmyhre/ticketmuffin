using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GroupGiving.Web.Code
{
    public abstract class ContentTypeAwareController : Controller
    {
        public ActionResult ContentTypeAwareView(object viewModel)
        {
            if (Request.AcceptTypes.Contains("application/json"))
                return Json(viewModel);

            return View(viewModel);
        }
    }
}