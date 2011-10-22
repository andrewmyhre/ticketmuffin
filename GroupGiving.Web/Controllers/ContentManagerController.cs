using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core.Domain;
using Raven.Client;

namespace GroupGiving.Web.Controllers
{
    public class ContentManagerController : Controller
    {
        private readonly IDocumentStore _store;

        public ContentManagerController(IDocumentStore store)
        {
            _store = store;
        }

        //
        // GET: /Content/

        public ActionResult Index()
        {
            var viewModel = new ContentListViewModel();
            using (var session = _store.OpenSession())
            {
                viewModel.Pages = session.Query<PageContent>().Take(1024);
            }
            return View(viewModel);
        }

        public ActionResult DeletePage(string id)
        {
            using (var session = _store.OpenSession())
            {
                var pageContent = session.Load<PageContent>("pagecontents/"+id);
                if (pageContent != null)
                {
                    session.Delete(pageContent);
                    session.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UpdateContentDefinition(int pageId, string contentLabel, string culture, string content)
        {
            using (var session = _store.OpenSession())
            {
                var page = session.Load<PageContent>("pagecontents/" + pageId);
                if (page==null)
                {
                    return HttpNotFound();
                }

                var contentDefinition = page.Content.Where(cd => cd.Label == contentLabel).FirstOrDefault();
                if (contentDefinition==null)
                {
                    return HttpNotFound();
                }

                string cultureContent = "";
                if (contentDefinition.ContentByCulture.ContainsKey(culture))
                {
                    contentDefinition.ContentByCulture[culture]=content;
                } else
                {
                    contentDefinition.ContentByCulture.Add(culture, content);
                }
                session.SaveChanges();
            }

            return RedirectToAction("Index");
        }

    }

    public class ContentListViewModel
    {
        public IQueryable<PageContent> Pages { get; set; }
    }
}
