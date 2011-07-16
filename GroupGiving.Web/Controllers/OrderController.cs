using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using GroupGiving.Core.Actions.CreatePledge;
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
        private readonly IEmailCreationService _emailCreationService;
        private readonly IFormsAuthenticationService _formsService;
        private readonly IMembershipService _membershipService;
        private readonly IAccountService _accountService;
        private readonly IPaymentGateway _paymentGateway;
        private readonly ITaxAmountResolver _taxResolver;
        private readonly IPayPalConfiguration _paypalConfiguration;
        private readonly IEmailRelayService _emailRelayService;

        public OrderController()
        {
            _emailRelayService = MvcApplication.Kernel.Get<IEmailRelayService>();
            _emailCreationService = MvcApplication.Kernel.Get<IEmailCreationService>();
            _eventRepository = MvcApplication.Kernel.Get<IRepository<GroupGivingEvent>>();
            _formsService = MvcApplication.Kernel.Get<IFormsAuthenticationService>();
            _membershipService = MvcApplication.Kernel.Get<AccountMembershipService>();
            _accountService = MvcApplication.Kernel.Get<IAccountService>();
            _paymentGateway = MvcApplication.Kernel.Get<IPaymentGateway>();
            _taxResolver = MvcApplication.Kernel.Get<ITaxAmountResolver>();
            _paypalConfiguration = MvcApplication.Kernel.Get<IPayPalConfiguration>();
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
            var eventDetails = _eventRepository.Retrieve(e => e.ShortUrl == purchaseDetails.ShortUrl);
            var account = _accountService.RetrieveByEmailAddress(purchaseDetails.EmailAddress);

            if (account == null)
            {
                IEmailRelayService emailRelayService = MvcApplication.Kernel.Get<IEmailRelayService>();
                account = _accountService.CreateIncompleteAccount(purchaseDetails.EmailAddress, emailRelayService);
            }

            var action = new MakePledgeAction(_taxResolver, _eventRepository, _paymentGateway, _paypalConfiguration, _emailCreationService, (IEmailRelayService) ViewBag);
            var request = new MakePledgeRequest()
            {
                AttendeeNames = purchaseDetails.AttendeeName,
                PayPalEmailAddress = account.Email
            };

            var result = action.Attempt(eventDetails, account, request);

            var viewModel = new OrderRequestViewModel();

            PaymentGatewayResponse response = result.GatewayResponse;
            viewModel.PayPalPostUrl = response.PaymentPageUrl;
            viewModel.Ack = response.ResponseEnvelope.Ack;
            viewModel.PayKey = response.TransactionId;
            viewModel.Errors = response.Errors;

            if (!result.Succeeded)
                return View(viewModel);

            return Redirect(string.Format(ConfigurationManager.AppSettings["PayFlowProPaymentPage"], response.TransactionId));
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
                        .Where(e => e.Pledges.Any(p => p.TransactionId == payKey))
                        .FirstOrDefault();
                pledge = @event.Pledges.Where(p => p.TransactionId == payKey).FirstOrDefault();
            }

            if (@event==null || pledge == null)
                return new HttpNotFoundResult();

            // user may just be reloading the page - fine, don't do any updates and present the view
            if (!pledge.Paid)
            {
                pledge.Paid = true;
                pledge.DatePledged = DateTime.Now;

                // this pledge has activated the event
                if (@event.IsOn 
                    && (@event.PledgeCount - pledge.Attendees.Count < @event.MinimumParticipants))
                {
                    foreach(var eventPledge in @event.Pledges)
                    {
                        var email = _emailCreationService.MinimumNumberOfAttendeesReached(@event, eventPledge);
                        _emailRelayService.SendEmail(email);
                    }
                }

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
