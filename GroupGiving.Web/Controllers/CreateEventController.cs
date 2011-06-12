using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using GroupGiving.Web.Code;
using GroupGiving.Web.Models;
using Ninject;
using RavenDBMembership.Provider;
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
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("create")]
        [Authorize]
        public ActionResult EventDetails(CreateEventRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var result = _eventService.CreateEvent(request);

            if (result.Success)
            {
                return RedirectToRoute("CreateEvent_TicketDetails", new {id=result.EventId});
            }

            ModelState.AddModelError("createevent", "There was a problem with the information you provided");
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("tickets")]
        [Authorize]
        public ActionResult TicketDetails()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("tickets")]
        [Authorize]
        public ActionResult TicketDetails(SetTicketDetailsRequest setTicketDetailsRequest)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            _eventService.SetTicketDetails(setTicketDetailsRequest);

            return RedirectToRoute("Event_ShareYourEvent", new {id=setTicketDetailsRequest.EventId});
        }
    }
}
