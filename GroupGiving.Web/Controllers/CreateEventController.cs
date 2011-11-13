using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Security;
using GroupGiving.Core.Services;
using GroupGiving.PayPal;
using GroupGiving.PayPal.Configuration;
using GroupGiving.PayPal.Model;
using GroupGiving.Web.App_Start;
using GroupGiving.Web.Areas.Api.Controllers;
using GroupGiving.Web.Code;
using Ninject;
using Raven.Client;
using RavenDBMembership.Web.Models;
using System.Web.Routing;
using System.Web;

namespace GroupGiving.Web.Controllers
{
    public class CreateEventController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IMembershipService _membershipService;
        private readonly IFormsAuthenticationService _formsAuthenticationService;
        private readonly IEventService _eventService;
        private readonly ICountryService _countryService;
        private readonly IIdentity _userIdentity;
        private readonly IDocumentStore _storage;
        private readonly IMembershipProviderLocator _membershipProviderLocator;
        private readonly IPaypalAccountService _paypalAccountService;

        public CreateEventController(IAccountService accountService, ICountryService countryService, IMembershipService membershipService, 
            IFormsAuthenticationService formsAuthenticationService, IEventService eventService, 
            IDocumentStore documentStore, IIdentity userIdentity, IDocumentStore storage, IMembershipProviderLocator membershipProviderLocator,
            IPaypalAccountService paypalAccountService)
        {
            _accountService = accountService;
            _countryService = countryService;
            _membershipService = membershipService;
            _formsAuthenticationService = formsAuthenticationService;
            _eventService = eventService;
            _userIdentity = userIdentity;
            _storage = storage;
            
            _membershipProviderLocator = membershipProviderLocator;
            _paypalAccountService = paypalAccountService;

            if (_membershipProviderLocator.Provider() is RavenDBMembership.Provider.RavenDBMembershipProvider)
            {
                ((RavenDBMembership.Provider.RavenDBMembershipProvider) _membershipProviderLocator.Provider()).
                    DocumentStore = documentStore;
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("check-url-availability")]
        public string CheckShortUrlAvailability(string shortUrl)
        {
            var uriString = string.Format("{0}://{1}/{2}",
                                          Request.Url.Scheme, Request.Url.Authority,
                                          shortUrl);
            var uri = new Uri(uriString);
            var routeData = new RouteInfo(uri, Request.ApplicationPath);

            return routeData == null ? "available" : "reserved";
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("create")]
        [Authorize]
        public ActionResult EventDetails()
        {
            var viewModel = new CreateEventRequest();
            viewModel.StartDateTime = DateTime.Now;
            viewModel.StartTimes = TimeOptions();

            using (var session = _storage.OpenSession())
            {
                viewModel.Countries = new SelectList(session.Query<Country>().Take(600).ToList(), "Name", "Name", "United Kingdom");
            }

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
                
                request.Countries = new SelectList(_countryService.RetrieveAllCountries(), "Name", "Name", "United Kingdom");
                return View(request);
            }

            var account = _accountService.RetrieveByEmailAddress(_userIdentity.Name);
            request.OrganiserAccountId = account.Id;
            request.OrganiserName = account.FirstName + " " + account.LastName;
            var result = _eventService.CreateEvent(request);
            
            if (result.Success)
            {
                return RedirectToRoute("CreateEvent_TicketDetails", new { shortUrl = result.Event.ShortUrl});
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
        public ActionResult TicketDetails(string shortUrl)
        {
            var membershipUser = _membershipService.GetUser(_userIdentity.Name);
            var account = _accountService.RetrieveByEmailAddress(membershipUser.Email);

            var viewModel = new SetTicketDetailsRequest();
            viewModel.SalesEndDateTime = DateTime.Now;
            viewModel.SalesEndTimeOptions = TimeOptions();
            viewModel.PayPalEmail = account.PayPalEmail;
            viewModel.PayPalFirstName = account.PayPalFirstName;
            viewModel.PayPalLastName = account.PayPalLastName;

            // sandbox details
            viewModel.PayPalEmail = "sellr2_1304843519_biz@gmail.com";
            viewModel.PayPalFirstName = "Andrew";
            viewModel.PayPalLastName = "Myhre";
            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("tickets")]
        [Authorize]
        public ActionResult TicketDetails(SetTicketDetailsRequest setTicketDetailsRequest)
        {
            var @event = _eventService.Retrieve(setTicketDetailsRequest.ShortUrl);
            if (@event == null)
            {
                return RedirectToAction("create");
            }
            if (setTicketDetailsRequest.MaximumParticipants < setTicketDetailsRequest.MinimumParticipants)
            {
                ModelState.AddModelError("MaximumParticipants", "Maximum participants can't be less than the minimum");
            }

            DateTime salesEndDateTime = new DateTime();
            if (!DateTime.TryParse(setTicketDetailsRequest.SalesEndDate + " " + setTicketDetailsRequest.SalesEndTime, out salesEndDateTime))
            {
                ModelState.AddModelError("SalesEndDateTime", "Provide a valid date and time for ticket sales to end");
            }
            else if (salesEndDateTime < DateTime.Now)
            {
                ModelState.AddModelError("SalesEndDateTime", "The date provided for sales to end is in the past, it must be in the future");
            }
            else
            {
                setTicketDetailsRequest.SalesEndDateTime = salesEndDateTime;
            }

            // paypal verification
            var paypalVerificationRequest = new VerifyPaypalAccountRequest()
                {Email = setTicketDetailsRequest.PayPalEmail, FirstName = setTicketDetailsRequest.PayPalFirstName, LastName = setTicketDetailsRequest.PayPalLastName};
            var accountVerification = _paypalAccountService.VerifyPaypalAccount(paypalVerificationRequest);
            if (!accountVerification.Success)
            {
                ModelState.AddModelError("PayPalEmail", "A PayPal account matching the credentials your provided could not be found");
            } 
            

            if (!ModelState.IsValid)
            {
                setTicketDetailsRequest.Times = TimeOptions();
                setTicketDetailsRequest.SalesEndTimeOptions = TimeOptions();
                return View(setTicketDetailsRequest);
            }

            _eventService.SetTicketDetails(setTicketDetailsRequest);

            var membershipUser = _membershipService.GetUser(_userIdentity.Name);
            var account = _accountService.RetrieveByEmailAddress(membershipUser.Email);
            if (string.IsNullOrWhiteSpace(account.PayPalEmail) && string.IsNullOrWhiteSpace(account.PayPalFirstName) && string.IsNullOrWhiteSpace(account.PayPalEmail))
            {
                account.PayPalEmail = setTicketDetailsRequest.PayPalEmail;
                account.PayPalFirstName = setTicketDetailsRequest.PayPalFirstName;
                account.PayPalLastName = setTicketDetailsRequest.PayPalLastName;
                _accountService.UpdateAccount(account);
            }

            

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

    public class RouteInfo
    {
        public RouteInfo(Uri uri, string applicationPath)
        {
            RouteData = RouteTable.Routes.GetRouteData(new InternalHttpContext(uri, applicationPath));
        }

        public RouteData RouteData { get; private set; }

        private class InternalHttpContext : HttpContextBase
        {
            private readonly HttpRequestBase _request;

            public InternalHttpContext(Uri uri, string applicationPath)
                : base()
            {
                _request = new InternalRequestContext(uri, applicationPath);
            }

            public override HttpRequestBase Request { get { return _request; } }
        }

        private class InternalRequestContext : HttpRequestBase
        {
            private readonly string _appRelativePath;
            private readonly string _pathInfo;

            public InternalRequestContext(Uri uri, string applicationPath)
                : base()
            {
                _pathInfo = uri.Query;

                if (String.IsNullOrEmpty(applicationPath) || !uri.AbsolutePath.StartsWith(applicationPath,
                    StringComparison.OrdinalIgnoreCase))
                    _appRelativePath = uri.AbsolutePath.Substring(applicationPath.Length);
                else
                    _appRelativePath = uri.AbsolutePath;
            }

            public override string AppRelativeCurrentExecutionFilePath
            {
                get
                {
                    return String.Concat("~",
                        _appRelativePath);
                }
            }
            public override string PathInfo { get { return _pathInfo; } }
        }
    }
}
