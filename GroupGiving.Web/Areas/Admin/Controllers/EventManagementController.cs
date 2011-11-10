using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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

        public ActionResult Index(int? page, string searchQuery, string[] state)
        {
            var eventListViewModel = new EventListViewModel();
            if (!page.HasValue || page.Value < 1) page = 1;
            using (var session = _documentStore.OpenSession())
            {
                var query = session.Advanced.LuceneQuery<GroupGivingEvent>("eventSearch");
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    //query = query.Where(e=>e.Title.StartsWith(searchQuery, StringComparison.OrdinalIgnoreCase));
                    query = query.WhereContains("Title", searchQuery);
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

                eventListViewModel.Events = query.Skip((page.Value - 1) * pageSize).Take(pageSize);
                eventListViewModel.Page = page.Value;
                eventListViewModel.PageSize = pageSize;
                eventListViewModel.States = state;

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
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult RefundPledge(int id, string orderNumber)
        {
            using (var session = _documentStore.OpenSession())
            {
                var @event = session.Load<GroupGivingEvent>("groupgivingevents/" + id);
                var pledge = @event.Pledges.Where(p => p.OrderNumber == orderNumber).FirstOrDefault();
                RefundViewModel viewModel = null;

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

                var refundResult = _paymentGateway.Refund(new RefundRequest() { TransactionId = pledge.TransactionId });

                if (refundResult.Successful)
                {
                    pledge.DateRefunded = DateTime.Now;
                    pledge.PaymentStatus = PaymentStatus.Refunded;
                    pledge.Paid = false;
                    session.SaveChanges();
                    TempData["refunded"] = true;
                }

                viewModel.Event = @event;
                viewModel.PledgeToBeRefunded = pledge;
                viewModel.RefundFailed = true;

                return RedirectToAction("ViewPledges", new { Id = id });
            }
        }

        public ActionResult TransactionHistory(string id)
        {
            throw new NotImplementedException();
        }
    }
}
