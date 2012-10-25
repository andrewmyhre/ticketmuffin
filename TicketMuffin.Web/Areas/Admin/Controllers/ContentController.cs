using System.Linq;
using System.Text;
using System.Web.Mvc;
using Raven.Client;
using Raven.Client.Linq;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Services;

namespace TicketMuffin.Web.Areas.Admin.Controllers
{
    [ValidateInput(false)]
    public class ContentController : Controller
    {
        private readonly IDocumentSession _session;

        public ContentController(IDocumentSession session)
        {
            _session = session;
        }

        //
        // GET: /Admin/Content/

        public ActionResult Index()
        {
            var viewModel = new PageListViewModel();
            viewModel.Pages = _session.Query<PageContent>();


            return View(viewModel);
        }

        public ActionResult Migrate()
        {
            _session.Advanced.MaxNumberOfRequestsPerSession = 1000;
            StringBuilder log = new StringBuilder();

            int tryint;
            var allPages = _session.Query<PageContent>().ToList()
                .Where(p => int.TryParse(p.Id.Substring("pagecontents/".Length), out tryint));
            foreach(var page in allPages)
            {
                var contentProvider = new RavenDbContentProvider(_session);
                foreach(var content in page.Content)
                {
                    foreach(var culture in content.ContentByCulture)
                    {
                        PageContent pageContent;
                        string contentLabel;
                        contentProvider.GetContent(page.Address, content.Label, culture.Value, culture.Culture,
                                                    out pageContent, out contentLabel);

                        log.AppendFormat("<p>migrated {0}->{1}<br/>{2}.{3}.{4}</p>", page.Id, pageContent.Id, page.Address, content.Label, culture.Culture);
                    }
                }
                _session.Delete(page);
            }

            return new ContentResult(){Content=log.ToString(), ContentType = "text/html"};
        }

        public ActionResult ViewPageContent(string id)
        {
            var viewModel = new PageViewModel();
            viewModel.Page = _session.Load<PageContent>("PageContents/"+id);
            return View(viewModel);
        }

        public ActionResult ViewTranslations(string id, string contentLabel)
        {
            var viewModel = new ContentTranslationViewModel();
            viewModel.Page = _session.Load<PageContent>("PageContents/" + id);
            viewModel.Content = viewModel.Page.Content.SingleOrDefault(c => c.Label == contentLabel);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateTranslations(string pageId, string contentLabel)
        {
            var page = _session.Load<PageContent>("PageContents/" + pageId);
            var content = page.Content.SingleOrDefault(c => c.Label == contentLabel);

            foreach(var localContent in content.ContentByCulture)
            {
                localContent.Value = Request.Form[localContent.Culture] ?? "";
            }

            _session.SaveChanges();

            return RedirectToAction("ViewTranslations", new {id = pageId, contentLabel = contentLabel});
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddTranslation(string pageId, string contentLabel, string cultureKey, string content)
        {
            var page = _session.Load<PageContent>("PageContents/" + pageId);
            var pageContent = page.Content.SingleOrDefault(c => c.Label == contentLabel);
            var localContent = pageContent.ContentByCulture.SingleOrDefault(lc => lc.Culture == cultureKey);
            if (localContent != null)
            {
                localContent.Value = content;
            }
            else
            {
                pageContent.ContentByCulture.Add(new LocalisedContent(){Culture = cultureKey, Value = content});
            }
            _session.SaveChanges();

            return RedirectToAction("ViewTranslations", new {id = pageId, contentLabel = contentLabel});
        }

        public ActionResult ClearAll()
        {
            var pages = _session.Query<PageContent>();
            foreach(var page in pages)
            {
                _session.Delete(page);
            }
            _session.SaveChanges();

            return RedirectToAction("Index");
        }
    }

    public class ContentTranslationViewModel    
    {
        public PageContent Page { get; set; }

        public ContentDefinition Content { get; set; }
    }

    public class PageViewModel
    {
        public PageContent Page { get; set; }
    }

    public class PageListViewModel
    {
        public IRavenQueryable<PageContent> Pages { get; set; }
    }
}
