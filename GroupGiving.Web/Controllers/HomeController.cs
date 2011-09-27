using System;
using System.Web.Mvc;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using GroupGiving.Web.Code;
using GroupGiving.Web.Models;
using Ninject;
using Raven.Client.Linq;

namespace GroupGiving.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<GroupGivingEvent> _eventRepository;

        public HomeController(IRepository<GroupGivingEvent> eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public ActionResult Index()
        {
            var viewModel = new HomePageViewModel();
            viewModel.Events = _eventRepository
                .Query(e=>e.StartDate > DateTime.Now 
                && e.City.Contains("London")
                && e.IsFeatured
                && e.State == EventState.SalesReady);

            return View(viewModel);
        }

        public ActionResult HowItWorks()
        {
            return View();
        }
    }
}
