using System.Linq;
using Raven.Client.Indexes;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Indexes
{
    public class ContentByCultureAndAddress : AbstractIndexCreationTask<LocalisedContent, ContentByCultureAndAddress.LocalisedContentByCultureAndAddressResult>
    {
        public class LocalisedContentByCultureAndAddressResult
        {
            public string Key { get; set; }
            public LocalisedContent[] ContentItems { get; set; }
        }

        public ContentByCultureAndAddress()
        {
            Map = contents => from content in contents 
                              select new {
                                  Key=string.Concat(content.Culture,"/",content.Address), 
                                ContentItems = new[]{content}};
            Reduce = results => from result in results
                                group result by result.Key
                                into g
                                select new
                                    {
                                        Key = g.Key,
                                        ContentItems = g.Select(x=>x.ContentItems)
                                    };
        }
    }
}