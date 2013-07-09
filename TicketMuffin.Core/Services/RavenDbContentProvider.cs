using System;
using System.Collections.Concurrent;
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
        readonly Stack<LocalisedContent> CreatedContent = new Stack<LocalisedContent>();
        private readonly IDocumentSession _session;
        private readonly ILog _logger = LogManager.GetLogger(typeof (RavenDbContentProvider));

        static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, LocalisedContent>> ContentsByAddress = new ConcurrentDictionary<string, ConcurrentDictionary<string, LocalisedContent>>();

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

        static object _lock = new object();

        public LocalisedContent GetContent(string pageAddress, string label, string defaultContent, string culture)
        {
            lock (_lock)
            {
                pageAddress = Sanitize(pageAddress).ToLowerInvariant();
                label = Sanitize(label).ToLowerInvariant();
                culture = culture.ToLowerInvariant();
                if (culture.Contains("-"))
                    culture = culture.Substring(0, culture.IndexOf("-", System.StringComparison.Ordinal));

                LocalisedContent page = null;

                string pageCacheKey = string.Concat(culture, "/", pageAddress);
                page = GetFromCache(pageCacheKey, label);
                if (page==null)
                {
                    _logger.WarnFormat("content cache miss: {0}/{1}", pageAddress, label);
                    _logger.DebugFormat("not found in store: {0}/{1}", pageAddress, label);
                    page = CreatePage(pageAddress, label, defaultContent, culture);
                    AddToCache(page);
                }
                else
                {
                    _logger.DebugFormat("Loaded content from cache: {0}/{1}", pageAddress, label);
                }
                _logger.Debug("exit GetContent");
                return page;
            }
        }

        private static LocalisedContent GetFromCache(string pageKey, string contentKey)
        {
            LocalisedContent page=null;
            if (ContentsByAddress.ContainsKey(pageKey))
            {
                var contentForPage = ContentsByAddress[pageKey];
                if (contentForPage.ContainsKey(contentKey))
                {
                    page = contentForPage[contentKey];
                }
            }
            return page;
        }

        private static string CacheKey(LocalisedContent content)
        {
            return string.Concat(content.Culture, "/", content.Address);
        }

        private void AddToCache(ContentByCultureAndAddress.LocalisedContentByCultureAndAddressResult contentForPage)
        {
            _logger.DebugFormat("Adding page to cache: {0}", contentForPage.Key);
            var pageSet = ContentsByAddress.GetOrAdd(contentForPage.Key, new ConcurrentDictionary<string, LocalisedContent>());
            
            foreach (var content in contentForPage.ContentItems)
            {
                _logger.DebugFormat("Adding content to cache: {0}/{1}", content.Address, content.Label);
                pageSet.GetOrAdd(content.Label, content);
            }
        }

        private void AddToCache(LocalisedContent content)
        {
            var pageSet = ContentsByAddress.GetOrAdd(CacheKey(content), new ConcurrentDictionary<string, LocalisedContent>());

            _logger.DebugFormat("Adding content to cache: {0}/{1}", content.Address, content.Label);
            pageSet.GetOrAdd(content.Label, content);
        }

        private LocalisedContent CreatePage(string pageAddress, string label, string defaultContent, string culture)
        {
            var content = new LocalisedContent()
                {
                    Address = pageAddress,
                    Value = defaultContent,
                    Culture = culture,
                    Label = label
                };
            CreatedContent.Push(content);

            _session.Store(content);
            _logger.DebugFormat("Stored content {0}/{1}", content.Address, content.Label);

            return content;
        }

        public void Dispose()
        {
            if (CreatedContent.Count > 0)
            {
                int count = CreatedContent.Count;
                _logger.DebugFormat("Saving {0} content items", count);
                try
                {
                    _session.SaveChanges();
                } catch
                {
                    _session.Advanced.Clear();
                }
                CreatedContent.Clear();
            }
        }
    }
}