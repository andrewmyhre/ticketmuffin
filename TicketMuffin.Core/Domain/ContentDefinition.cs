using System.Collections.Generic;

namespace GroupGiving.Core.Domain
{
    public class ContentDefinition
    {
        public string Label { get; set; }
        public List<LocalisedContent> ContentByCulture { get; set; }
    }

    public class LocalisedContent
    {
        public string Culture { get; set; }
        public string Value { get; set; }
    }
}