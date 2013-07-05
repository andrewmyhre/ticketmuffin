using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Indexes;
using log4net;

namespace TicketMuffin.Core.Services
{
    public class RavenDbContentProvider : IContentProvider, IDisposable
    {
        private readonly IDocumentSession _session;
        private readonly ILog _logger = LogManager.GetLogger(typeof (RavenDbContentProvider));

        static Dictionary<string, Dictionary<string, LocalisedContent>> _contentsByAddress = new Dictionary<string, Dictionary<string, LocalisedContent>>();

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
                .Replace(" ","-")
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

        public LocalisedContent GetContent(string pageAddress, string label, string defaultContent, string culture)
        {
            pageAddress = Sanitize(pageAddress).ToLowerInvariant();
            label = Sanitize(label).ToLowerInvariant();
            culture = culture.ToLowerInvariant();
            if (culture.Contains("-"))
                culture = culture.Substring(0, culture.IndexOf("-", System.StringComparison.Ordinal));

            LocalisedContent page = null;

            string pageCacheKey = string.Concat(culture, "/", pageAddress);
            if (!PageIsCached(culture, pageAddress))
            {
                var contentForPage = _session.Query<ContentByCultureAndAddress.LocalisedContentByCultureAndAddressResult>("ContentByCultureAndAddress", true)
                    .SingleOrDefault(x => x.Key == pageCacheKey);
                if (contentForPage != null)
                    AddToCache(contentForPage);
                
            }
            page = GetFromCache(pageCacheKey, label);

            if(page==null)
            {
                _logger.WarnFormat("content cache miss");
                page = CreatePage(pageAddress, label, defaultContent, culture);
            }

            return page;
        }

        private bool PageIsCached(string culture, string address)
        {
            return _contentsByAddress.ContainsKey(string.Concat(culture, "/", address));
        }

        private static LocalisedContent GetFromCache(string pageCacheKey, string label)
        {
            LocalisedContent page=null;
            if (_contentsByAddress.ContainsKey(pageCacheKey))
            {
                var contentForPage = _contentsByAddress[pageCacheKey];
                if (contentForPage.ContainsKey(label))
                {
                    page = contentForPage[label];
                }
            }
            return page;
        }

        private void AddToCache(ContentByCultureAndAddress.LocalisedContentByCultureAndAddressResult contentForPage)
        {
            if (!_contentsByAddress.ContainsKey(contentForPage.Key))
            {
                _contentsByAddress.Add(contentForPage.Key, new Dictionary<string, LocalisedContent>());
            }
            foreach (var content in contentForPage.ContentItems)
            {
                _contentsByAddress[contentForPage.Key].Add(content.Label, content);    
            }
            
        }

        Stack<LocalisedContent> _createdContent = new Stack<LocalisedContent>(); 
        private LocalisedContent CreatePage(string pageAddress, string label, string defaultContent, string culture)
        {
            var content = new LocalisedContent()
                {
                    Address = pageAddress,
                    Value = defaultContent,
                    Culture = culture,
                    Label = label
                };
            _createdContent.Push(content);
            _session.Store(content);
            _logger.DebugFormat("Stored content {0}", content.Id);
 
            return content;
        }

        public void Dispose()
        {
            if (_createdContent.Count > 0)
            {
                int count = _createdContent.Count;
                _logger.DebugFormat("Saving {0} content items", count);
                try
                {
                    _session.SaveChanges();
                } catch
                {
                    _session.Advanced.Clear();
                }
                _createdContent.Clear();
            }
        }
    }
}