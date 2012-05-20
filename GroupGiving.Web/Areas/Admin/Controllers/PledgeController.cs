using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core.Domain;
using GroupGiving.Web.App_Start;
using GroupGiving.Web.Areas.Admin.Models;
using GroupGiving.Web.Models;
using Raven.Client;
using Raven.Client.Linq;

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

        public ActionResult Search(string q)
        {
            using (var session = _documentStore.OpenSession())
            {
                var query = session.Advanced.LuceneQuery<TransactionHistoryItem>("pledges");
                if (!string.IsNullOrWhiteSpace(q))
                {
                    query = query.OpenSubclause()
                        .WhereStartsWith("TransactionId", q)
                        .OrElse().WhereStartsWith("OrderNumber", q)
                        .OrElse().WhereContains("AccountEmailAddress", q).Fuzzy(0.5m)
                        .OrElse().WhereContains("AttendeeName", q).Fuzzy(0.8m)
                        .OrElse().WhereContains("EventName", q).Fuzzy(0.8m)
                        .OrElse().WhereContains("EventOrganiser", q).Fuzzy(0.8m)
                        .CloseSubclause();
                }

                dynamic viewModel = new ExpandoObject();
                viewModel.Pledges = query.ToList();

                return View(viewModel);
            }

        }
    }
}
