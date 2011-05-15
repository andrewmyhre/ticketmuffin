using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core.Data.Azure;
using GroupGiving.Web.Models;

namespace GroupGiving.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var account = CloudConfiguration.GetStorageAccount("DataConnectionString");
            var eventRepository = new AzureRepository<EventRow>(account);

            var viewModel = new HomePageViewModel();
            viewModel.Events = eventRepository.All;

            return View(viewModel);
        }
    }
}
