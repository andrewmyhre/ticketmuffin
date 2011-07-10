namespace GroupGiving.Core.Domain
{
    public class ContentDefinition : IDomainObject
    {
        public string Page { get; set; }
        public string Label { get; set; }
        public string Content { get; set; }
        public string Id { get; set; }
    }
}