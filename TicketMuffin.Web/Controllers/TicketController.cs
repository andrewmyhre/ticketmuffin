using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Raven.Client;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Services;

namespace TicketMuffin.Web.Controllers
{
    public class TicketController : Controller
    {
        private readonly ITicketGenerator _ticketGenerator;
        private readonly IDocumentSession _ravenDbSession;

        public TicketController(ITicketGenerator ticketGenerator, IDocumentSession ravenDbSession)
        {
            _ticketGenerator = ticketGenerator;
            _ravenDbSession = ravenDbSession;
        }

        //
        // GET: /Ticket/

        public ActionResult Index(string id, string culture = "en-GB")
        {
            // TODO: create an index to reduce these queries to one call
            var @event = _ravenDbSession.Query<GroupGivingEvent>()
                .SingleOrDefault(e => e.Pledges.Any(p => p.Attendees.Any(a=>a.TicketNumber==id)));
            if(@event==null)
            {
                throw new ArgumentException("Pledge not found");
            }
            var pledge = @event.Pledges.SingleOrDefault(p => p.Attendees.Any(a=>a.TicketNumber==id));
            var attendee = pledge.Attendees.Single(p => p.TicketNumber == id);

            var ticketFileStream = _ticketGenerator.LoadTicket(@event, pledge, attendee, culture);
            return new FileStreamResult(ticketFileStream, "application/pdf");
        }

    }
}
