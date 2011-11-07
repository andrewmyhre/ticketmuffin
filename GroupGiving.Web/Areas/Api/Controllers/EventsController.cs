using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EmailProcessing;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using GroupGiving.Web.Areas.Api.Models;
using GroupGiving.Web.Code;
using GroupGiving.Web.Models;

namespace GroupGiving.Web.Areas.Api.Controllers
{
    public class EventsController : ApiControllerBase
    {
        private readonly IEmailPackageRelayer _emailRelayer;
        private readonly IEventService _eventService;
        //
        // GET: /Api/Event/

        public EventsController(IEmailPackageRelayer emailRelayer, IEventService eventService)
        {
            _emailRelayer = emailRelayer;
            _eventService = eventService;
        }

        public ActionResult Index(string shortUrl)
        {
            var @event = _eventService.Retrieve(shortUrl);

            if (@event==null)
                return new HttpNotFoundResult();

            return ApiResponse(@event);
        }

        [HttpPost]
        public ActionResult ShareViaEmail(string shortUrl, ShareViaEmailRequest request)
        {
            var response = new ApiResponse<EventModel>();
            if (!ModelState.IsValid)
            {
                response.Errors = ModelState.ToErrorResponse();
                return ApiResponse(response, HttpStatusCode.BadRequest);
            }

            var @event = _eventService.Retrieve(shortUrl);
            var eventModel = AutoMapper.Mapper.Map<GroupGivingEvent>(@event);

            _eventService.SendEventInvitationEmails(_emailRelayer, request.Recipients, request.Body, request.Subject);

            return ApiResponse(
                new ApiResponse<EventModel>() {
                    Link = new ResourceLink<EventModel>(){Href="http://www.ticketmuffin.com/"+eventModel.ShortUrl}},
                    HttpStatusCode.OK);
        }
    }
}
