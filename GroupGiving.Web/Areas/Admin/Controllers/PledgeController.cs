using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core.Domain;
using GroupGiving.Web.Areas.Admin.Models;
using Raven.Client;

namespace GroupGiving.Web.Areas.Admin.Controllers
{
    public class PledgeController : Controller
    {
        private readonly IDocumentStore _documentStore;

        public PledgeController(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        //
        // GET: /Admin/Pledge/

        public ActionResult Index()
        {
            dynamic viewModel = new {};

            return View(viewModel);
        }

        public ActionResult Search(string q)
        {
            using (var session = _documentStore.OpenSession())
            {

                dynamic viewModel = new ExpandoObject();
                viewModel.Pledges = session.Query<EventPledge>()
                    .Where(p => p.TransactionId.Equals(q)
                                || p.OrderNumber.Equals(q)
                                || p.AccountName.Contains(q)
                                || p.Attendees.Any(a => a.FullName.Contains(q)))
                    .ToList();

                return View(viewModel);
            }

        }
    }
}
