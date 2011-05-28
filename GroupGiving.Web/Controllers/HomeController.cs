using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core.Data;
using GroupGiving.Core.Data.Azure;
using GroupGiving.Core.Domain;
using GroupGiving.Web.Models;
using Microsoft.WindowsAzure;
using Ninject;

namespace GroupGiving.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<GroupGivingEvent> _eventRepository;

        public HomeController()
        {
            _eventRepository = MvcApplication.Kernel.Get<IRepository<GroupGivingEvent>>();
        }

        public ActionResult Index()
        {
            var viewModel = new HomePageViewModel();
            viewModel.Events = _eventRepository.RetrieveAll();
            return View(viewModel);
        }
    }
}
