using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Web.Hosting;
using System.Web.Security;
using EmailProcessing;
using Raven.Client;
using TicketMuffin.Core.Actions.ActivateEvent;
using TicketMuffin.Core.Actions.CancelEvent;
using TicketMuffin.Core.Actions.CreatePledge;
using TicketMuffin.Core.Actions.RefundPledge;
using TicketMuffin.Core.Configuration;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Payments;
using TicketMuffin.Core.Services;
using System.Web.Mvc;
using RavenDBMembership.Provider;
using TicketMuffin.PayPal;
using TicketMuffin.PayPal.Model;
using TicketMuffin.Web.Code;
using TicketMuffin.Web.Configuration;
using TicketMuffin.Web.Models;
using log4net;

namespace TicketMuffin.Web.Controllers
{
    public class EventController : Controller
    {
        private readonly ILog logger = LogManager.GetLogger(typeof (EventController));
        private readonly IFormsAuthenticationService _formsService;
        private readonly IMembershipService _membershipService;
        private readonly IAccountService _accountService;
        private readonly IPaymentGateway _paymentGateway;
        private readonly ITaxAmountResolver _taxResolver;
        private ISiteConfiguration _siteConfiguration;
        private readonly IDocumentSession _ravenSession;
        private static Markdown _markdown = new Markdown();
        private readonly IIdentity _userIdentity;
        private readonly IEventService _eventService;
        private readonly IEmailFacade _emailService;
        private readonly ICultureService _cultureService;
        private ITicketGenerator _ticketGenerator;
        private IEventCultureResolver _cultureResolver;
        private readonly IOrderNumberGenerator _orderNumberGenerator;

        public EventController(IAccountService accountService,
                               IFormsAuthenticationService formsService, IMembershipService membershipService,
                               IPaymentGateway paymentGateway,
                               ITaxAmountResolver taxResolver, ISiteConfiguration siteConfiguration,
                               IDocumentSession ravenSession, 
            IIdentity userIdentity, 
            IEventService eventService,
            IEmailFacade emailService,
            ICultureService cultureService, ITicketGenerator ticketGenerator, IEventCultureResolver cultureResolver,
            IOrderNumberGenerator orderNumberGenerator)
        {
            _accountService = accountService;
            _formsService = formsService;
            _membershipService = membershipService;
            _paymentGateway = paymentGateway;
            _taxResolver = taxResolver;
            _siteConfiguration = siteConfiguration;
            _ravenSession = ravenSession;
            ((RavenDBMembershipProvider) Membership.Provider).DocumentStore
                = _ravenSession.Advanced.DocumentStore;
            _userIdentity = userIdentity;
            _eventService = eventService;
            _emailService = emailService;
            _cultureService = cultureService;
            _ticketGenerator = ticketGenerator;
            _cultureResolver = cultureResolver;
            _orderNumberGenerator = orderNumberGenerator;
        }

