using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using EmailProcessing;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using GroupGiving.Web.Models;
using Raven.Client;

namespace GroupGiving.Web.Areas.Admin.Controllers
{
    public class DiagnosticsController : Controller
    {
        //
        // GET: /Diagnostics/
        IEmailFacade _emailFacade = null;
        private readonly IDocumentStore _storage;
        private readonly ICountryService _countryService;
        private IRepository<GroupGivingEvent> _eventRepository=null;

        public DiagnosticsController(IRepository<GroupGivingEvent> eventRepository, IEmailFacade emailFacade, IDocumentStore storage, ICountryService countryService)
        {
            _eventRepository = eventRepository;
            _emailFacade = emailFacade;
            _storage = storage;
            _countryService = countryService;
            _emailFacade.LoadTemplates();
        }

        public ActionResult Index()
        {
            var viewModel = new DiagnosticsDashboardViewModel();
            using (var session = _storage.OpenSession())
            {
                viewModel.CountryCount = session.Query<Country>().Count();
            }
            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SendTest(string toAddress, string emailTemplate)
        {
            var account = new Account()
            {
                FirstName = "Andrew",
                LastName = "Myhre",
                Email = "andrew.myhre@gmail.com"
            };
            switch (emailTemplate)
            {
                case "EventActivated":
                    var @event = CreateSampleEvent();
                    _emailFacade.Send(toAddress, emailTemplate, new {Event=@event, Pledge = @event.Pledges.First()});
                    break;
                case "AccountCreated":
                    _emailFacade.Send(toAddress, emailTemplate, new {Account=account, AccountPageUrl="http://somedomain.com/YourAccount"});
                    break;
                case "GetYourAccountStarted":
                    _emailFacade.Send(toAddress, emailTemplate, new { Account = account, CompleteAccountUrl = "http://somedomain.com/CompleteYourAccount" });
                    break;
                case "ResetYourPassword":
                    _emailFacade.Send(toAddress, emailTemplate, new { Account = account, ResetPasswordUrl = "http://somedomain.com/ResetYourPassword" });
                    break;
            }

            return Json("Ok");
        }

        private GroupGivingEvent CreateSampleEvent()
        {
            return new GroupGivingEvent()
                       {
                           Title = "sample event",
                           Description = "just a sample",
                           AddressLine = "1 sample lane",
                           City = "samplecity",
                           Postcode = "N1Test",
                           Country="United Kingdom",
                           MinimumParticipants = 10,
                           MaximumParticipants = 100,
                           TicketPrice = 10,
                           AdditionalBenefits = "additional benefits are increased labido and a profound sense of your place in the world",
                           OrganiserName = "Mr Sample Man",
                           PayPalAccountFirstName = "Not",
                           PayPalAccountLastName = "Real",
                           PaypalAccountEmailAddress = "fake.paypal.account@paypal.com",
                           SalesEndDateTime = DateTime.Now.AddDays(1),
                           ShortUrl = "sample-event",
                           State = EventState.SalesReady,
                           Venue = "Some Fake Place",
                           Pledges = CreateSamplePledges()
                       };
        }

        private List<EventPledge> CreateSamplePledges()
        {
            return new List<EventPledge>()
                       {
                           new EventPledge()
                                     {
                                         AccountEmailAddress = "fake-email-address@ticketmuffin.com",
                                         AccountId="0",
                                         AccountName="Not a real account",
                                         Attendees = new List<EventPledgeAttendee>(){ new EventPledgeAttendee("Joe"), new EventPledgeAttendee("Billy")},
                                         DatePledged = DateTime.Now.AddDays(-1),
                                         OrderNumber="NOT-A-REAL-PLEDGE",
                                         Paid=true,
                                         PaymentStatus = PaymentStatus.PaidPendingReconciliation,
                                         SubTotal = 10,
                                         Total = 10,
                                         TransactionId = "FAKETRANSACTION"
                                     }
                       };
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string LoadCountries()
        {
            Response.Write("loading countries...<br/>");
            try
            {
                string sourceFilePath = HostingEnvironment.MapPath("~/App_Data/countrylist.csv");
                using (var session = _storage.OpenSession())
                {
                    var loaded = _countryService.LoadCountriesFromCsv(session, sourceFilePath);

                    foreach(var country in loaded)
                    {
                        Response.Write(string.Format("<p>{0}</p>", country.Name));
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Write("failed<br/>");
                Response.Write(ex.Message);
                Response.Write(ex.StackTrace);
            }

            return "finished";
        }
    }
}