﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using EmailProcessing;
using GroupGiving.Core.Actions.CreatePledge;
using GroupGiving.Core.Actions.SettlePledge;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Email;
using GroupGiving.Core.Services;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Model;
using GroupGiving.Web.Code;
using GroupGiving.Web.Models;
using Ninject;
using Raven.Client;
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
        private readonly IPaymentGateway _paymentGateway;
        private readonly ITaxAmountResolver _taxResolver;
        private readonly IPayPalConfiguration _paypalConfiguration;
        private IEmailRelayService _emailRelayService;

        public OrderController(IRepository<GroupGivingEvent> eventRepository, IFormsAuthenticationService formsService, IMembershipService membershipService, IAccountService accountService, IPaymentGateway paymentGateway, ITaxAmountResolver taxResolver, IPayPalConfiguration paypalConfiguration, IDocumentStore documentStore, IEmailRelayService emailRelayService)
        {
            _eventRepository = eventRepository;
            _formsService = formsService;
            _membershipService = membershipService;
            _accountService = accountService;
            _paymentGateway = paymentGateway;
            _taxResolver = taxResolver;
            _paypalConfiguration = paypalConfiguration;
            ((RavenDBMembershipProvider)Membership.Provider).DocumentStore
                = documentStore;
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
            EventPledge pledge=null;
            Account account = null;
            using (var session = RavenDbDocumentStore.Instance.OpenSession())
            {
                @event =
                    session.Query<GroupGivingEvent>()
                        .Where(e => e.Pledges.Any(p => p.TransactionId == payKey))
                        .FirstOrDefault();
                pledge = @event.Pledges.Where(p => p.TransactionId == payKey).FirstOrDefault();

                if (@event == null || pledge == null)
                    return new HttpNotFoundResult();

                // user may just be reloading the page - fine, don't do any updates and present the view
                if (!pledge.Paid && pledge.PaymentStatus == PaymentStatus.Unpaid)
                {
                    ConfirmPledgePaymentAction action
                        = new ConfirmPledgePaymentAction(_eventRepository, _paymentGateway, _accountService, _emailRelayService);

                    var paymentConfirmationResult = action.ConfirmPayment(new SettlePledgeRequest() {PayPalPayKey = payKey});

                    // send a purchase confirmation email
                    MvcApplication.EmailFacade.Send(pledge.AccountEmailAddress,
                                                    "PledgeConfirmation",
                                                    new {Event = @event, Pledge = pledge, Account = account});

                    // this pledge has activated the event);
                    if (@event.IsOn
                        && (@event.PledgeCount - pledge.Attendees.Count < @event.MinimumParticipants))
                    {
                        foreach (var eventPledge in @event.Pledges)
                        {
                            MvcApplication.EmailFacade.Send(
                                eventPledge.AccountEmailAddress,
                                "EventActivated",
                                new {Event = @event, Pledge = pledge});
                        }
                    }

                    session.SaveChanges();
                }
            }
            var viewModel = new OrderConfirmationViewModel();
            viewModel.Event = @event;
            viewModel.PledgesRequired = viewModel.Event.MinimumParticipants -
                                        viewModel.Event.PledgeCount;
            viewModel.Pledge = pledge;

            return View(viewModel);
        }

        public ActionResult Cancel()
        {
            return View();
        }
    }
}
