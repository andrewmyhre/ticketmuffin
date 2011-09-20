using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Web.Areas.Api.Models;
using GroupGiving.Web.Models;

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

            return Response(@event);
        }

        [HttpPost]
        public ActionResult ShareViaEmail(string shortUrl, ShareViaEmailViewModel request)
        {
            var @event = _eventRepository.Retrieve(e => e.ShortUrl == shortUrl);
            var eventModel = AutoMapper.Mapper.Map<GroupGivingEvent>(@event);

            return Response(
                new ApiResponse<EventModel>() {
                    Link = new ResourceLink<EventModel>(){Href="http://www.ticketmuffin.com/"+eventModel.ShortUrl}},
                    HttpStatusCode.OK);
        }
    }
}
