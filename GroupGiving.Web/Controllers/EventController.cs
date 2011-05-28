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
using Ninject;

namespace GroupGiving.Web.Controllers
{
    public class EventController : Controller
    {
        private readonly IRepository<GroupGivingEvent> _eventRepository;
        public EventController(IRepository<GroupGivingEvent> eventRepository)
        {
            _eventRepository = eventRepository;
        }

        //
        // GET: /Event/
        public ActionResult Index(Guid id)
        {
            var viewModel = new EventViewModel();
            var givingEvent = _eventRepository.Retrieve(id);
            if (givingEvent == null)
                return HttpNotFound();

            viewModel.EventId = givingEvent.Id;

            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create(string name, string city)
        {
            var givingEvent = new GroupGivingEvent()
            {
                Id=Guid.NewGuid(),
                Name="this is a test event",
                City="London"
            };

            _eventRepository.SaveOrUpdate(givingEvent);
            return RedirectToAction("Index", new {id = givingEvent.Id});
        }
    }
}
