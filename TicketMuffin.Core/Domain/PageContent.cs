using System.Collections.Generic;

namespace TicketMuffin.Core.Domain
{
    public class PageContent : IDomainObject
    {
        public string Id { get; set; }
        public string Address { get; set; }
        public List<ContentDefinition> Content { get; set; } 
    }
}