using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using EmailProcessing;
using Raven.Client;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Payments;
using TicketMuffin.Core.Services;
using TicketMuffin.PayPal.Clients;
using TicketMuffin.PayPal.Model;
using TicketMuffin.Web.Models;

namespace TicketMuffin.Web.Areas.Admin.Controllers
{
    public class DiagnosticsController : Controller
    {
        //
        // GET: /Diagnostics/
        IEmailFacade _emailFacade = null;
        private readonly IDocumentSession _documentSession;
        private readonly ICountryService _countryService;
        private readonly IPayPalApiClient _payPalApiClient;
        private readonly IPayRequestFactory _payRequestFactory;

        public DiagnosticsController(IEmailFacade emailFacade, IDocumentSession documentSession, ICountryService countryService,
            IPayPalApiClient payPalApiClient,
            IPayRequestFactory payRequestFactory)
        {
            _emailFacade = emailFacade;
            _documentSession = documentSession;
            _countryService = countryService;
            _payPalApiClient = payPalApiClient;
            _payRequestFactory = payRequestFactory;
            _emailFacade.LoadTemplates();
        }

        public ActionResult Index()
        {
            var viewModel = new DiagnosticsDashboardViewModel();
            viewModel.CountryCount = _documentSession.Query<Country>().Count();
            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SendTest(string toAddress, string emailTemplate, string culture)
        {
            var account = new Account()
            {
                FirstName = "Andrew",
                LastName = "Myhre",
                Email = "andrew.myhre@gmail.com"
            };
            var @event = CreateSampleEvent();
            switch (emailTemplate)
            {
                case "EventActivated":
                    _emailFacade.Send(toAddress, emailTemplate, new {Event=@event, Pledge = @event.Pledges.First()});
                    break;
                case "PledgeConfirmation":
                    _emailFacade.Send(toAddress, emailTemplate, new { Event = @event, Pledge = @event.Pledges.First(), AccountPageUrl = "http://somedomain.com/YourAccount" });
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
                                         Payments = new List<Payment>(new[]{new Payment(){PaymentStatus = PaymentStatus.Unsettled, TransactionId = "FAKETRANSACTION"}}),
                                         SubTotal = 10,
                                         Total = 10,
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
                var loaded = _countryService.LoadCountriesFromCsv(_documentSession, sourceFilePath);

                foreach(var country in loaded)
                {
                    Response.Write(string.Format("<p>{0}</p>", country.Name));
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

        public ActionResult AuthenticatePayPalApi()
        {
            PayResponse response = null;

            try
            {
                var receivers = new TicketMuffin.PayPal.Model.Receiver[]
                                                {
                                                    new TicketMuffin.PayPal.Model.Receiver("5", "andrew.myhre@gmail.com", false)
                                                };
                var payRequest = _payRequestFactory.RegularPayment("GBP", receivers, "diagnostics " + DateTime.Now.ToString());
                
                response = _payPalApiClient.Payments.SendPayRequest(payRequest);
            }
            catch (HttpChannelException exception)
            {
                response = new PayResponse()
                               {
                                   Raw = exception.FaultMessage.Raw,
                                   responseEnvelope = new PayResponseResponseEnvelope()
                               };
            }

            XDocument xml = null;
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings xmlsettings=new XmlWriterSettings()
                                              {
                                                  Indent = true,
                                                  NewLineHandling = NewLineHandling.Entitize,
                                                  NewLineOnAttributes = true
                                              };
            using (XmlWriter writer = XmlWriter.Create(sb, xmlsettings))
            {
                xml = XDocument.Parse(response.Raw.Request);
                xml.WriteTo(writer);
                writer.Flush();
                response.Raw.Request = sb.ToString();
            }
            sb.Clear();

            using (XmlWriter writer = XmlWriter.Create(sb, xmlsettings))
            {
                xml = XDocument.Parse(response.Raw.Response);
                xml.WriteTo(writer);
                writer.Flush();
                response.Raw.Response = sb.ToString();
            }

            return View(response);

        }
    }
}
