using System.Collections.Generic;

namespace TicketMuffin.Core.Domain
{
    public class ContentDefinition
    {
        public string Label { get; set; }
        public List<LocalisedContent> ContentByCulture { get; set; }
    }

    public class LocalisedContent
    {
        public string Id { get; set; }
        public string Address { get; set; }
        public string Culture { get; set; }
        public string Value { get; set; }
    }
}