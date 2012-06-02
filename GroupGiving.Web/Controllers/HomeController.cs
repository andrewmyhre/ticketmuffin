using System;
using System.Web.Mvc;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using GroupGiving.Web.Models;
using Ninject;
using Raven.Client;
using Raven.Client.Linq;

namespace GroupGiving.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDocumentSession _ravenSession;

        public HomeController(IDocumentSession ravenSession)
        {
            _ravenSession = ravenSession;
        }

        public ActionResult Index()
        {
            var viewModel = new HomePageViewModel();
            viewModel.Events = _ravenSession
                .Query<GroupGivingEvent>().Where(e=>e.StartDate > DateTime.Now 
                && e.IsFeatured
                && (e.State == EventState.SalesReady || e.State == EventState.Activated));

            return View(viewModel);
        }

        public ActionResult HowItWorks()
        {
            return View();
        }
    }
}
