using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Raven.Client;
using Raven.Client.Linq;
using TicketMuffin.Core.Actions;
using TicketMuffin.Core.Actions.ActivateEvent;
using TicketMuffin.Core.Actions.CancelEvent;
using TicketMuffin.Core.Actions.ExecutePayment;
using TicketMuffin.Core.Actions.RefundPledge;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Services;
using TicketMuffin.PayPal;
using TicketMuffin.PayPal.Model;
using TicketMuffin.Web.Areas.Admin.Models;
using TicketMuffin.Web.Controllers;
using TicketMuffin.Web.Models;

namespace TicketMuffin.Web.Areas.Admin.Controllers
{
    public class EventManagementController : Controller
    {
        private int pageSize = 100;
        private readonly IDocumentSession _documentSession;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IPledgeTicketSender _pledgeTicketSender;
        private ITicketGenerator _ticketGenerator;
        private IEventCultureResolver _cultureResolver;

        public EventManagementController(IDocumentSession documentSession, IPaymentGateway paymentGateway, IPledgeTicketSender pledgeTicketSender, ITicketGenerator ticketGenerator, IEventCultureResolver cultureResolver)
        {
            _documentSession = documentSession;
            _paymentGateway = paymentGateway;
            _pledgeTicketSender = pledgeTicketSender;
            _ticketGenerator = ticketGenerator;
            _cultureResolver = cultureResolver;
        }

        //
        // GET: /Admin/EventManagement/

        public ActionResult Index(int? page, string searchQuery, string[] state, string states, string orderBy, bool? descending)
        {
            if (!string.IsNullOrWhiteSpace(states))
            {
                state = states.Split(',');
            }

            descending = descending.HasValue && descending.Value;
            var eventListViewModel = new EventListViewModel();
            if (!page.HasValue || page.Value < 1) page = 1;
            var query = _documentSession.Advanced.LuceneQuery<GroupGivingEvent>("eventSearch");
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                //query = query.Where(e=>e.Title.StartsWith(searchQuery, StringComparison.OrdinalIgnoreCase));
                query = query.OpenSubclause()
                    .WhereContains("Title", searchQuery)
                    .OrElse().WhereContains("City", searchQuery)
                    .OrElse().WhereContains("Country", searchQuery)
                    .CloseSubclause();
            }
            if (state != null)
            {
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    query = query.AndAlso();
                    query = query.OpenSubclause();
                }


