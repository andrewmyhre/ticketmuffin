using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core.Data;
using GroupGiving.Core.Data.Azure;
using GroupGiving.Core.Domain;
using GroupGiving.Web.Models;
using Ninject;

namespace GroupGiving.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //var account = CloudConfiguration.GetStorageAccount("DataConnectionString");
            var eventRepository = MvcApplication.NinjectKernel.Get<IRepository<GroupGivingEvent>>();

            var viewModel = new HomePageViewModel();
            viewModel.Events = eventRepository.RetrieveAll();

            return View(viewModel);
        }
    }
}
