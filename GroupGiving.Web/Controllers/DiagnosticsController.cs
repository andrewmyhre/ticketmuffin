using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EmailProcessing;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using Ninject;

namespace GroupGiving.Web.Controllers
{
    public class DiagnosticsController : Controller
    {
        //
        // GET: /Diagnostics/
        IEmailFacade _emailFacade = null;
        private IRepository<GroupGivingEvent> _eventRepository=null;

        public DiagnosticsController()
        {
            _eventRepository = MvcApplication.Kernel.Get<IRepository<GroupGivingEvent>>();
            _emailFacade = MvcApplication.Kernel.Get<IEmailFacade>();
            _emailFacade.LoadTemplates();
        }

        public ActionResult Index()
        {
            return View();
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

    }
}
