using System;
using System.Collections.Generic;
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
            viewModel.Pages = _session.Query<PageContent>().OrderBy(p=>p.Id);


            return View(viewModel);
        }

        public ActionResult Clean()
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

            var pages = _session.Query<PageContent>().ToList();

            log.Append("<p><strong>cleaning translations</strong></p>");
            foreach(var page in pages)
            {
                foreach(var content in page.Content)
                {
                    log.AppendFormat("<p>{0}.{1}</p>", page.Id, content.Label);
                    Dictionary<string,LocalisedContent> translationsToKeep = new Dictionary<string,LocalisedContent>();
                    var englishVersion = content.ContentByCulture.SingleOrDefault(lc=>lc.Culture.Equals("en", StringComparison.InvariantCultureIgnoreCase));

                    if (englishVersion==null)
                    {
                        englishVersion = content.ContentByCulture.SingleOrDefault(lc=>lc.Culture.Equals("en-GB", StringComparison.InvariantCultureIgnoreCase));
                        if (englishVersion != null)
                        {
                            englishVersion.Culture = "en";
                        } else
                        {
                            englishVersion = content.ContentByCulture.SingleOrDefault(lc=>lc.Culture.Equals("en-US", StringComparison.InvariantCultureIgnoreCase));
                            if (englishVersion != null)
                            {
                                englishVersion.Culture = "en";
                            }
                        }
                    }

                    if (englishVersion != null)
                    {
                        translationsToKeep.Add(englishVersion.Culture, englishVersion);
                    }

                    foreach(var localised in content.ContentByCulture)
                    {
                        if (localised.Culture.StartsWith("en"))
                            continue;

                        string culture = localised.Culture;
                        if (culture.Contains("-"))
                            culture = culture.Substring(0, culture.IndexOf("-"));
                        culture = culture.ToLowerInvariant();

                        localised.Culture = culture;

                        if (!translationsToKeep.ContainsKey(culture))
                        {
                            translationsToKeep.Add(culture, localised);
                        } else
                        {
                            if (englishVersion != null && localised.Value != englishVersion.Value)
                            {
                                translationsToKeep[culture] = localised;
                            }
                        }
                    }

                    content.ContentByCulture.Clear();
                    foreach(var keep in translationsToKeep)
                    {
                        log.AppendFormat("<p>{0}</p>", keep.Value.Culture);
                        content.ContentByCulture.Add(keep.Value);
                    }
                }
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
