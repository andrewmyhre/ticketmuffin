using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core.Data;
using GroupGiving.Core.Data.Azure;
using GroupGiving.Core.Domain;
using GroupGiving.Web.Models;
using Microsoft.WindowsAzure;

namespace GroupGiving.Web.Controllers
{
    public class EventController : Controller
    {
        //
        // GET: /Event/
        CloudStorageAccount account = CloudConfiguration.GetStorageAccount("DataConnectionString");

        public ActionResult Index(Guid id)
        {
            var eventRepository = new AzureRepository<EventRow>(account);

            var viewModel = new EventViewModel();
            var givingEvent = eventRepository.Retrieve(id);
            if (givingEvent == null)
                return HttpNotFound();

            viewModel.EventId = givingEvent.Id;

            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create(string name, string city)
        {
            var eventRepository = new AzureRepository<EventRow>(account);
            var givingEvent = new EventRow(Guid.NewGuid(), "London");
            givingEvent.Name = "this is a test event";

            eventRepository.Save(givingEvent);
            return RedirectToAction("Index", new {id = givingEvent.Id});
        }
    }
}
