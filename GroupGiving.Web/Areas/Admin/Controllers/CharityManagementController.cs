using System.Linq;
using System.Web.Mvc;
using GroupGiving.Core.Domain;
using Raven.Client;

namespace GroupGiving.Web.Areas.Admin.Controllers
{
    public class CharityManagementController : Controller
    {
        private readonly IDocumentStore _documentStore;

        public CharityManagementController(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public ActionResult Index()
        {
            using (var session = _documentStore.OpenSession())
            {
                var allCharities = session.Query<Charity>()
                    .OrderBy(c => c.Name)
                    .Take(1024)
                    .ToList();

                return View(allCharities);
            }
        }
    }
}