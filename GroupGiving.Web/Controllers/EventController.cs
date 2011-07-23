﻿using System;
using System.Configuration;
using System.Linq;
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
using System.Web.Mvc;
using Ninject;
using RavenDBMembership.Provider;
using RavenDBMembership.Web.Models;

namespace GroupGiving.Web.Controllers
{
    public class EventController : Controller
    {
        private readonly IRepository<GroupGivingEvent> _eventRepository;
        private readonly IFormsAuthenticationService _formsService;
        private readonly IMembershipService _membershipService;
        private readonly IAccountService _accountService;
        private readonly IPaymentGateway _paymentGateway;
        private readonly ITaxAmountResolver _taxResolver;
        private IPayPalConfiguration _paypalConfiguration;

        public EventController()
        {
            _accountService = MvcApplication.Kernel.Get<IAccountService>();
            _eventRepository = MvcApplication.Kernel.Get<IRepository<GroupGivingEvent>>();
            _formsService = MvcApplication.Kernel.Get<IFormsAuthenticationService>();
            _membershipService = MvcApplication.Kernel.Get<AccountMembershipService>();
            _paymentGateway = MvcApplication.Kernel.Get<IPaymentGateway>();
            _taxResolver = MvcApplication.Kernel.Get<ITaxAmountResolver>();
            _paypalConfiguration = MvcApplication.Kernel.Get<IPayPalConfiguration>();
            ((RavenDBMembershipProvider)Membership.Provider).DocumentStore
                = RavenDbDocumentStore.Instance;
        }

        //
        // GET: /Event/
        public ActionResult Index(string shortUrl)
        {
            if (string.IsNullOrWhiteSpace(shortUrl))
                return new HttpNotFoundResult();

            var viewModel = new EventViewModel();
            var givingEvent = _eventRepository.Retrieve(e=>e.ShortUrl==shortUrl);
            if (givingEvent == null)
                return HttpNotFound();

            viewModel.EventId = givingEvent.Id;
            viewModel.StartDate = givingEvent.StartDate;
            viewModel.AdditionalBenefits = givingEvent.AdditionalBenefits;
            viewModel.AddressLine = givingEvent.AddressLine;
            viewModel.City = givingEvent.City;
            viewModel.Description = givingEvent.Description;
            viewModel.IsFeatured = givingEvent.IsFeatured;
            viewModel.IsPrivate = givingEvent.IsPrivate;
            viewModel.MaximumParticipants = givingEvent.MaximumParticipants;
            viewModel.MinimumParticipants = givingEvent.MinimumParticipants;
            viewModel.PhoneNumber = givingEvent.PhoneNumber;
            viewModel.SalesEndDateTime = givingEvent.SalesEndDateTime;
            viewModel.ShortUrl = givingEvent.ShortUrl;
            viewModel.Title = givingEvent.Title;
            viewModel.TicketPrice = givingEvent.TicketPrice;
            viewModel.Venue = givingEvent.Venue;
            viewModel.VenueLatitude = givingEvent.Latitude;
            viewModel.VenueLongitude = givingEvent.Longitude;
            viewModel.EventIsOn = givingEvent.IsOn;
            viewModel.EventIsFull = givingEvent.IsFull;
            viewModel.ContactName = givingEvent.OrganiserName;

            viewModel.PledgeCount = givingEvent.PledgeCount;
            viewModel.RequiredPledgesPercentage = (int)Math.Round(((double) viewModel.PledgeCount/(double) Math.Max(givingEvent.MinimumParticipants, 1))*100, 0);

            if (givingEvent.SalesEndDateTime > DateTime.Now)
            {
                var timeLeft = givingEvent.SalesEndDateTime - DateTime.Now;
                viewModel.DaysLeft = timeLeft.Days;
                viewModel.HoursLeft = timeLeft.Hours;
                viewModel.MinutesLeft = timeLeft.Minutes;
                viewModel.SecondsLeft = timeLeft.Seconds;
                viewModel.CountDown = true;
            }

            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("share")]
        public ActionResult Share(string shortUrl)
        {
            if (string.IsNullOrWhiteSpace(shortUrl))
                return new HttpNotFoundResult();

            var viewModel = new ShareEventViewModel();
            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("pledge")]
        public ActionResult Pledge(string shortUrl)
        {
            if (string.IsNullOrWhiteSpace(shortUrl))
                return new HttpNotFoundResult();

            var givingEvent = _eventRepository.Retrieve(e => e.ShortUrl == shortUrl);
            if (givingEvent == null)
                return HttpNotFound();

            EventPledgeViewModel viewModel = BuildEventPageViewModel(givingEvent);

            return View(viewModel);
        }

