using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core;
using GroupGiving.Core.Actions.CancelEvent;
using GroupGiving.Core.Actions.ExecutePayment;
using GroupGiving.Core.Actions.RefundPledge;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Dto;
using GroupGiving.Core.Services;
using GroupGiving.Web.Controllers;
using GroupGiving.Web.Models;
using Raven.Client;
using Raven.Client.Linq;

namespace GroupGiving.Web.Areas.Admin.Controllers
{
    public class EventManagementController : Controller
    {
        private int pageSize = 100;
        private readonly IDocumentStore _documentStore;
        private readonly IPaymentGateway _paymentGateway;

        public EventManagementController(IDocumentStore documentStore, IPaymentGateway paymentGateway)
        {
            _documentStore = documentStore;
            _paymentGateway = paymentGateway;
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
            using (var session = _documentStore.OpenSession())
            {
                var query = session.Advanced.LuceneQuery<GroupGivingEvent>("eventSearch");
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
                eventListViewModel.LastPage = session.Query<GroupGivingEvent>().Skip((page.Value) * pageSize).Count() == 0;
            }

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
            using (var session = _documentStore.OpenSession())
            {
                var @event = session.Load<GroupGivingEvent>("groupgivingevents/" + id);
                eventViewModel = AutoMapper.Mapper.Map<EventViewModel>(@event);

                var transactionHistory = session.Query<TransactionHistoryEntry>("transactionHistory")
                    .Where(e => e.EventId == @event.Id)
                    .OrderByDescending(e => e.TimeStamp);
                eventViewModel.TransactionHistory = transactionHistory;
            }

            return View(eventViewModel);
        }

        public ActionResult EditEventDetails(int id)
        {
            var eventViewModel = new UpdateEventViewModel();
            AutoMapper.Mapper.CreateMap<GroupGivingEvent, UpdateEventViewModel>();
            using (var session = _documentStore.OpenSession())
            {
                var @event = session.Load<GroupGivingEvent>("groupgivingevents/" + id);
                eventViewModel = AutoMapper.Mapper.Map<UpdateEventViewModel>(@event);
                if (@event.OrganiserId != null)
                {
                    var eventOrganiser = session.Load<Account>(@event.OrganiserId);
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
            using (var session = _documentStore.OpenSession())
            {

                organiser = session.Query<Account>().Where(a => a.Email == viewModel.EventOrganiser).FirstOrDefault();

                if (organiser == null)
                {
                    ModelState.AddModelError("EventOrganiser", "No account was found matching that email address");
                }

                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }

                AutoMapper.Mapper.CreateMap<UpdateEventViewModel, GroupGivingEvent>();

                var groupGivingEvent = session.Load<GroupGivingEvent>(id);
                this.TryUpdateModel(groupGivingEvent, "", null, new[] {"Id"});
                groupGivingEvent.OrganiserId = organiser.Id;
                groupGivingEvent.OrganiserName = organiser.FirstName + " " + organiser.LastName;

                if (groupGivingEvent.SalesEndDateTime > DateTime.Now)
                    groupGivingEvent.State = EventState.SalesReady;
                else if (groupGivingEvent.StartDate > DateTime.Now)
                    groupGivingEvent.State = EventState.SalesClosed;
                else
                    groupGivingEvent.State = EventState.Completed;


                session.SaveChanges();
            }
            return RedirectToAction("EditEventDetails", new { id });
        }
        public ActionResult ViewPledges(string id)
        {
            var viewModel = new EventViewModel();
            AutoMapper.Mapper.CreateMap<GroupGivingEvent, EventViewModel>();
            using (var session = _documentStore.OpenSession())
            {
                var @event = session.Load<GroupGivingEvent>("groupgivingevents/" + id);
                viewModel = AutoMapper.Mapper.Map<EventViewModel>(@event);
            }

            return View(viewModel);
        }

        [ActionName("refund-pledge")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult RefundPledge(int id, string orderNumber)
        {
            using (var session = _documentStore.OpenSession())
            {
                RefundViewModel viewModel = new RefundViewModel();
                RefundPledgeAction action = new RefundPledgeAction(_paymentGateway);

                RefundResponse refundResult = null;
                try
                {
                    refundResult = action.Execute(session, "groupgivingevents/" + id, orderNumber);
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
        }

        [ActionName("execute-payment")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ExecutePayment(int id, string orderNumber)
        {
            using (var session = _documentStore.OpenSession())
            {
                ExecutePaymentViewModel viewModel = new ExecutePaymentViewModel();
                ExecutePaymentAction action = new ExecutePaymentAction(_paymentGateway);

                try
                {
                    var response = action.Execute(session, "groupgivingevents/" + id, orderNumber);

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
        }

        public ActionResult TransactionHistory(int id, string orderNumber)
        {
            using (var session = _documentStore.OpenSession())
            {
                var @event = session.Load<GroupGivingEvent>("groupgivingevents/" + id);
                var pledge = @event.Pledges.Where(p => p.OrderNumber == orderNumber).FirstOrDefault();

                if (pledge == null)
                {
                    ModelState.AddModelError("ordernumber", "We couldn't locate a pledge with that order number.");
                }

                var viewModel = new TransactionHistoryViewModel();
                if (pledge.PaymentGatewayHistory == null)
                    pledge.PaymentGatewayHistory = new List<DialogueHistoryEntry>();
                viewModel.Messages = pledge.PaymentGatewayHistory.Where(h=>h != null).OrderByDescending(h => h.Timestamp).Take(1024).ToList();
                viewModel.EventId = id;
                viewModel.OrderNumber = orderNumber;

                return View(viewModel);
            }
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
            using (var session = _documentStore.OpenSession())
            {
                try
                {
                    var cancelEventResponse = action.Execute(session, id);
                    if (cancelEventResponse.Success)
                    {
                        var @event = session.Load<GroupGivingEvent>(id);
                        @event.State = EventState.Deleted;
                        session.SaveChanges();

                        TempData["success"] = true;
                        return RedirectToAction("Index");
                    }
                } catch (InvalidOperationException exception)
                {
                    // when event is already completed or cancelled
                    var @event = session.Load<GroupGivingEvent>(id);
                    @event.State = EventState.Deleted;
                    session.SaveChanges();

                    TempData["success"] = true;
                    return RedirectToAction("Index");
                    
                }

                ModelState.AddModelError("confirmed", "There was some problems and the event could not be refunded/cancelled");
                return View();
            }

        }
    }

    public class TransactionHistoryEntry
    {
        public string EventId { get; set; }
        public string OrderNumber { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string AccountEmailAddress { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
    }


    public class ExecutePaymentViewModel
    {
    }

    public class TransactionHistoryViewModel
    {
        public List<DialogueHistoryEntry> Messages { get; set; }

        public int EventId { get; set; }

        public string OrderNumber { get; set; }
    }
}
