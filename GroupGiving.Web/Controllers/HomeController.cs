using System.Web.Mvc;
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
        private readonly IEventService _eventService;

        public HomeController(IEventService eventService)
        {
            _eventService = eventService;
        }

        public ActionResult Index()
        {
            var viewModel = new HomePageViewModel();
            viewModel.Events = _eventService.RetrieveAllEvents();

            return View(viewModel);
        }

        public ActionResult HowItWorks()
        {
            return View();
        }
    }
}
