using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;
using JustGiving.Api.Sdk;
using Raven.Client;
using System.Web.Routing;
using System.Web;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Services;
using TicketMuffin.PayPal.Clients;
using TicketMuffin.PayPal.Model;
using TicketMuffin.Web.Models;
using log4net;

namespace TicketMuffin.Web.Controllers
{
    public class CreateEventController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IFormsAuthenticationService _formsAuthenticationService;
        private readonly IEventService _eventService;
        private readonly ICountryService _countryService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IIdentity _userIdentity;
        private readonly IPayPalApiClient _payPalApiClient;
        private readonly IDocumentSession _ravenSession;
        Regex mustContainAlphaCharacters = new Regex(@".*\w.*");
        private readonly ILog _logger = LogManager.GetLogger(typeof (CreateEventController));

        public CreateEventController(IAccountService accountService, ICountryService countryService, IAuthenticationService authenticationService, 
            IFormsAuthenticationService formsAuthenticationService, IEventService eventService, IIdentity userIdentity, 
            IPayPalApiClient payPalApiClient,
            IDocumentSession ravenSession)
        {
            _accountService = accountService;
            _countryService = countryService;
            _authenticationService = authenticationService;

            _formsAuthenticationService = formsAuthenticationService;
            _eventService = eventService;
            _userIdentity = userIdentity;

            _payPalApiClient = payPalApiClient;
            _ravenSession = ravenSession;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("check-url-availability")]
        public string CheckShortUrlAvailability(string shortUrl)
        {
            
            if (!mustContainAlphaCharacters.IsMatch(shortUrl))
            {
                return "invalid";
            }

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

            viewModel.Countries = new SelectList(_ravenSession.Query<Country>().Take(600).ToList(), "Name", "Name", "United Kingdom");

            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("create")]
        [Authorize]
        public ActionResult EventDetails(CreateEventRequest request)
        {
            DateTime startDate = new DateTime();
            Image uploadedImage = null;
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
                ModelState.AddModelError("startDateTime", "The date provided isn't valid because it's in the past");
            }
            if (string.IsNullOrWhiteSpace(request.ShortUrl) || !mustContainAlphaCharacters.IsMatch(request.ShortUrl))
            {
                ModelState.AddModelError("ShortUrl",
                                         "Please select a url containing at least 1 alphabetical character (a-z)");
            }
            else if (!_eventService.ShortUrlAvailable(request.ShortUrl))
            {
                ModelState.AddModelError("ShortUrl", "Unfortunately that url is already in use");
            }

            // pull any files out of the request
            if (Request != null && Request.Files != null)
            {
                if (Request.Files.Count > 0)
                {
                    var file = Request.Files[0];
                    if (file != null && file.ContentLength > 0)
                    {
                        byte[] fileData = new byte[file.ContentLength];

                        file.InputStream.Read(fileData, 0, file.ContentLength);
                        MemoryStream imageByteStream = new MemoryStream(fileData);
                        // try to load as an image
                        try
                        {
                            uploadedImage = Bitmap.FromStream(imageByteStream);
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("ImageFilename", "Please choose an image file to upload");
                        }
                    }
                }
            }

