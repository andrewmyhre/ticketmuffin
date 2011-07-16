using System;
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
        private readonly IEmailRelayService _emailRelayService;
        private readonly IEmailCreationService _emailCreationService;
        private readonly IRepository<GroupGivingEvent> _eventRepository;
        private readonly IFormsAuthenticationService _formsService;
        private readonly IMembershipService _membershipService;
        private readonly IAccountService _accountService;
        private readonly IPaymentGateway _paymentGateway;
        private readonly ITaxAmountResolver _taxResolver;
        private IPayPalConfiguration _paypalConfiguration;

        public EventController()
        {
            _emailRelayService = MvcApplication.Kernel.Get<IEmailRelayService>();
            _emailCreationService = MvcApplication.Kernel.Get<IEmailCreationService>();
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
            viewModel.VenueLatitude = 50.022019d;
            viewModel.VenueLongitude = 19.984719d;
            viewModel.EventIsOn = givingEvent.IsOn;
            viewModel.EventIsFull = givingEvent.IsFull;

            viewModel.PledgeCount = givingEvent.Pledges.Sum(p=>p.Attendees.Count);
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

            var action = new MakePledgeAction(_taxResolver, _eventRepository, _paymentGateway, _paypalConfiguration, _emailCreationService, _emailRelayService);
            var makePledgeRequest = new MakePledgeRequest()
            {
                AttendeeNames = request.AttendeeName,
                PayPalEmailAddress = account.Email
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
    }
}
