using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core;
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

        public ActionResult Index(int? page, string searchQuery, string[] state, string orderBy, bool? descending)
        {
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
                eventListViewModel.States = Enum.GetNames(typeof (EventState)).ToArray();
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
            }

            return View(eventViewModel);
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
                var @event = session.Load<GroupGivingEvent>("groupgivingevents/" + id);
                var pledge = @event.Pledges.Where(p => p.OrderNumber == orderNumber).FirstOrDefault();
                RefundViewModel viewModel = new RefundViewModel();

                if (pledge == null)
                {
                    ModelState.AddModelError("ordernumber", "We couldn't locate a pledge with that order number.");
                }

                if (!ModelState.IsValid)
                {
                    viewModel.Event = @event;
                    viewModel.PledgeToBeRefunded = pledge;

                    return RedirectToAction("ViewPledges", new {Id=id});
                }

                RefundResponse refundResult = null;
                try
                {
                    refundResult = _paymentGateway.Refund(new RefundRequest()
                    {
                        TransactionId = pledge.TransactionId,
                        Receivers = new List<PaymentRecipient>()
                                            {
                                                new PaymentRecipient(pledge.AccountEmailAddress, pledge.Total, true)
                                            }
                    });
                } catch (HttpChannelException exception)
                {
                    if (pledge.PaymentGatewayHistory == null)
                        pledge.PaymentGatewayHistory = new List<DialogueHistoryEntry>();
                    pledge.PaymentGatewayHistory.Add(((ResponseBase)exception.FaultMessage).Raw);
                    session.SaveChanges();
                    throw;
                }

                if (pledge.PaymentGatewayHistory == null)
                    pledge.PaymentGatewayHistory = new List<DialogueHistoryEntry>();
                pledge.PaymentGatewayHistory.Add(refundResult.DialogueEntry);

                if (refundResult.Successful)
                {
                    pledge.DateRefunded = DateTime.Now;
                    pledge.PaymentStatus = PaymentStatus.Refunded;
                    pledge.Paid = false;
                    pledge.Notes = "REFUNDED";
                    TempData["refunded"] = true;
                } else
                {
                    pledge.Notes = refundResult.Message;
                }

                session.SaveChanges();

                viewModel.Event = @event;
                viewModel.PledgeToBeRefunded = pledge;
                viewModel.RefundFailed = true;

                return RedirectToAction("ViewPledges", new { Id = id });
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
    }

    public class TransactionHistoryViewModel
    {
        public List<DialogueHistoryEntry> Messages { get; set; }

        public int EventId { get; set; }

        public string OrderNumber { get; set; }
    }
}
