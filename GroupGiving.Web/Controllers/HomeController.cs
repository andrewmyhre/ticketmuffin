using System;
using System.Web.Mvc;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using GroupGiving.Web.Code;
using GroupGiving.Web.Models;
using Ninject;
using Raven.Client;
using Raven.Client.Linq;

namespace GroupGiving.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<GroupGivingEvent> _eventRepository;
        private readonly IContentProvider _contentProvider;

        public HomeController(IRepository<GroupGivingEvent> eventRepository, IContentProvider contentProvider)
        {
            _eventRepository = eventRepository;
            _contentProvider = contentProvider;
        }

        public ActionResult Index()
        {
            var viewModel = new HomePageViewModel();
            viewModel.Events = _eventRepository
                .Query(e=>e.StartDate > DateTime.Now 
                && e.City.Contains("London")
                && e.IsFeatured
                && e.State == EventState.SalesReady);

            string pageAddress = ControllerContext.HttpContext.Request.Url.AbsolutePath;
            var pageContent = _contentProvider.GetPage(pageAddress);
            if (pageContent == null)
            {
                pageContent = _contentProvider.AddContentPage(pageAddress);
            }
            ViewData["pageContent"] = pageContent;

            return View(viewModel);
        }

        public ActionResult HowItWorks()
        {
            return View();
        }
    }
}
