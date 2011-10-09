using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Com.StellmanGreene.CSVReader;
using EmailProcessing;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Core.Services;
using GroupGiving.Web.App_Start;
using GroupGiving.Web.Models;
using Ninject;
using Raven.Client;

namespace GroupGiving.Web.Controllers
{
    public class DiagnosticsController : Controller
    {
        //
        // GET: /Diagnostics/
        IEmailFacade _emailFacade = null;
        private readonly IDocumentStore _storage;
        private IRepository<GroupGivingEvent> _eventRepository=null;

        public DiagnosticsController(IRepository<GroupGivingEvent> eventRepository, IEmailFacade emailFacade, IDocumentStore storage)
        {
            _eventRepository = eventRepository;
            _emailFacade = emailFacade;
            _storage = storage;
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
                using (var filestream = System.IO.File.OpenRead(HostingEnvironment.MapPath("~/App_Data/countrylist.csv")))
                using (var session = _storage.OpenSession())
                using (var reader = new StreamReader(filestream))
                {

                    var countries = session.Query<Country>().ToList();
                    foreach(var country in countries)
                    {
                        session.Delete(country);
                    }
                    session.SaveChanges();

                    CSVReader csv = new CSVReader(reader);
                    var table = csv.CreateDataTable(true);
                    foreach (DataRow row in table.Rows)
                    {
                        var country = new Country((string) row["Common Name"]);
                        session.Store(country);
                        Response.Write((string)row["Common Name"] + "<br/>");
                        Response.Flush();
                    }
                    session.SaveChanges();
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
