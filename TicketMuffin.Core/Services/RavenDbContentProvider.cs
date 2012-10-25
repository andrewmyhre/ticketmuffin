using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Services
{
    public class RavenDbContentProvider : IContentProvider
    {
        private readonly IDocumentSession _session;
        private PageContent _page=null;

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
                .Replace(">","-")
                .Replace(".","-")
                .Trim('-');
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
            pageAddress = Sanitize(pageAddress).ToLowerInvariant();
            label = Sanitize(label).ToLowerInvariant();
            culture = culture.ToLowerInvariant();

            if (_page == null) {
                _page = _session
                    .Load<PageContent>("PageContents/"+pageAddress);
            }

            if(_page==null)
            {
                _page = CreatePage(pageAddress, label, defaultContent, culture);
            }

            ContentDefinition contentDefinition = _page.Content.SingleOrDefault(cd => cd.Label == label);
            var defaultLocalisedContent = new LocalisedContent() {Culture = culture, Value = defaultContent};
            
            if (contentDefinition == null)
                {
                    contentDefinition = new ContentDefinition()
                    {
                        ContentByCulture = new List<LocalisedContent>(){defaultLocalisedContent},
                        Label = label
                    };

                    _page.Content.Add(contentDefinition);
                }

            contentLabel = contentDefinition.Label;
            pageObject = _page;

            var localisedContent = contentDefinition.ContentByCulture.SingleOrDefault(lc => lc.Culture == culture);
            if (localisedContent==null)
            {
                localisedContent = defaultLocalisedContent;
                // if we're updating, make sure the _page is attached to the db session
                if (!_session.Advanced.IsLoaded(_page.Id))
                {
                    _page = _session.Load<PageContent>(_page.Id);
                    contentDefinition = _page.Content.FirstOrDefault(cd => cd.Label == label);
                }

                contentDefinition.ContentByCulture.Add(defaultLocalisedContent);
            }

            return localisedContent.Value;
        }

        private PageContent CreatePage(string pageAddress, string label, string defaultContent, string culture)
        {
            var page = new PageContent()
                {
                    Id="PageContents/"+pageAddress,
                    Address = pageAddress,
                    Content = new List<ContentDefinition>()
                        {
                            new ContentDefinition()
                                {
                                    ContentByCulture = new List<LocalisedContent>()
                                        {
                                            new LocalisedContent() {Culture = culture, Value = defaultContent}
                                        },
                                    Label = label
                                },
                        }
                };
            _session.Store(page);

            return page;
        }
    }
}