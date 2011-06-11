using System;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Web.Models;
using System.Web.Mvc;

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
        public ActionResult Index(int id)
        {
            var viewModel = new EventViewModel();
            var givingEvent = _eventRepository.Retrieve(e=>e.Id=="groupgivingevents/"+id);
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
                Name="this is a test event",
                City="London"
            };

            _eventRepository.SaveOrUpdate(givingEvent);
            return RedirectToAction("Index", new {id = givingEvent.Id});
        }
    }
}
