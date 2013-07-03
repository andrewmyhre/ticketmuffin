using System.Dynamic;
using System.Linq;
using System.Web.Mvc;
using Raven.Client;
using TicketMuffin.Core.Actions;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Services;
using TicketMuffin.Web.Models;

namespace TicketMuffin.Web.Areas.Admin.Controllers
{
    public class PledgeController : Controller
    {
        private readonly IDocumentSession _documentSession;
        private readonly IPledgeTicketSender _pledgeTicketSender;
        private ITicketGenerator _ticketGenerator;
        private IEventCultureResolver _cultureResolver;

        public PledgeController(IDocumentSession documentSession, IPledgeTicketSender pledgeTicketSender, ITicketGenerator ticketGenerator, IEventCultureResolver cultureResolver)
        {
            _documentSession = documentSession;
            _pledgeTicketSender = pledgeTicketSender;
            _ticketGenerator = ticketGenerator;
            _cultureResolver = cultureResolver;
        }

        //
        // GET: /Admin/Pledge/

        public ActionResult Search(string q)
        {
            var query = _documentSession.Advanced.LuceneQuery<TransactionHistoryItem>("pledges");
                if (!string.IsNullOrWhiteSpace(q))
                {
                    query = query.OpenSubclause()
                        .WhereStartsWith("TransactionId", q)
                        .OrElse().WhereStartsWith("OrderNumber", q)
                        .OrElse().WhereStartsWith("AccountEmailAddress", q).Fuzzy(0.5m)
                        .OrElse().WhereStartsWith("AttendeeName", q).Fuzzy(0.8m)
                        .OrElse().WhereStartsWith("EventName", q).Fuzzy(0.8m)
                        .OrElse().WhereStartsWith("EventOrganiser", q).Fuzzy(0.8m)
                        .CloseSubclause();
                }

                dynamic viewModel = new ExpandoObject();
                viewModel.Pledges = query.ToList();

                return View(viewModel);
        }
        public ActionResult Attendees(string id)
        {
            var @event = _documentSession.Query<GroupGivingEvent>()
                .SingleOrDefault(e => e.Pledges.Any(p => p.OrderNumber == id));
            var pledge = @event.Pledges.SingleOrDefault(p => p.OrderNumber == id);

            var viewModel = new AttendeesViewModel();
            viewModel.Pledge = pledge;
            viewModel.Event = @event;

            return View(viewModel);
        }

        public ActionResult SendTickets(string id)
        {
            var @event = _documentSession.Query<GroupGivingEvent>()
                            .SingleOrDefault(e => e.Pledges.Any(p => p.OrderNumber == id));
            var pledge = @event.Pledges.SingleOrDefault(p => p.OrderNumber == id);

            _pledgeTicketSender.SendTickets(@event, pledge);
            return RedirectToAction("Attendees", "Pledge", new {id = pledge.OrderNumber});
        }

        public ActionResult CreateTickets(string id)
        {
            var @event = _documentSession.Query<GroupGivingEvent>()
                            .SingleOrDefault(e => e.Pledges.Any(p => p.OrderNumber == id));
            var pledge = @event.Pledges.SingleOrDefault(p => p.OrderNumber == id);

            foreach(var attendee in pledge.Attendees)
            {
                _ticketGenerator.CreateTicket(@event, pledge, attendee, _cultureResolver.ResolveCulture(@event));
            }

            return RedirectToAction("Attendees", new {id=pledge.OrderNumber});
        }
    }

    public class AttendeesViewModel
    {
        public EventPledge Pledge { get; set; }

        public GroupGivingEvent Event { get; set; }
    }
}
