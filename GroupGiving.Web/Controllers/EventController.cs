using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Web.Security;
using System.Xml;
using GroupGiving.Core.Configuration;
using Raven.Client;
using anrControls;
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
using RefundRequest = GroupGiving.Core.Dto.RefundRequest;

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
        private readonly IDocumentStore _documentStore;
        private static Markdown _markdown =  new Markdown();
        private IEmailRelayService _emailRelayService;
        private readonly IIdentity _userIdentity;

        public EventController(IAccountService accountService, IRepository<GroupGivingEvent> eventRepository, 
            IFormsAuthenticationService formsService, IMembershipService membershipService, IPaymentGateway paymentGateway, 
            ITaxAmountResolver taxResolver, IPayPalConfiguration paypalConfiguration, IDocumentStore documentStore, 
            IEmailRelayService emailRelayService, IIdentity userIdentity)
        {
            _accountService = accountService;
            _eventRepository = eventRepository;
            _formsService = formsService;
            _membershipService = membershipService;
            _paymentGateway = paymentGateway;
            _taxResolver = taxResolver;
            _paypalConfiguration = paypalConfiguration;
            _documentStore = documentStore;
            ((RavenDBMembershipProvider)Membership.Provider).DocumentStore
                = documentStore;
            _emailRelayService = emailRelayService;
            _userIdentity = userIdentity;
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

            Account userAccount = null;
            if (_userIdentity.IsAuthenticated)
            {
                userAccount = _accountService.RetrieveByEmailAddress(_userIdentity.Name);
                viewModel.UserIsEventOwner = givingEvent.OrganiserId == userAccount.Id;
            }

            viewModel.EventId = givingEvent.Id;
            viewModel.StartDate = givingEvent.StartDate;
            viewModel.AdditionalBenefitsMarkedDown = _markdown.Transform(!string.IsNullOrWhiteSpace(givingEvent.AdditionalBenefits) ? givingEvent.AdditionalBenefits : "");
            viewModel.AddressLine = givingEvent.AddressLine;
            viewModel.City = givingEvent.City;
            viewModel.PostCode = givingEvent.Postcode;
            viewModel.DescriptionMarkedDown = _markdown.Transform(!string.IsNullOrWhiteSpace(givingEvent.Description) ? givingEvent.Description : "");
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
        [ActionName("pledge")]
        public ActionResult Pledge(string shortUrl)
        {
            if (string.IsNullOrWhiteSpace(shortUrl))
                return new HttpNotFoundResult();

            var givingEvent = _eventRepository.Retrieve(e => e.ShortUrl == shortUrl);
            if (givingEvent == null)
                return HttpNotFound();

            if (givingEvent.SalesEndDateTime < DateTime.Now)
            {
                return RedirectToAction("Index", new {shortUrl = givingEvent.ShortUrl});
            }

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
            viewModel.AdditionalBenefitsMarkedDown = givingEvent.AdditionalBenefits;
            viewModel.AddressLine = givingEvent.AddressLine;
            viewModel.City = givingEvent.City;
            viewModel.DescriptionMarkedDown = givingEvent.Description;
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
            viewModel.UserIsEventOwner = true; // todo

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
            var organiserAccount = _accountService.RetrieveById(eventDetails.OrganiserId);

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
                account = _accountService.CreateIncompleteAccount(request.EmailAddress, _emailRelayService);
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
                result = action.Attempt(eventDetails, account, organiserAccount, makePledgeRequest);
            } catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("_form", ex.Message);
                viewModel = BuildEventPageViewModel(eventDetails, request);
                return View(viewModel);
            }

            var response = result.GatewayResponse;

            if (!result.Succeeded)
            {
                viewModel = BuildEventPageViewModel(eventDetails, request);
                return View(viewModel);
            }

            return Redirect(string.Format(response.PaymentPageUrl, response.payKey));
        }

        [ActionName("edit-event")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Edit(string shortUrl)
        {
            var viewModel = new UpdateEventViewModel();
            var groupGivingEvent = _eventRepository.Retrieve(e => e.ShortUrl == shortUrl);

            AutoMapper.Mapper.CreateMap<GroupGivingEvent, UpdateEventViewModel>();
            viewModel = AutoMapper.Mapper.Map<GroupGivingEvent, UpdateEventViewModel>(groupGivingEvent);
            viewModel.LatLong = string.Format("{0:#.#####},{1:#.#####}", groupGivingEvent.Latitude, groupGivingEvent.Longitude);

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
            this.TryUpdateModel(groupGivingEvent);

            if (groupGivingEvent.SalesEndDateTime > DateTime.Now)
                groupGivingEvent.State = EventState.SalesReady;
            else if (groupGivingEvent.StartDate > DateTime.Now)
                groupGivingEvent.State = EventState.SalesClosed;
            else
                groupGivingEvent.State = EventState.Completed;
            

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

        [ActionName("management-console")]
        [HttpGet]
        public ActionResult ManagementConsole(string shortUrl)
        {
            var groupGivingEvent = _eventRepository.Retrieve(e => e.ShortUrl == shortUrl);

            AutoMapper.Mapper.CreateMap<GroupGivingEvent, UpdateEventViewModel>();
            var viewModel = AutoMapper.Mapper.Map<GroupGivingEvent, UpdateEventViewModel>(groupGivingEvent);

            return View(viewModel);
            
        }

        [ActionName("cancel-event")]
        [HttpGet]
        public ActionResult CancelEventAreYouSure(string shortUrl)
        {
            return View();
        }

        [ActionName("cancel-event")]
        [HttpPost]
        public ActionResult CancelEvent(string shortUrl, string confirmationCode)
        {
            if (confirmationCode != "cancel")
            {
                ModelState.AddModelError("confirmationCode", "Incorrect confirmation");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            // cancel the event - refund each pledger
            Dictionary<string, GroupGiving.Core.Dto.RefundResponse> results =
                new Dictionary<string, GroupGiving.Core.Dto.RefundResponse>();
            using (var session = _documentStore.OpenSession())
            {
                var @event = session.Query<GroupGivingEvent>().Where(e=>e.ShortUrl==shortUrl).SingleOrDefault();
                var pledges =
                    @event.Pledges.Where(
                        p =>
                        p.PaymentStatus == PaymentStatus.Reconciled ||
                        p.PaymentStatus == PaymentStatus.PaidPendingReconciliation);
                foreach (var pledge in pledges)
                {
                    var refundResult = _paymentGateway.Refund(new RefundRequest() {TransactionId = pledge.TransactionId});
                    results.Add(pledge.TransactionId, refundResult);
                    if (refundResult.Successful)
                    {
                        pledge.Paid = false;
                        pledge.PaymentStatus = PaymentStatus.Refunded;
                        pledge.DateRefunded = DateTime.Now;
                    }
                }

                @event.State = EventState.Cancelled;
                session.SaveChanges();
            }

            if (results.Any(r=>!r.Value.Successful))
            {
                TempData["failures"] = true;
            }
            else
            {
                TempData["failures"] = false;
            }

            return RedirectToAction("event-cancelled", new {shortUrl = shortUrl});
        }

        [ActionName("event-cancelled")]
        [HttpGet]
        public ActionResult EventCancelled(string shortUrl)
        {
            var eventCancelledViewModel = new EventCancelledViewModel();
            eventCancelledViewModel.Failures = (bool) TempData["failures"];
            return View(eventCancelledViewModel);
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
            RefundViewModel viewModel = null;

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
                viewModel.Event = @event;
                viewModel.PledgeToBeRefunded = pledge;

                return View(viewModel);
            }

            var refundResult = _paymentGateway.Refund(new RefundRequest() {TransactionId = pledge.TransactionId});

            if (refundResult.Successful)
            {
                pledge.DateRefunded = DateTime.Now;
                pledge.PaymentStatus = PaymentStatus.Refunded;
                pledge.Paid = false;
                _eventRepository.SaveOrUpdate(@event);
                _eventRepository.CommitUpdates();
                TempData["refunded"] = true;
                return RedirectToAction("event-pledges", new { shortUrl = shortUrl });
            }

            viewModel.Event = @event;
            viewModel.PledgeToBeRefunded = pledge;
            viewModel.RefundFailed = true;

            return View();

        }
    }

    public class EventCancelledViewModel
    {
        public bool Failures { get; set; }
    }
}