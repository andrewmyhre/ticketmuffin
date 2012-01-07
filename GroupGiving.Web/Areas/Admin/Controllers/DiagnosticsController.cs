using System;
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
                    var @event = _eventRepository.Query(e=>e != null).FirstOrDefault();
                    var pledge = @event.Pledges.First();
                    _emailFacade.Send(toAddress, emailTemplate, new {Event=@event, Pledge = pledge});
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
