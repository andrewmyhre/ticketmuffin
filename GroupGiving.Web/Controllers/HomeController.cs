using System.Web.Mvc;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using GroupGiving.Web.Code;
using GroupGiving.Web.Models;
using Ninject;

namespace GroupGiving.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEventService _eventService;

        public HomeController()
        {
            _eventService = MvcApplication.Kernel.Get<IEventService>();
        }

        public ActionResult Index()
        {
            var viewModel = new HomePageViewModel();
            viewModel.Events = _eventService.RetrieveAllEvents();
            return View(viewModel);
        }
    }
}
