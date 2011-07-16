using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;
using GroupGiving.Core.Services;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Model;
using GroupGiving.Web.Code;
using GroupGiving.Web.Models;
using Ninject;
using RavenDBMembership.Provider;
using RavenDBMembership.Web.Models;

namespace GroupGiving.Web.Controllers
{
    public class OrderController : Controller
    {
        //
        // GET: /Order/
        private readonly IRepository<GroupGivingEvent> _eventRepository;
        private readonly IFormsAuthenticationService _formsService;
        private readonly IMembershipService _membershipService;
        private readonly IAccountService _accountService;

        public OrderController()
        {
            _eventRepository = MvcApplication.Kernel.Get<IRepository<GroupGivingEvent>>();
            _formsService = MvcApplication.Kernel.Get<IFormsAuthenticationService>();
            _membershipService = MvcApplication.Kernel.Get<AccountMembershipService>();
            _accountService = MvcApplication.Kernel.Get<IAccountService>();
            ((RavenDBMembershipProvider)Membership.Provider).DocumentStore
                = RavenDbDocumentStore.Instance;
        }

        public ActionResult PaymentRequest()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult StartRequest(PurchaseDetails purchaseDetails)
        {
            var viewModel = new OrderRequestViewModel();

            var eventDetails = _eventRepository.Retrieve(e => e.ShortUrl == purchaseDetails.ShortUrl);

            decimal amount = eventDetails.TicketPrice*purchaseDetails.Quantity;
            
            PayResponse response = SendPaymentRequest(amount);
            viewModel.PayPalPostUrl = ConfigurationManager.AppSettings["PayFlowProPaymentPage"];
            viewModel.Ack = response.ResponseEnvelope.Ack;
            viewModel.PayKey = response.PayKey;
            viewModel.Errors = response.Errors;

            if (response.Errors != null && response.Errors.Count() > 0)
                return View(viewModel);



            var account = _accountService.RetrieveByEmailAddress(purchaseDetails.EmailAddress);
            if (account==null)
            {
                IEmailRelayService emailRelayService = MvcApplication.Kernel.Get<IEmailRelayService>();
                _accountService.CreateIncompleteAccount(purchaseDetails.EmailAddress, emailRelayService);
            }

            // create an event pledge
            var pledge = new EventPledge();
            pledge.EmailAddress = purchaseDetails.EmailAddress;
            pledge.AmountPaid = amount;
            pledge.PayPalPayKey = response.PayKey;
            pledge.OrderNumber = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", "");

            // add the attendees
            pledge.Attendees = new List<EventPledgeAttendee>();
            foreach(var attendeeName in purchaseDetails.AttendeeName)
            {
                var attendee = new EventPledgeAttendee();
                attendee.FullName = attendeeName;
                pledge.Attendees.Add(attendee);
            }

            eventDetails.Pledges.Add(pledge);
            _eventRepository.SaveOrUpdate(eventDetails);
            _eventRepository.CommitUpdates();

            return Redirect(string.Format(ConfigurationManager.AppSettings["PayFlowProPaymentPage"], response.PayKey));
        }

        private PayResponse SendPaymentRequest(decimal amount)
        {
            IApiClient payPal = new ApiClient(new ApiClientSettings()
                                                  {
                                                      Username = "seller_1304843436_biz_api1.gmail.com",
                                                      Password = "1304843443",
                                                      Signature = "AFcWxV21C7fd0v3bYYYRCpSSRl31APG52hf-AmPfK7eyvf7LBc0.0sm7"
                                                  });
            decimal amountCommissionAdded = amount*1.05m;
            var request = new PayRequest();
            request.ActionType = "PAY";
            request.CurrencyCode = "GBP";
            request.FeesPayer = "EACHRECEIVER";
            request.Memo = "test order";
            request.CancelUrl = "http://" + Request.Url.Authority + "/Order/Cancel?payKey=${payKey}";
            request.ReturnUrl = "http://" + Request.Url.Authority + "/Order/Success?payKey=${payKey}";
            request.Receivers.Add(new Receiver(amountCommissionAdded.ToString("#.00"), "seller_1304843436_biz@gmail.com"));
            request.Receivers.Add(new Receiver(amount.ToString("#.00"), "sellr2_1304843519_biz@gmail.com"));
            return payPal.SendPayRequest(request);
        }

        public ActionResult Success(string payKey)
        {
            // update the pledge
            GroupGivingEvent @event = null;
            EventPledge pledge=null;
            using (var session = RavenDbDocumentStore.Instance.OpenSession())
            {
                @event =
                    session.Query<GroupGivingEvent>()
                        .Where(e => e.Pledges.Any(p => p.PayPalPayKey == payKey))
                        .FirstOrDefault();
                pledge = @event.Pledges.Where(p => p.PayPalPayKey == payKey).FirstOrDefault();
            }

            if (@event==null || pledge == null)
                return new HttpNotFoundResult();

            // user may just be reloading the page - fine, don't do any updates and present the view
            if (pledge.Paid)
            {
                pledge.Paid = true;
                pledge.DatePledged = DateTime.Now;
                _eventRepository.SaveOrUpdate(@event);
                _eventRepository.CommitUpdates();
            }

            var viewModel = new OrderConfirmationViewModel();
            viewModel.Event = @event;
            viewModel.PledgesRequired = viewModel.Event.MinimumParticipants -
                                        viewModel.Event.Pledges.Sum(p => p.Attendees.Count);
            viewModel.Pledge = pledge;

            return View(viewModel);
        }

        public ActionResult Cancel()
        {
            return View();
        }
    }
}
