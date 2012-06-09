using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Web.Code
{
    public class RavenDbContentProvider : IContentProvider
    {
        private readonly IDocumentSession _session;

        public RavenDbContentProvider(IDocumentSession session)
        {
            _session = session;
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
            return _session.Query<PageContent>().FirstOrDefault(pc => pc.Address == Sanitize(address));
        }

        public PageContent AddContentPage(string pageAddress)
        {
            var page = new PageContent() { Address = Sanitize(pageAddress), Content = new List<ContentDefinition>() };

            _session.Store(page);
            return page;
        }

        public ContentDefinition AddContentDefinition(PageContent pageContent, string label, string defaultContent="", string culture="en")
        {
            var pc = _session.Load<PageContent>(pageContent.Id);
            label = Sanitize(label);
            if (pc != null)
            {
                var contentDefinition = new ContentDefinition(){Label=label, 
                    ContentByCulture = new List<LocalisedContent>()};  
                if (!string.IsNullOrWhiteSpace(defaultContent))
                {
                    contentDefinition.ContentByCulture.Add(new LocalisedContent(){Culture=culture, Value = defaultContent});
                }
                pc.Content.Add(contentDefinition);
                _session.Store(pc);

                return contentDefinition;
            }
            return null;
        }

        public void Flush()
        {
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
                                                ContentByCulture = new List<LocalisedContent>()
                                                                       {
                                                                           new LocalisedContent() { Culture = culture, Value = defaultContent}
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
                    _session.SaveChanges();
                    if (_session.Advanced.NumberOfRequests >= _session.Advanced.MaxNumberOfRequestsPerSession - 1)
                    {
                        _session.Advanced.MaxNumberOfRequestsPerSession *= 2;
                    }
                    return defaultContent;
                }
                contentLabel = contentDefinition.Label;
                pageObject = page;

                if (!contentDefinition.ContentByCulture.Any(lc=>lc.Culture == culture))
                {
                    // if we're updating, make sure the page is attached to the db session
                    if (!_session.Advanced.IsLoaded(page.Id))
                    {
                        page = _session.Load<PageContent>(page.Id);
                        contentDefinition = page.Content.FirstOrDefault(cd => cd.Label == label);
                    }

                    contentDefinition.ContentByCulture.Add(new LocalisedContent(){Culture = culture, Value = defaultContent});

                    _session.SaveChanges();
                    if (_session.Advanced.NumberOfRequests >= _session.Advanced.MaxNumberOfRequestsPerSession - 1)
                    {
                        _session.Advanced.MaxNumberOfRequestsPerSession *= 2;
                    }
                }
                return contentDefinition.ContentByCulture.SingleOrDefault(lc=>lc.Culture == culture).Value;
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
                                                    ContentByCulture = new List<LocalisedContent>()
                                                                           {
                                                                               new LocalisedContent(){Culture = culture, Value = defaultContent}
                                                                           },
                                                    Label = label
                                                },
                                             }
                           };
                _session.Store(page);
                _session.SaveChanges();
                if (_session.Advanced.NumberOfRequests >= _session.Advanced.MaxNumberOfRequestsPerSession-1)
                {
                    _session.Advanced.MaxNumberOfRequestsPerSession *= 2;
                }
                pageObject = page;
                contentLabel = label;
                return defaultContent;
            }
        }
    }
}