            if (request.ForCharity)
            {
                if(!request.CharityId.HasValue)
                {
                    ModelState.AddModelError("CharityId", "Please select a charity from the suggestions provided");
                }
                else
                {
                    JustGivingClient apiClient = new JustGivingClient(JustGivingApiConfigurationFactory.Build());
                    try
                    {
                        var charityDetails = apiClient.Charity.Retrieve(request.CharityId.Value);
                        request.CharityLogoUrl = "http://www.justgiving.com/"+charityDetails.LogoUrl;
                        request.CharityName = charityDetails.Name;
                        request.CharityRegistrationNumber = charityDetails.RegistrationNumber;
                        request.CharityDonationPageUrl = string.Format("http://www.justgiving.com/{0}/donate", charityDetails.PageShortName);
                        request.CharityDescription = charityDetails.Description;
                        request.CharityDonationGatewayName = "JustGiving";
                    } catch (Exception exception)
                    {
                        // probably justgiving api is down
                        _logger.Warn("Justgiving api seems to be down", exception);
                        // let the user think it's their fault
                        ModelState.AddModelError("CharityId", "Please select a charity from the suggestions provided");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                request.StartDateTime = DateTime.Now;
                request.StartTimes = TimeOptions();
                
                request.Countries = new SelectList(_countryService.RetrieveAllCountries(), "Name", "Name", "United Kingdom");
                return View(request);
            }

            if (uploadedImage != null)
            {
                // save the uploaded image
                // todo: should resize the image here
                string imagePath = string.Format(ConfigurationManager.AppSettings["EventImagePathFormat"],
                                                 request.ShortUrl);
                if (imagePath.StartsWith("~") || imagePath.StartsWith("/"))
                    imagePath = HostingEnvironment.MapPath(imagePath);
                if (!Directory.Exists(Path.GetDirectoryName(imagePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(imagePath));

                uploadedImage.Save(imagePath, ImageFormat.Jpeg);
                request.ImageFilename = imagePath;
                request.ImageUrl =
                    Url.Content(string.Format(ConfigurationManager.AppSettings["EventImageUrlFormat"], request.ShortUrl));
            }

            var account = _accountService.RetrieveByEmailAddress(_userIdentity.Name);
            request.OrganiserAccountId = account.Id;
            request.OrganiserName = account.FirstName + " " + account.LastName;
            var result = _eventService.CreateEvent(request);

            if (result.Success)
            {
                _ravenSession.SaveChanges();
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
            var account = _accountService.RetrieveByEmailAddress(_userIdentity.Name);
            var @event = _eventService.Retrieve(shortUrl);

            if (@event == null)
            {
                _logger.Error("Couldn't find event "+ shortUrl);
                return HttpNotFound();
            }

            SetPayPalDetailsIfMissing(account);

            var viewModel = new SetTicketDetailsRequest();
            viewModel.SalesEndDateTime = @event.StartDate.AddDays(-1);
            viewModel.SalesEndTimeOptions = TimeOptions();
            viewModel.PayPalEmail = account.PaymentGatewayId;
            viewModel.PayPalFirstName = account.PayPalFirstName;
            viewModel.PayPalLastName = account.PayPalLastName;
            return View(viewModel);
        }

        private void SetPayPalDetailsIfMissing(Account account)
        {
            if (string.IsNullOrWhiteSpace(account.PaymentGatewayId))
                account.PaymentGatewayId = account.Email;
            if (string.IsNullOrWhiteSpace(account.PayPalFirstName))
                account.PayPalFirstName = account.FirstName;
            if (string.IsNullOrWhiteSpace(account.PayPalLastName))
                account.PayPalLastName = account.LastName;
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
            GetVerifiedStatusResponse accountVerification = new GetVerifiedStatusResponse();
            try
            {
                GetVerifiedStatusRequest verifyAccountRequest=new GetVerifiedStatusRequest(_payPalApiClient.Configuration)
                                                                  {
                                                                      EmailAddress=setTicketDetailsRequest.PayPalEmail,
                                                                      FirstName=setTicketDetailsRequest.PayPalFirstName,
                                                                      LastName=setTicketDetailsRequest.PayPalLastName
                                                                  };
                accountVerification = _payPalApiClient.Accounts.VerifyAccount(verifyAccountRequest);
            } catch (Exception ex)
            {
                // TODO: be more specific about the exception
                _logger.Error(ex);
                //ModelState.AddModelError("PayPalEmail", ex.Message);
            }

            if (!accountVerification.Verified)
            {
                //ModelState.AddModelError("PayPalEmail", "A PayPal account matching the credentials your provided could not be found");
            } 
            

            if (!ModelState.IsValid)
            {
                setTicketDetailsRequest.Times = TimeOptions();
                setTicketDetailsRequest.SalesEndTimeOptions = TimeOptions();
                return View(setTicketDetailsRequest);
            }

            _eventService.SetTicketDetails(setTicketDetailsRequest);

            var account = _accountService.RetrieveByEmailAddress(_userIdentity.Name);
            account.PaymentGatewayId = setTicketDetailsRequest.PayPalEmail;
            account.PayPalFirstName = setTicketDetailsRequest.PayPalFirstName;
            account.PayPalLastName = setTicketDetailsRequest.PayPalLastName;

            

            return RedirectToAction("Index", "Event", new { shortUrl = @event.ShortUrl });
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

        public ActionResult FindCharities(string query)
        {
            JustGiving.Api.Sdk.Model.Search.CharitySearchResults charityResults = null;

            try
            {
                charityResults = new JustGiving.Api.Sdk.JustGivingClient(JustGivingApiConfigurationFactory.Build())
                    .Search.CharitySearch(query);
            } catch (Exception ex)
            {
                _logger.Warn("justgiving api seems to be down", ex);
                return PartialView(new CharitySearchResults() {Results = new List<CharitySearchResult>()});
            }

            var viewModel = new CharitySearchResults()
                                {
                                    Results = charityResults
                                        .Results.Select(c => new CharitySearchResult()
                                                                 {
                                                                     Id = c.CharityId,
                                                                     Name = c.Name,
                                                                     LogoUrl = "http://www.justgiving.com" + c.LogoFileName.Replace("height=120", "height=30").Replace("&width=120", "")
                                                                 })
                                                                 .Take(5)
                                                                 .ToList()
                                };

            return PartialView(viewModel);
    }
    }

    public class CharitySearchResults
    {
        public List<CharitySearchResult> Results { get; set; }
    }

    public static class JustGivingApiConfigurationFactory
    {
        public static ClientConfiguration Build()
        {
            return new ClientConfiguration(
                ConfigurationManager.AppSettings["JustGivingApiDomainBase"],
                ConfigurationManager.AppSettings["JustGivingApiKey"],
                int.Parse(ConfigurationManager.AppSettings["JustGivingApiVersion"]));
        }
    }

    public class CharitySearchResult
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string LogoUrl { get; set; }
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
