using System.Collections.Generic;
using System.Linq;
using GroupGiving.Core.Domain;
using Raven.Client;
using Raven.Client.Linq;

namespace GroupGiving.Web.Code
{
    public class RavenDbContentProvider : IContentProvider
    {
        private readonly IDocumentStore _store;

        public RavenDbContentProvider(IDocumentStore store)
        {
            _store = store;
        }

        public void Initialise()
        {
            
        }

        public PageContent GetPage(string address)
        {
            using (var session = _store.OpenSession())
            {
                var page = session.Query<PageContent>().Where(pc => pc.Address == address).FirstOrDefault();

                return page;
            }
        }

        public PageContent AddContentPage(string pageAddress)
        {
            using (var session = _store.OpenSession())
            {
                var page = new PageContent() {Address = pageAddress, Content = new List<ContentDefinition>()};
                session.Store(page);
                session.SaveChanges();
                return page;
            }
        }

        public ContentDefinition AddContentDefinition(PageContent pageContent, string label, string defaultContent="", string culture="en")
        {
            using (var session = _store.OpenSession())
            {
                var pc = session.Load<PageContent>(pageContent.Id);
                if (pc != null)
                {
                    var contentDefinition = new ContentDefinition(){Label=label, ContentByCulture = new Dictionary<string, string>()};  
                    if (!string.IsNullOrWhiteSpace(defaultContent))
                    {
                        contentDefinition.ContentByCulture.Add(culture, defaultContent);
                    }
                    pc.Content.Add(contentDefinition);
                    session.SaveChanges();
                    return contentDefinition;
                }
                return null;
            }
        }
    }
}