using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;

namespace GroupGiving.Web.Areas.Api.Controllers
{
    public class EventsController : ApiControllerBase
    {
        //
        // GET: /Api/Event/

        public EventsController(IRepository<GroupGivingEvent> eventRepository) : base(eventRepository)
        {
        }

        public ActionResult Index(string shortUrl)
        {
            var @event = _eventRepository.Retrieve(e => e.ShortUrl == shortUrl);

            if (Request.AcceptTypes.Contains("application/json"))
            {
                return Json(@event, JsonRequestBehavior.AllowGet);
            } 

            return Xml(@event);
        }

    }
}