        private EventPledgeViewModel BuildEventPageViewModel(GroupGivingEvent givingEvent)
        {
            return BuildEventPageViewModel(givingEvent, new EventPledgeViewModel());
        }
        private EventPledgeViewModel BuildEventPageViewModel(GroupGivingEvent givingEvent, EventPledgeViewModel viewModel)
        {
            viewModel.EventId = givingEvent.Id;
            viewModel.StartDate = givingEvent.StartDate;
            viewModel.AdditionalBenefits = givingEvent.AdditionalBenefits;
            viewModel.AddressLine = givingEvent.AddressLine;
            viewModel.City = givingEvent.City;
            viewModel.Description = givingEvent.Description;
            viewModel.IsFeatured = givingEvent.IsFeatured;
            viewModel.IsPrivate = givingEvent.IsPrivate;
            viewModel.MaximumParticipants = givingEvent.MaximumParticipants;
            viewModel.MinimumParticipants = givingEvent.MinimumParticipants;
            viewModel.PhoneNumber = givingEvent.PhoneNumber;
            viewModel.SalesEndDateTime = givingEvent.SalesEndDateTime;
            viewModel.ShortUrl = givingEvent.ShortUrl;
            viewModel.Title = givingEvent.Title;
            viewModel.TicketPrice = givingEvent.TicketPrice;
            viewModel.Venue = givingEvent.Venue;
            viewModel.EventIsOn = givingEvent.IsOn;
            viewModel.EventIsFull = givingEvent.IsFull;
            viewModel.AttendeeName = viewModel.AttendeeName ?? new string[1];

            if (User.Identity.IsAuthenticated)
            {
                var account = _accountService.RetrieveByEmailAddress(User.Identity.Name);
                viewModel.UserFirstName = account.FirstName;
                viewModel.UserLastName = account.LastName;
                viewModel.EmailAddress = account.Email;
            }
            return viewModel;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Pledge(EventPledgeViewModel request)
        {
            EventPledgeViewModel viewModel = null;
            var eventDetails = _eventRepository.Retrieve(e => e.ShortUrl == request.ShortUrl);
            var account = _accountService.RetrieveByEmailAddress(request.EmailAddress);

            if (!request.AcceptTerms)
            {
                ModelState.AddModelError("acceptTerms", "You must accept the Terms and Conditions to pledge");
            }

            foreach(string attendeeName in request.AttendeeName)
            {
                if (string.IsNullOrWhiteSpace(attendeeName) || attendeeName.Length==0)
                {
                    ModelState.AddModelError("attendeeName", "Make sure you provide the name of each attendee");
                    break;
                }
            }

            if (!ModelState.IsValid)
            {
                viewModel = BuildEventPageViewModel(eventDetails, request);
                return View(viewModel);
            }


            if (account == null)
            {
                IEmailRelayService emailRelayService = MvcApplication.Kernel.Get<IEmailRelayService>();
                account = _accountService.CreateIncompleteAccount(request.EmailAddress, emailRelayService);
            }

            if (request.OptInForOffers)
            {
                account.OptInForOffers = true;
                _accountService.UpdateAccount(account);
            }

            var action = new MakePledgeAction(_taxResolver, _eventRepository, _paymentGateway, _paypalConfiguration);
            var makePledgeRequest = new MakePledgeRequest()
            {
                AttendeeNames = request.AttendeeName,
                PayPalEmailAddress = request.EmailAddress
            };

            CreatePledgeActionResult result = null;
            try
            {
                result = action.Attempt(eventDetails, account, makePledgeRequest);
            } catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("_form", ex.Message);
                viewModel = BuildEventPageViewModel(eventDetails, request);
                return View(viewModel);
            }

            PaymentGatewayResponse response = result.GatewayResponse;

            if (!result.Succeeded)
                return View(viewModel);

            return Redirect(string.Format(response.PaymentPageUrl, response.TransactionId));
        }

