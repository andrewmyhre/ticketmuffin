using System.Linq;
using System.Web.Mvc;
using Raven.Client;
using Raven.Client.Linq;
using TicketMuffin.Core.Domain;

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
            viewModel.Pages = _session.Query<PageContent>().OrderBy(p=>p.Id);


            return View(viewModel);
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


        public ActionResult DeleteTranslation(string id, string contentLabel, string culture)
        {
            var page = _session.Load<PageContent>(id);
            if (page == null)
            {
                return HttpNotFound();
            }
            var contentDefinition = page.Content.SingleOrDefault(c => c.Label == contentLabel);
            if (contentDefinition == null)
            {
                return HttpNotFound();
            }

            var localisedContent = contentDefinition.ContentByCulture.SingleOrDefault(lc => lc.Culture.Equals(culture));
            contentDefinition.ContentByCulture.Remove(localisedContent);

            return RedirectToAction("ViewTranslations", new { id = id, contentLabel = contentLabel });
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
