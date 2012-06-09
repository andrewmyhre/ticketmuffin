using System;
using System.Web.Mvc;
using Raven.Client;
using Raven.Client.Linq;
using TicketMuffin.Core.Domain;
using TicketMuffin.Web.Models;

namespace TicketMuffin.Web.Controllers
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
