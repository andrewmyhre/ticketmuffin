﻿using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using GroupGiving.Core.Actions.SettlePledge;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;
using GroupGiving.Core.Services;
using GroupGiving.PayPal;
using GroupGiving.Web.Models;
using Raven.Client;
using RavenDBMembership.Provider;
using RavenDBMembership.Web.Models;

namespace GroupGiving.Web.Controllers
{
    public class OrderController : Controller
    {
        //
        // GET: /Order/
        private readonly IFormsAuthenticationService _formsService;
        private readonly IMembershipService _membershipService;
        private readonly IAccountService _accountService;
        private readonly IPaymentGateway _paymentGateway;
        private readonly ITaxAmountResolver _taxResolver;
        private readonly ISiteConfiguration _siteConfiguration;
        private readonly IDocumentSession _documentSession;
        private IEmailRelayService _emailRelayService;

        public OrderController(IFormsAuthenticationService formsService, IMembershipService membershipService, 
            IAccountService accountService, IPaymentGateway paymentGateway, ITaxAmountResolver taxResolver, 
            ISiteConfiguration siteConfiguration, IDocumentSession documentSession, IEmailRelayService emailRelayService)
        {
            _formsService = formsService;
            _membershipService = membershipService;
            _accountService = accountService;
            _paymentGateway = paymentGateway;
            _taxResolver = taxResolver;
            _siteConfiguration = siteConfiguration;
            _documentSession = documentSession;
            ((RavenDBMembershipProvider)Membership.Provider).DocumentStore
                = documentSession.Advanced.DocumentStore;
            _emailRelayService = emailRelayService;
        }

        public ActionResult PaymentRequest()
        {
            return View();
        }



        public ActionResult Success(string payKey)
        {
            // update the pledge
            GroupGivingEvent @event = null;
            EventPledge pledge = null;
            Account account = null;
            @event =
                _documentSession.Query<GroupGivingEvent>()
                    .SingleOrDefault(e => e.Pledges.Any(p => p.TransactionId == payKey));
            if (@event == null)
                return new HttpNotFoundResult();
            pledge = @event.Pledges.Where(p => p.TransactionId == payKey).FirstOrDefault();

            if (pledge == null)
                return new HttpNotFoundResult();

            // user may just be reloading the page - fine, don't do any updates and present the view
            if (!pledge.Paid && pledge.PaymentStatus == PaymentStatus.Unpaid)
            {
                ConfirmPledgePaymentAction action
                    = new ConfirmPledgePaymentAction(_paymentGateway, _accountService, _emailRelayService);

                var paymentConfirmationResult = action.ConfirmPayment(@event,
                                                                      new SettlePledgeRequest() {PayPalPayKey = payKey});

                // send a purchase confirmation email
                MvcApplication.EmailFacade.Send(pledge.AccountEmailAddress,
                                                "PledgeConfirmation",
                                                new
                                                    {
                                                        Event = @event,
                                                        Pledge = pledge,
                                                        Account = account,
                                                        AccountPageUrl = "http://www.ticketmuffin.com/YourAccount"
                                                    }, "pl");

                // this pledge has activated the event);
                if (@event.IsOn
                    && (@event.PaidAttendeeCount - pledge.Attendees.Count < @event.MinimumParticipants))
                {
                    foreach (var eventPledge in @event.Pledges)
                    {
                        MvcApplication.EmailFacade.Send(
                            eventPledge.AccountEmailAddress,
                            "EventActivated",
                            new {Event = @event, Pledge = pledge}, "pl");
                    }
                }

                _documentSession.SaveChanges();
            }

            @event = _documentSession.Load<GroupGivingEvent>(@event.Id);
            pledge = @event.Pledges.Where(p => p.TransactionId == payKey).FirstOrDefault();
            var viewModel = new OrderConfirmationViewModel();
            viewModel.Event = @event;
            viewModel.PledgesRequired = viewModel.Event.MinimumParticipants -
                                        viewModel.Event.PaidAttendeeCount;
            viewModel.Pledge = pledge;

            return View(viewModel);
        }

        public ActionResult Cancel()
        {
            return View();
        }
    }
}