                for (int i = 0; i < state.Length; i++)
                {
                    if (i > 0)
                        query = query.OrElse();
                    query = query.WhereEquals("State", state[i]);
                }

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    query = query.CloseSubclause();
                }
            }

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                if (descending.Value)
                {
                    query = query.OrderBy("-"+orderBy);
                } else
                {
                    query = query.OrderBy(orderBy);
                }
            }
            else
            {
                query = query.OrderBy("-StartDate");
            }

            eventListViewModel.Events = query.Skip((page.Value - 1) * pageSize).Take(pageSize);
            eventListViewModel.Page = page.Value;
            eventListViewModel.PageSize = pageSize;
            eventListViewModel.States = state;
            eventListViewModel.SearchQuery = searchQuery;
            eventListViewModel.OrderBy = orderBy;
            eventListViewModel.Descending = descending.Value;

            // is there a next page?
            eventListViewModel.LastPage = _documentSession.Query<GroupGivingEvent>().Skip((page.Value) * pageSize).Count() == 0;

            if (eventListViewModel.States == null)
            {
                eventListViewModel.States = Enum.GetNames(typeof (EventState)).Where(s=>s != Enum.GetName(typeof(EventState), EventState.Deleted)).ToArray();
            }

            return View(eventListViewModel);
        }

        public ActionResult ManageEvent(int id)
        {
            var eventViewModel = new EventViewModel();

            AutoMapper.Mapper.CreateMap<GroupGivingEvent, EventViewModel>();
            var @event = _documentSession.Load<GroupGivingEvent>("groupgivingevents/" + id);
            eventViewModel = AutoMapper.Mapper.Map<EventViewModel>(@event);

            var transactionHistory = _documentSession.Query<TransactionHistoryEntry>("transactionHistory")
                .Where(e => e.EventId == @event.Id)
                .OrderByDescending(e => e.TimeStamp);
            eventViewModel.TransactionHistory = transactionHistory;

            return View(eventViewModel);
        }

        public ActionResult EditEventDetails(int id)
        {
            var eventViewModel = new UpdateEventViewModel();
            AutoMapper.Mapper.CreateMap<GroupGivingEvent, UpdateEventViewModel>();
            var @event = _documentSession.Load<GroupGivingEvent>("groupgivingevents/" + id);
            eventViewModel = AutoMapper.Mapper.Map<UpdateEventViewModel>(@event);
            if (@event.OrganiserId != null)
            {
                var eventOrganiser = _documentSession.Load<Account>(@event.OrganiserId);
                if (eventOrganiser != null)
                {
                    eventViewModel.EventOrganiser = eventOrganiser.Email;
                }
            }

            return View(eventViewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult EditEventDetails(int id, UpdateEventViewModel viewModel)
        {
            Account organiser = null;
            organiser = _documentSession.Query<Account>().Where(a => a.Email == viewModel.EventOrganiser).FirstOrDefault();

            if (organiser == null)
            {
                ModelState.AddModelError("EventOrganiser", "No account was found matching that email address");
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            AutoMapper.Mapper.CreateMap<UpdateEventViewModel, GroupGivingEvent>();

            var groupGivingEvent = _documentSession.Load<GroupGivingEvent>(id);
            this.TryUpdateModel(groupGivingEvent, "", null, new[] {"Id"});
            groupGivingEvent.OrganiserId = organiser.Id;
            groupGivingEvent.OrganiserName = organiser.FirstName + " " + organiser.LastName;
            groupGivingEvent.Currency = (int)viewModel.Currency;

            _documentSession.SaveChanges();
            return RedirectToAction("EditEventDetails", new { id });
        }
        public ActionResult ViewPledges(string id)
        {
            var viewModel = new EventViewModel();
            AutoMapper.Mapper.CreateMap<GroupGivingEvent, EventViewModel>();
            var @event = _documentSession.Load<GroupGivingEvent>("groupgivingevents/" + id);
            viewModel = AutoMapper.Mapper.Map<EventViewModel>(@event);

            return View(viewModel);
        }

        [ActionName("refund-pledge")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult RefundPledge(int id, string orderNumber)
        {
            RefundViewModel viewModel = new RefundViewModel();
            RefundPledgeAction action = new RefundPledgeAction(_paymentGateway);

            RefundResponse refundResult = null;
            try
            {
                refundResult = action.Execute(_documentSession, "groupgivingevents/" + id, orderNumber);
            } catch(Exception exception)
            {
                TempData["exception"] = exception;
                return RedirectToAction("ViewPledges", new { Id = id });
            } 

            if (refundResult.Successful)
            {
                TempData["refunded"] = true;
            }
            else
                TempData["refunded"] = false;

            return RedirectToAction("ViewPledges", new {Id = id});
        }

        [ActionName("execute-payment")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ExecutePayment(int id, string orderNumber)
        {
            ExecutePaymentViewModel viewModel = new ExecutePaymentViewModel();
            ExecutePaymentAction action = new ExecutePaymentAction(_paymentGateway);

            try
            {
                var response = action.Execute(_documentSession, "groupgivingevents/" + id, orderNumber);

                if (response.Successful)
                {
                    TempData["captured"] = true;
                } else
                {
                    TempData["captured"] = false;
                }
                    
            } catch (Exception exception)
            {
                    
                TempData["exception"] = exception;
            }
            return RedirectToAction("ViewPledges", new {id = id});
        }

        public ActionResult TransactionHistory(string id)
        {
            var @event = _documentSession.Query<GroupGivingEvent>()
                .FirstOrDefault(e => e.Pledges.Any(p=>p.OrderNumber.Equals(id, StringComparison.InvariantCulture)));
            if (@event==null)
            {
                ModelState.AddModelError("ordernumber", "We couldn't locate a pledge with that order number.");
            }

            var pledge = @event.Pledges.FirstOrDefault(p => p.OrderNumber == id);

            if (pledge == null)
            {
                ModelState.AddModelError("ordernumber", "We couldn't locate a pledge with that order number.");
            }

            var viewModel = new TransactionHistoryViewModel();
            if (pledge.PaymentGatewayHistory == null)
                pledge.PaymentGatewayHistory = new List<DialogueHistoryEntry>();
            viewModel.Messages = pledge.PaymentGatewayHistory.Where(h=>h != null).OrderByDescending(h => h.Timestamp).Take(1024).ToList();
            viewModel.EventId = int.Parse(@event.Id.Substring(@event.Id.IndexOf('/')+1));
            viewModel.OrderNumber = id;

            return View(viewModel);
        }

        public ActionResult DeleteEvent(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult DeleteEvent(int id, bool? confirmed)
        {
            if (!confirmed.HasValue || !confirmed.Value)
            {
                ModelState.AddModelError("confirmed", "We won't do this unless you confirm it");
            }

            CancelEventAction action = new CancelEventAction(_paymentGateway);
                try
                {
                    var cancelEventResponse = action.Execute(_documentSession, id);
                    if (cancelEventResponse.Success)
                    {
                        var @event = _documentSession.Load<GroupGivingEvent>(id);
                        @event.State = EventState.Deleted;

                        TempData["success"] = true;
                        return RedirectToAction("Index");
                    }
                } catch (InvalidOperationException exception)
                {
                    // when event is already completed or cancelled
                    var @event = _documentSession.Load<GroupGivingEvent>(id);
                    @event.State = EventState.Deleted;

                    TempData["success"] = true;
                    return RedirectToAction("Index");
                    
                }

                ModelState.AddModelError("confirmed", "There was some problems and the event could not be refunded/cancelled");
                return View();

        }

        public ActionResult Activate(int id)
        {
            var action = new ActivateEventAction(_paymentGateway, _ticketGenerator, _cultureResolver);
            action.Execute("groupgivingevents/" + id, _documentSession);

            return RedirectToAction("ManageEvent", new {id = id});
        }

        public ActionResult SendTicketsToAttendees(int id)
        {
            var @event = _documentSession.Load<GroupGivingEvent>(id);
            foreach(var pledge in @event.Pledges)
            {
                if (pledge.PaymentStatus == PaymentStatus.PaidPendingReconciliation
                    || pledge.PaymentStatus == PaymentStatus.Reconciled)
                {
                    _pledgeTicketSender.SendTickets(@event, pledge);
                }
            }

            return RedirectToAction("ManageEvent", "EventManagement", new {id = id});
        }
    }
}
