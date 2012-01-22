using System.Collections.Generic;
using System.Linq;
using GroupGiving.Core.Domain;
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
        }

        public void Initialise()
        {
            
        }

        public PageContent GetPage(string address)
        {
            var page = _pages.FirstOrDefault(p => p.Address == address);

            if (page == null)
            {
                page = _session.Query<PageContent>().FirstOrDefault(pc => pc.Address == address);
            }

            return page;
        }

        public PageContent AddContentPage(string pageAddress)
        {
            var page = new PageContent() {Address = pageAddress, Content = new List<ContentDefinition>()};

            _newContent = true;
            _pages.Add(page);
            _session.Store(page);
            return page;
        }

        public ContentDefinition AddContentDefinition(PageContent pageContent, string label, string defaultContent="", string culture="en")
        {
            var pc = _session.Load<PageContent>(pageContent.Id);
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
    }
}