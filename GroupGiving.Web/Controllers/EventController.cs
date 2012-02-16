using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using System.Xml;
using GroupGiving.Core;
using GroupGiving.Core.Actions.ActivateEvent;
using GroupGiving.Core.Actions.CancelEvent;
using GroupGiving.Core.Actions.RefundPledge;
using GroupGiving.Core.Configuration;
using GroupGiving.Core.Dto;
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
using log4net;
using RefundRequest = GroupGiving.Core.Dto.RefundRequest;
using RefundResponse = GroupGiving.Core.Dto.RefundResponse;

namespace GroupGiving.Web.Controllers
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
        private readonly IDocumentStore _documentStore;
        private static Markdown _markdown = new Markdown();
        private IEmailRelayService _emailRelayService;
        private readonly IIdentity _userIdentity;
        private readonly IEventService _eventService;

        public EventController(IAccountService accountService,
                               IFormsAuthenticationService formsService, IMembershipService membershipService,
                               IPaymentGateway paymentGateway,
                               ITaxAmountResolver taxResolver, ISiteConfiguration siteConfiguration,
                               IDocumentStore documentStore,
                               IEmailRelayService emailRelayService, IIdentity userIdentity, IEventService eventService)
        {
            _accountService = accountService;
            _formsService = formsService;
            _membershipService = membershipService;
            _paymentGateway = paymentGateway;
            _taxResolver = taxResolver;
            _siteConfiguration = siteConfiguration;
            _documentStore = documentStore;
            ((RavenDBMembershipProvider) Membership.Provider).DocumentStore
                = documentStore;
            _emailRelayService = emailRelayService;
            _userIdentity = userIdentity;
            _eventService = eventService;
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
            var organiserAccount = _accountService.RetrieveById(eventDetails.OrganiserId);

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

            var action = new MakePledgeAction(_taxResolver, _paymentGateway, _siteConfiguration, _documentStore);
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

            var response = result.GatewayResponse;

            if (!result.Succeeded)
            {
                viewModel = BuildEventPageViewModel(eventDetails, request);
                ModelState.AddModelError("_request", result.Exception.Message);
                return View(viewModel);
            }

            return Redirect(string.Format(response.PaymentPageUrl, response.payKey));
        }

        [ActionName("edit-event")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Edit(string shortUrl)
        {
            var viewModel = new UpdateEventViewModel();
            var groupGivingEvent = _eventService.Retrieve(shortUrl);

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
                return View(viewModel);
            }

            using (var session = _documentStore.OpenSession())
            {
                var groupGivingEvent = session.Load<GroupGivingEvent>(viewModel.Id);
                this.TryUpdateModel(groupGivingEvent);
                groupGivingEvent.Currency = (int)viewModel.Currency;

                // update organiser details if we have an organiser id but not organiser name set on the event
                if (string.IsNullOrWhiteSpace(groupGivingEvent.OrganiserName)
                    && !string.IsNullOrWhiteSpace(groupGivingEvent.OrganiserId))
                {
                    var organiser = session.Load<Account>(groupGivingEvent.OrganiserId);
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


                session.SaveChanges();
            }
            return RedirectToAction("edit-event", new {shortUrl = shortUrl});
        }

        [ActionName("event-pledges")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ListPledges(string shortUrl)
        {
            var groupGivingEvent = _eventService.Retrieve(shortUrl);

            AutoMapper.Mapper.CreateMap<GroupGivingEvent, UpdateEventViewModel>();
            var viewModel = AutoMapper.Mapper.Map<GroupGivingEvent, UpdateEventViewModel>(groupGivingEvent);

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
            using (var session = _documentStore.OpenSession())
            {


                var @event =
                    session.Query<GroupGivingEvent>().Where(e => e.ShortUrl == shortUrl && e.State != EventState.Deleted)
                        .FirstOrDefault();

                if (@event.State == EventState.Cancelled || @event.State == EventState.Completed)
                {
                    return RedirectToAction("Index", new {shortUrl = shortUrl});
                }

                CancelEventAction action = new CancelEventAction(_paymentGateway);
                var cancelEventResponse = action.Execute(session, @event.Id);

                if (cancelEventResponse.Success)
                {
                    @event.State = EventState.Cancelled;
                    session.SaveChanges();
                    TempData["failures"] = false;
                    return RedirectToAction("event-cancelled", new {shortUrl = shortUrl});
                }
                else
                {
                    TempData["failures"] = true;
                    return RedirectToAction("cancel-event", new {shortUrl = shortUrl});
                }

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

            var viewModel = new GroupGiving.Web.Models.RefundViewModel();
            viewModel.Event = @event;
            viewModel.PledgeToBeRefunded = pledge;

            return View(viewModel);
        }

        [ActionName("refund-pledge")]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult RefundPledge(string shortUrl, string orderNumber, bool? confirm)
        {
            using (var session = _documentStore.OpenSession())
            {
                var @event = session.Query<GroupGivingEvent>().Where(e => e.ShortUrl == shortUrl).FirstOrDefault();
                var pledge = @event.Pledges.Where(p => p.OrderNumber == orderNumber).FirstOrDefault();
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
                RefundResponse refundResult = null;
                RefundPledgeAction action = new RefundPledgeAction(_paymentGateway);
                try
                {
                    refundResult = action.Execute(session, @event.Id, pledge.OrderNumber);

                }
                catch (Exception exception)
                {
                    logger.Fatal("Could not refund pledge with transaction id " + pledge.TransactionId, exception);
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

                    using (var session = _documentStore.OpenSession())
                    {
                        var @event = session.Query<GroupGivingEvent>().Where(e => e.ShortUrl == shortUrl
                                                                                  && e.State != EventState.Deleted)
                            .FirstOrDefault();
                        if (@event == null)
                        {
                            return RedirectToAction("Index", new {shortUrl = shortUrl});
                        }

                        @event.ImageFilename = imagePath;
                        @event.ImageUrl =
                            Url.Content(string.Format(ConfigurationManager.AppSettings["EventImagePathFormat"], shortUrl));
                        session.SaveChanges();
                    }
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

            using (var session = _documentStore.OpenSession())
            {
                viewModel.Event = session.Query<GroupGivingEvent>().Where(e => e.ShortUrl == shortUrl).FirstOrDefault();
                viewModel.TotalAmountOwedToFundraiser = viewModel.Event.Pledges.Sum(p => p.Total);

                if (!viewModel.Event.ReadyToActivate)
                {
                    return RedirectToAction("Index", new {shortUrl = shortUrl});
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Activate(string shortUrl, string confirm)
        {
            using (var session = _documentStore.OpenSession())
            {
                var @event = session.Query<GroupGivingEvent>().Where(e => e.ShortUrl == shortUrl).FirstOrDefault();
                var action = new ActivateEventAction(_documentStore, _paymentGateway);
                var response = action.Execute(@event.Id, session);
                TempData["activate_response"] = response;
            }

            return RedirectToAction("management-console", new {shortUrl = shortUrl});
        }
    }
}