        //
        // GET: /Event/
        public ActionResult Index(string shortUrl)
        {
            if (string.IsNullOrWhiteSpace(shortUrl))
                return new HttpNotFoundResult();

            var viewModel = new EventViewModel();
            var givingEvent = _eventService.Retrieve(shortUrl);
            if (givingEvent == null)
                return HttpNotFound();

            Account userAccount = null;
            if (_userIdentity.IsAuthenticated)
            {
                userAccount = _accountService.RetrieveByEmailAddress(_userIdentity.Name);
                viewModel.UserIsEventOwner = givingEvent.OrganiserId == userAccount.Id;
            }


            viewModel.Id = givingEvent.Id;
            viewModel.StartDate = givingEvent.StartDate;
            viewModel.AdditionalBenefitsMarkedDown =
                _markdown.Transform(!string.IsNullOrWhiteSpace(givingEvent.AdditionalBenefits)
                                        ? givingEvent.AdditionalBenefits
                                        : "");
            viewModel.AddressLine = givingEvent.AddressLine;
            viewModel.City = givingEvent.City;
            viewModel.PostCode = givingEvent.Postcode;
            viewModel.DescriptionMarkedDown =
                _markdown.Transform(!string.IsNullOrWhiteSpace(givingEvent.Description) ? givingEvent.Description : "");
            viewModel.IsFeatured = givingEvent.IsFeatured;
            viewModel.IsPrivate = givingEvent.IsPrivate;
            viewModel.MaximumParticipants = givingEvent.MaximumParticipants;
            viewModel.MinimumParticipants = givingEvent.MinimumParticipants;
            viewModel.PhoneNumber = givingEvent.PhoneNumber;
            viewModel.SalesEndDateTime = givingEvent.SalesEndDateTime;
            viewModel.ShortUrl = givingEvent.ShortUrl;
            viewModel.Title = givingEvent.Title;
            viewModel.TicketPrice = givingEvent.TicketPrice;
            viewModel.Currency = (Currency) givingEvent.Currency;
            viewModel.Venue = givingEvent.Venue;
            viewModel.VenueLatitude = givingEvent.Latitude;
            viewModel.VenueLongitude = givingEvent.Longitude;
            viewModel.EventIsOn = givingEvent.IsOn;
            viewModel.EventIsFull = givingEvent.IsFull;
            viewModel.ContactName = givingEvent.OrganiserName;
            viewModel.State = givingEvent.State;
            viewModel.ImageUrl = givingEvent.ImageUrl;
            viewModel.Charity = givingEvent.CharityDetails;

            viewModel.PledgeCount = givingEvent.PaidAttendeeCount;
            viewModel.RequiredPledgesPercentage =
                (int)
                Math.Round(((double) viewModel.PledgeCount/(double) Math.Max(givingEvent.MinimumParticipants, 1))*100, 0);
            if (givingEvent.MaximumParticipants.HasValue)
            {
                viewModel.TotalPledgesPercentage =
                    (int)
                    Math.Round(
                        ((double) viewModel.PledgeCount/(double) Math.Max(givingEvent.MaximumParticipants.Value, 1))*100,
                        0);
            }
            else
            {
                viewModel.TotalPledgesPercentage = viewModel.RequiredPledgesPercentage;
            }

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

            var givingEvent = _eventService.Retrieve(shortUrl);
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

        private EventPledgeViewModel BuildEventPageViewModel(GroupGivingEvent givingEvent,
                                                             EventPledgeViewModel viewModel)
        {
            viewModel.Id = givingEvent.Id;
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
            viewModel.Currency = (Currency)givingEvent.Currency;
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
            var eventDetails = _eventService.Retrieve(request.ShortUrl);
            var account = _accountService.RetrieveByEmailAddress(request.EmailAddress);
            var organiserAccount = _accountService.GetById(eventDetails.OrganiserId);

            if (!request.AcceptTerms)
            {
                ModelState.AddModelError("acceptTerms", "You must accept the Terms and Conditions to pledge");
            }

            foreach (string attendeeName in request.AttendeeName)
            {
                if (string.IsNullOrWhiteSpace(attendeeName) || attendeeName.Length == 0)
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

            var action = new MakePledgeAction(_taxResolver, _paymentGateway, _ravenSession, _orderNumberGenerator);
            var makePledgeRequest = new MakePledgeRequest()
                                        {
                                            AttendeeNames = request.AttendeeName,
                                            PayPalEmailAddress = request.EmailAddress,
                                            OptInForOffers = request.OptInForOffers,
                                            WebsiteUrlBase =
                                                string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Authority)
                                        };

            CreatePledgeActionResult result = null;
            try
            {
                result = action.Attempt(eventDetails.Id, organiserAccount, makePledgeRequest);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("_form", ex.Message);
                viewModel = BuildEventPageViewModel(eventDetails, request);

                return View(viewModel);
            }

            if (!result.Succeeded)
            {
                viewModel = BuildEventPageViewModel(eventDetails, request);
                ModelState.AddModelError("_request", result.Exception.Message);
                return View(viewModel);
            }

            return Redirect(string.Format(result.PaymentPageUrl, result.TransactionId));
        }

        [ActionName("edit-event")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Edit(string shortUrl)
        {
            var groupGivingEvent = _eventService.Retrieve(shortUrl);
            Account userAccount = null;
            if (_userIdentity.IsAuthenticated)
            {
                userAccount = _accountService.RetrieveByEmailAddress(_userIdentity.Name);
                if (groupGivingEvent.OrganiserId != userAccount.Id)
                {
                    return RedirectToAction("Index", new {shortUrl});
                }
            }

            var viewModel = new UpdateEventViewModel();

            AutoMapper.Mapper.CreateMap<GroupGivingEvent, UpdateEventViewModel>();
            viewModel = AutoMapper.Mapper.Map<GroupGivingEvent, UpdateEventViewModel>(groupGivingEvent);
            viewModel.LatLong = string.Format("{0:#.#####},{1:#.#####}", groupGivingEvent.Latitude,
                                              groupGivingEvent.Longitude);

            return View(viewModel);
        }

        [ActionName("edit-event")]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Edit(string shortUrl, UpdateEventViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                StringBuilder debug=new StringBuilder();
                foreach(var item in ModelState.Keys)
                {
                    var state = ModelState[item];
                    debug.AppendFormat("<p>{0}: {1}, {2} errors", item, state.Value, state.Errors.Count);
                    if (state.Errors.Count > 0)
                    {
                        foreach (var error in state.Errors)
                            debug.AppendFormat("<br/>{0} - {1}", error.ErrorMessage, error.Exception.Message);
                    }
                    debug.AppendLine("</p>");
                }
                var result = new ContentResult(){Content = debug.ToString(), ContentType = "text/html"};
                return result;
                return View(viewModel);
            }

            var groupGivingEvent = _ravenSession.Load<GroupGivingEvent>(viewModel.Id);
            this.TryUpdateModel(groupGivingEvent);
            groupGivingEvent.Currency = (int)viewModel.Currency;

            // update organiser details if we have an organiser id but not organiser name set on the event
            if (string.IsNullOrWhiteSpace(groupGivingEvent.OrganiserName)
                && !string.IsNullOrWhiteSpace(groupGivingEvent.OrganiserId))
            {
                var organiser = _ravenSession.Load<Account>(groupGivingEvent.OrganiserId);
                if (organiser != null)
                {
                    groupGivingEvent.OrganiserName = organiser.FirstName + " " + organiser.LastName;
                }
            }

            if (groupGivingEvent.SalesEndDateTime > DateTime.Now)
                groupGivingEvent.State = EventState.SalesReady;
            else if (groupGivingEvent.StartDate > DateTime.Now)
                groupGivingEvent.State = EventState.SalesClosed;
            else
                groupGivingEvent.State = EventState.Completed;
            
            return RedirectToAction("edit-event", new {shortUrl = shortUrl});
        }

        [ActionName("event-pledges")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ListPledges(string shortUrl)
        {
            var groupGivingEvent = _eventService.Retrieve(shortUrl);

            AutoMapper.Mapper.CreateMap<GroupGivingEvent, UpdateEventViewModel>();
            var viewModel = new PledgeListViewModel();
            viewModel.Pledges = groupGivingEvent.Pledges.Where(p => !p.Paid);
            viewModel.EventName = groupGivingEvent.Title;
            viewModel.ShortUrl = groupGivingEvent.ShortUrl;

            return View(viewModel);
        }

        [ActionName("management-console")]
        [HttpGet]
        public ActionResult ManagementConsole(string shortUrl)
        {
            var groupGivingEvent = _eventService.Retrieve(shortUrl);

            AutoMapper.Mapper.CreateMap<GroupGivingEvent, UpdateEventViewModel>();
            var viewModel = AutoMapper.Mapper.Map<GroupGivingEvent, UpdateEventViewModel>(groupGivingEvent);

            return View(viewModel);

        }

        public ActionResult Attendees(string shortUrl)
        {
            var viewModel = new AttendeeListViewModel();
            var @event = _eventService.Retrieve(shortUrl);

            viewModel.Attendees = from pledge in @event.Pledges
                                  from attendee in pledge.Attendees
                                  select new AttendeeViewModel()
                                             {
                                                 FullName = attendee.FullName,
                                                 OrderNumber = pledge.OrderNumber
                                             };
            viewModel.EventName = @event.Title;
            viewModel.ShortUrl = @event.ShortUrl;

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
            var @event =_eventService.Retrieve(shortUrl);

            if (@event.State == EventState.Cancelled || @event.State == EventState.Completed)
            {
                return RedirectToAction("Index", new {shortUrl = shortUrl});
            }

            CancelEventAction action = new CancelEventAction(_paymentGateway);
            var cancelEventResponse = action.Execute(_ravenSession, @event.Id);

            if (cancelEventResponse.Success)
            {
                @event.State = EventState.Cancelled;
                _ravenSession.SaveChanges();
                TempData["failures"] = false;
                return RedirectToAction("event-cancelled", new {shortUrl = shortUrl});
            }
            else
            {
                TempData["failures"] = true;
                return RedirectToAction("cancel-event", new {shortUrl = shortUrl});
            }
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
            var @event = _eventService.Retrieve(shortUrl);
            var pledge = @event.Pledges.Where(p => p.OrderNumber == orderNumber).FirstOrDefault();
            if (pledge == null)
            {
                ModelState.AddModelError("ordernumber", "We couldn't locate a pledge with that order number.");
                return View();
            }

            var viewModel = new RefundViewModel();
            viewModel.Event = @event;
            viewModel.PledgeToBeRefunded = pledge;

            return View(viewModel);
        }

        [ActionName("refund-pledge")]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult RefundPledge(string shortUrl, string orderNumber, bool? confirm)
        {
            var @event = _ravenSession.Query<GroupGivingEvent>().SingleOrDefault(e => e.ShortUrl == shortUrl);
            var pledge = @event.Pledges.SingleOrDefault(p => p.OrderNumber == orderNumber);
            RefundViewModel viewModel = new RefundViewModel();

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

            IPaymentRefundResponse refundResult = null;
            RefundPledgeAction action = new RefundPledgeAction(_paymentGateway);
            try
            {
                refundResult = action.Execute(_ravenSession, @event.Id, pledge.OrderNumber);

            }
            catch (Exception exception)
            {
                logger.Fatal("Could not refund pledge with order number " + pledge.OrderNumber, exception);
                return RedirectToAction("event-pledges", new {shortUrl = shortUrl});
            }

            if (refundResult.Successful)
            {
                TempData["refunded"] = true;
                return RedirectToAction("event-pledges", new {shortUrl = shortUrl});
            }

            viewModel.Event = @event;
            viewModel.PledgeToBeRefunded = pledge;
            viewModel.RefundFailed = true;

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult UploadEventImage(string shortUrl)
        {
            if (Request.Files.Count > 0)
            {
                var imageFile = Request.Files[0];
                if (imageFile == null || imageFile.ContentLength > 0)
                {
                    ModelState.AddModelError("imageFile", "Please select an image to upload");
                }

                Image image = null;
                byte[] fileData = new byte[imageFile.ContentLength];
                imageFile.InputStream.Read(fileData, 0, fileData.Length);
                MemoryStream imageStream = new MemoryStream(fileData);
                try
                {
                    image = Bitmap.FromStream(imageStream);

                    // save the uploaded image
                    // todo: should resize the image here
                    string imagePath = string.Format(ConfigurationManager.AppSettings["EventImagePathFormat"],
                                                     shortUrl);
                    if (imagePath.StartsWith("~") || imagePath.StartsWith("/"))
                        imagePath = HostingEnvironment.MapPath(imagePath);
                    if (!Directory.Exists(Path.GetDirectoryName(imagePath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(imagePath));

                    image.Save(imagePath, ImageFormat.Jpeg);

                    var @event = _eventService.Retrieve(shortUrl);
                    if (@event == null)
                    {
                        return RedirectToAction("Index", new {shortUrl = shortUrl});
                    }

                    @event.ImageFilename = imagePath;
                    @event.ImageUrl =
                        Url.Content(string.Format(ConfigurationManager.AppSettings["EventImageUrlFormat"], shortUrl));
                    _ravenSession.SaveChanges();
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError("imageFile", "Please select an image to upload");
                }

            }
            return RedirectToAction("edit-event", new {shortUrl = shortUrl});
        }

        public ActionResult Activate(string shortUrl)
        {
            var viewModel = new ActivateEventViewModel();

            viewModel.Event = _eventService.Retrieve(shortUrl);
            viewModel.TotalAmountOwedToFundraiser = viewModel.Event.Pledges.Sum(p => p.Total);

            if (!viewModel.Event.ReadyToActivate)
            {
                return RedirectToAction("Index", new {shortUrl = shortUrl});
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Activate(string shortUrl, string confirm)
        {
            var @event = _eventService.Retrieve(shortUrl);
            var action = new ActivateEventAction(_paymentGateway, _ticketGenerator, _cultureResolver);
            var response = action.Execute(@event.Id, _ravenSession);
            TempData["activate_response"] = response;

            return RedirectToAction("management-console", new {shortUrl = shortUrl});
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Contact(string shortUrl, string message, string senderName, string senderEmail)
        {
            // send the organiser an email
            // todo: messaging should all be done through TM
            var @event = _eventService.Retrieve(shortUrl);
            var account = _ravenSession.Query<Account>().FirstOrDefault(a => a.Id == @event.OrganiserId);
            _emailService.Send(account.Email, "NewMessage",
                new
                    {
                        Account=account,
                        Event=@event,
                        Message=message,
                        SenderName=senderName,
                        SenderEmail=senderEmail
                    },
                    _cultureService.GetCultureOrDefault(HttpContext, "pl"));
            
            if (Request.AcceptTypes.Contains("application/json"))
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.OK);
            }
            return RedirectToAction("Index", new {shortUrl = shortUrl});
        }
    }
}
