using System.Dynamic;
using System.Linq;
using System.Web.Mvc;
using Raven.Client;
using TicketMuffin.Web.Models;

namespace TicketMuffin.Web.Areas.Admin.Controllers
{
    public class PledgeController : Controller
    {
        private readonly IDocumentSession _documentSession;

        public PledgeController(IDocumentSession documentSession)
        {
            _documentSession = documentSession;
        }

        //
        // GET: /Admin/Pledge/

        public ActionResult Search(string q)
        {
            var query = _documentSession.Advanced.LuceneQuery<TransactionHistoryItem>("pledges");
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
