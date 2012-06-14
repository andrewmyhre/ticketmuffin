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

        public ActionResult Index(string id, string culture = "en-GB", int attendee=0)
        {
            var @event = _ravenDbSession.Query<GroupGivingEvent>()
                .SingleOrDefault(e => e.Pledges.Any(p => p.OrderNumber == id));
            if(@event==null)
            {
                throw new ArgumentException("Pledge not found");
            }
            var pledge = @event.Pledges.SingleOrDefault(p => p.OrderNumber == id);

            var ticketFileStream = _ticketGenerator.CreatePdf(@event, pledge, pledge.Attendees[attendee], culture);
            return new FileStreamResult(ticketFileStream, "application/pdf");
        }

    }
}
