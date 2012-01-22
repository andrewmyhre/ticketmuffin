using System.Collections.Generic;
using System.Linq;
using System.Web;
using GroupGiving.Core.Domain;
using JustGiving.Api.Sdk.Model;
using Raven.Client;
using Raven.Client.Linq;

namespace GroupGiving.Web.Code
{
    public class RavenDbContentProvider : IContentProvider
    {
        private readonly IDocumentSession _session;
        private bool _newContent = false;
        private List<PageContent> _pages = new List<PageContent>();

        public RavenDbContentProvider(IDocumentSession session)
        {
            _session = session;
            _pages = _session.Query<PageContent>().Take(1024).ToList();
        }

        public void Initialise()
        {
            
        }

        private string Sanitize(string something)
        {
            return something
                .Replace('/', '-')
                .Replace("~", "")
                .Replace("<","-")
                .Replace(">","-");
        }

        public PageContent GetPage(string address)
        {
            var page = _pages.FirstOrDefault(p => p.Address == Sanitize(address));

            if (page == null)
            {
                page = _session.Query<PageContent>().FirstOrDefault(pc => pc.Address == Sanitize(address));
            }

            return page;
        }

        public PageContent AddContentPage(string pageAddress)
        {
            var page = new PageContent() { Address = Sanitize(pageAddress), Content = new List<ContentDefinition>() };

            _newContent = true;
            _pages.Add(page);
            _session.Store(page);
            return page;
        }

        public ContentDefinition AddContentDefinition(PageContent pageContent, string label, string defaultContent="", string culture="en")
        {
            var pc = _session.Load<PageContent>(pageContent.Id);
            label = Sanitize(label);
            if (pc != null)
            {
                var contentDefinition = new ContentDefinition(){Label=label, ContentByCulture = new Dictionary<string, string>()};  
                if (!string.IsNullOrWhiteSpace(defaultContent))
                {
                    contentDefinition.ContentByCulture.Add(culture, defaultContent);
                }
                pc.Content.Add(contentDefinition);
                _session.Store(pc);

                var transientPage = _pages.FirstOrDefault(p=>p.Id == pageContent.Id);
                if (transientPage != null && !transientPage.Content.Any(c=>c.Label == label))
                {
                    transientPage.Content.Add(contentDefinition);
                }

                _newContent = true;
                return contentDefinition;
            }
            return null;
        }

        public void Flush()
        {
            if (_newContent)
            {
                _session.SaveChanges();
            }
        }

        public string GetContent(string pageAddress, string label, string defaultContent, string culture, out PageContent pageObject, out string contentLabel)
        {
            pageAddress = Sanitize(pageAddress);
            label = Sanitize(label);
            PageContent page = null/*_pages.FirstOrDefault(p => p.Address == pageAddress)*/;
            if (page == null) {
                page = _session.Query<PageContent>().FirstOrDefault(p => p.Address == pageAddress);
            }

            ContentDefinition contentDefinition = null;

            if (page != null)
            {
                contentDefinition = page.Content.FirstOrDefault(cd => cd.Label == label);
                if (contentDefinition == null)
                {
                    contentDefinition = new ContentDefinition()
                                            {
                                                ContentByCulture = new Dictionary<string, string>()
                                                                       {
                                                                           {culture, defaultContent}
                                                                       },
                                                Label = label
                                            };

                    // if we're updating, make sure the page is attached to the db session
                    if (!_session.Advanced.IsLoaded(page.Id))
                    {
                        page = _session.Load<PageContent>(page.Id);
                    }
                    page.Content.Add(contentDefinition);
                    contentLabel = label;
                    pageObject = page;
                    _newContent = true;
                    return defaultContent;
                }
                contentLabel = contentDefinition.Label;
                pageObject = page;

                if (!contentDefinition.ContentByCulture.ContainsKey(culture))
                {
                    // if we're updating, make sure the page is attached to the db session
                    if (!_session.Advanced.IsLoaded(page.Id))
                    {
                        page = _session.Load<PageContent>(page.Id);
                        contentDefinition = page.Content.FirstOrDefault(cd => cd.Label == label);
                    }

                    contentDefinition.ContentByCulture.Add(culture, defaultContent);
                    _newContent = true;
                }
                return contentDefinition.ContentByCulture[culture];
            }
            else // no page, make a whole new one
            {

                page = new PageContent()
                           {
                               Address = pageAddress,
                               Content = new List<ContentDefinition>()
                                             {
                                                 new ContentDefinition()
                                                {
                                                    ContentByCulture = new Dictionary<string, string>()
                                                                           {
                                                                               {culture, defaultContent}
                                                                           },
                                                    Label = label
                                                },
                                             }
                           };
                _session.Store(page);
                _pages.Add(page);
                pageObject = page;
                contentLabel = label;
                _newContent = true;
                return defaultContent;
            }
        }
    }
}