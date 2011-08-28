using System;
using System.Collections.Generic;
using System.Web.Mvc;
using GroupGiving.Core.Services;
using Ninject;
using RavenDBMembership.Web.Models;

namespace GroupGiving.Web.Controllers
{
    public class CreateEventController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IMembershipService _membershipService;
        private readonly IFormsAuthenticationService _formsAuthenticationService;
        private readonly IEventService _eventService;
        private readonly ICountryService _countryService;

        public CreateEventController()
        {
            _accountService = MvcApplication.Kernel.Get<IAccountService>();
            _countryService = MvcApplication.Kernel.Get<ICountryService>();
            _membershipService = MvcApplication.Kernel.Get<IMembershipService>();
            _formsAuthenticationService = MvcApplication.Kernel.Get<IFormsAuthenticationService>();
            _eventService = MvcApplication.Kernel.Get<IEventService>();
        }

        public CreateEventController(IAccountService accountService,
            IMembershipService membershipService,
            IFormsAuthenticationService formsAuthenticationService,
            IEventService eventService, ICountryService countryService)
        {
            _accountService = accountService;
            _countryService = countryService;
            _membershipService = membershipService;
            _formsAuthenticationService = formsAuthenticationService;
            _eventService = eventService;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("create")]
        [Authorize]
        public ActionResult EventDetails()
        {
            var viewModel = new CreateEventRequest();
            viewModel.StartDateTime = DateTime.Now;
            viewModel.StartTimes = TimeOptions();
            
            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("create")]
        [Authorize]
        public ActionResult EventDetails(CreateEventRequest request)
        {
            DateTime startDate = new DateTime();
            if (!DateTime.TryParse(string.Format("{0} {1}", request.StartDate, request.StartTime), out startDate))
            {
                ModelState.AddModelError("startDateTime", "Please select a valid date for the event");
            }
            else
            {
                request.StartDateTime = startDate;
            }
            if (request.StartDateTime < DateTime.Now)
            {
                ModelState.AddModelError("startDateTime", "The date you provided isn't valid because it's in the past");
            }
            if (!_eventService.ShortUrlAvailable(request.ShortUrl))
            {
                ModelState.AddModelError("ShortUrl", "Unfortunately that url is already in use");
            }
            if (!ModelState.IsValid)
            {
                request.StartDateTime = DateTime.Now;
                request.StartTimes = TimeOptions();
                return View(request);
            }

            var account = _accountService.RetrieveByEmailAddress(User.Identity.Name);
            request.OrganiserName = string.Format("{0} {1}", account.FirstName, account.LastName);
            var result = _eventService.CreateEvent(request);

            if (result.Success)
            {
                return RedirectToRoute("CreateEvent_TicketDetails", new { eventId = result.EventId });
            }

            ModelState.AddModelError("createevent", "There was a problem with the information you provided");
            return View(request);
        }

        bool ParseDateStrict(string dateValue, out DateTime dateTime)
        {
            dateTime = DateTime.MinValue;
            string[] values = dateValue.Split('/');
            if (values.Length != 3)
                return false;

            if (values[2].Length != 2 && values[2].Length != 4)
                return false;

            int year = 0, month = 0, day = 0;
            if (!int.TryParse(values[2], out year)
                || !int.TryParse(values[1], out month)
                || !int.TryParse(values[0], out day))
                return false;

            dateTime = new DateTime(year, month, day);
            return true;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("tickets")]
        [Authorize]
        public ActionResult TicketDetails(int eventId)
        {
            var viewModel = new SetTicketDetailsRequest();
            viewModel.SalesEndDateTime = DateTime.Now;
            viewModel.SalesEndTimeOptions = TimeOptions();
            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("tickets")]
        [Authorize]
        public ActionResult TicketDetails(SetTicketDetailsRequest setTicketDetailsRequest)
        {
            if (setTicketDetailsRequest.MaximumParticipants < setTicketDetailsRequest.MinimumParticipants)
            {
                ModelState.AddModelError("MaximumParticipants", "Maximum participants can't be less than the minimum");
            }

            DateTime salesEndDateTime = new DateTime();
            if (!DateTime.TryParse(setTicketDetailsRequest.SalesEndDate + " " + setTicketDetailsRequest.SalesEndTime, out salesEndDateTime))
            {
                ModelState.AddModelError("SalesEndDateTime", "Provide a valid date and time for ticket sales to end");
            }
            else
            {
                setTicketDetailsRequest.SalesEndDateTime = salesEndDateTime;
            }

            if (!ModelState.IsValid)
            {
                setTicketDetailsRequest.Times = TimeOptions();
                return View(setTicketDetailsRequest);
            }

            _eventService.SetTicketDetails(setTicketDetailsRequest);

            var @event = _eventService.Retrieve(setTicketDetailsRequest.EventId);

            return RedirectToRoute("Event_ShareYourEvent", new { shortUrl = @event.ShortUrl });
        }

        private SelectList TimeOptions()
        {
            List<string> dateTimes = new List<string>();
            DateTime time = new DateTime(200, 1, 1, 0, 0, 0);
            for (int i = 0; i < 48; i++)
            {
                dateTimes.Add(time.ToString("HH:mmtt"));
                time = time.AddMinutes(30);
            }

            return new SelectList(dateTimes, "12:00PM");
        }
    }
}
