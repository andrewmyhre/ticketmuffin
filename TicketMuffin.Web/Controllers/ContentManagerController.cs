using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core.Domain;
using Raven.Client;

namespace GroupGiving.Web.Controllers
{
    public class ContentSearch
    {
        public string Id { get; set; }
        public string Address { get; set; }
        public string Label { get; set; }
        public string Key { get; set; }
        public string Content { get; set; }
    }

    public class ContentManagerController : Controller
    {
        private readonly IDocumentSession _session;

        public ContentManagerController(IDocumentSession session)
        {
            _session = session;
        }

        //
        // GET: /Content/

        public ActionResult Index(string q)
        {
            var viewModel = new ContentListViewModel();
            if (!string.IsNullOrWhiteSpace(q))
            {
                // load items from index
                viewModel.Pages = _session.Advanced.LuceneQuery<PageContent>("contentSearch")
                    .WhereContains("Content", q)
                    .OrElse().WhereContains("Label", q)
                    .OrElse().WhereContains("Address", q);
                    
            } else
            {
                viewModel.Pages = _session.Query<PageContent>().Take(1024);
            }
            viewModel.Query = q;
            return View(viewModel);
        }

        public ActionResult DeletePage(string id)
        {
            var pageContent = _session.Load<PageContent>("pagecontents/" + id);
            if (pageContent != null)
            {
                _session.Delete(pageContent);
                _session.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UpdateContentDefinition(int pageId, string contentLabel, string culture, string content)
        {
            var page = _session.Load<PageContent>("pagecontents/" + pageId);
            if (page==null)
            {
                return HttpNotFound();
            }

            var contentDefinition = page.Content.Where(cd => cd.Label == contentLabel).FirstOrDefault();
            if (contentDefinition==null)
            {
                return HttpNotFound();
            }

            var localContent = contentDefinition.ContentByCulture.SingleOrDefault(lc => lc.Culture == culture);
            if (localContent != null)
            {
                localContent.Value = content;
            } else
            {
                contentDefinition.ContentByCulture.Add(new LocalisedContent(){Culture = culture, Value = content});
            }
            _session.SaveChanges();

            return RedirectToAction("Index");
        }

    }

    public class ContentListViewModel
    {
        public IEnumerable<PageContent> Pages { get; set; }

        public string Query { get; set; }
    }
}