        [ActionName("edit-event")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Edit(string shortUrl)
        {
            var viewModel = new UpdateEventViewModel();
            var groupGivingEvent = _eventRepository.Retrieve(e => e.ShortUrl == shortUrl);

            AutoMapper.Mapper.CreateMap<GroupGivingEvent, UpdateEventViewModel>();
            viewModel = AutoMapper.Mapper.Map<GroupGivingEvent, UpdateEventViewModel>(groupGivingEvent);

            return View(viewModel);
        }

        [ActionName("edit-event")]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Edit(string shortUrl, UpdateEventViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var groupGivingEvent = _eventRepository.Retrieve(e => e.ShortUrl == shortUrl);
            AutoMapper.Mapper.CreateMap<UpdateEventViewModel, GroupGivingEvent>()
                .ForMember(d=>d.Id, m=>m.Ignore());

            this.TryUpdateModel(groupGivingEvent);

            _eventRepository.SaveOrUpdate(groupGivingEvent);
            _eventRepository.CommitUpdates();

            return RedirectToAction("edit-event", new {shortUrl=shortUrl});
        }

        [ActionName("event-pledges")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ListPledges(string shortUrl)
        {
            var groupGivingEvent = _eventRepository.Retrieve(e => e.ShortUrl == shortUrl);

            AutoMapper.Mapper.CreateMap<GroupGivingEvent, UpdateEventViewModel>();
            var viewModel = AutoMapper.Mapper.Map<GroupGivingEvent, UpdateEventViewModel>(groupGivingEvent);

            return View(viewModel);
        }

        [ActionName("refund-pledge")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult RefundPledge(string shortUrl, string orderNumber)
        {
            var @event = _eventRepository.Retrieve(e => e.ShortUrl == shortUrl);
            var pledge = @event.Pledges.Where(p => p.OrderNumber == orderNumber).FirstOrDefault();
            if (pledge==null)
            {
                ModelState.AddModelError("ordernumber", "We couldn't locate a pledge with that order number.");
                return View();
            }

            var viewModel = new GroupGiving.Web.Models.RefundViewModel();
            viewModel.Event = @event;
            viewModel.PledgeToBeRefunded = pledge;

            return View(viewModel);
        }

        [ActionName("refund-pledge")]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult RefundPledge(string shortUrl, string orderNumber, bool? confirm)
        {
            var @event = _eventRepository.Retrieve(e => e.ShortUrl == shortUrl);
            var pledge = @event.Pledges.Where(p => p.OrderNumber == orderNumber).FirstOrDefault();
            if (pledge == null)
            {
                ModelState.AddModelError("ordernumber", "We couldn't locate a pledge with that order number.");
            }

            if (!confirm.HasValue || !confirm.Value)
            {
                ModelState.AddModelError("confirm",
                                         "You have to confirm that you definitely want to refund this pledge.");
            }

            if (!ModelState.IsValid)
            {
                var viewModel = new GroupGiving.Web.Models.RefundViewModel();
                viewModel.Event = @event;
                viewModel.PledgeToBeRefunded = pledge;

                return View(viewModel);
            }

            pledge.DateRefunded = DateTime.Now;
            pledge.PaymentStatus = PaymentStatus.Refunded;
            pledge.Paid = false;
            _eventRepository.SaveOrUpdate(@event);
            _eventRepository.CommitUpdates();

            TempData["refunded"] = true;

            return RedirectToAction("event-pledges", new {shortUrl = shortUrl});
        }
    }
}