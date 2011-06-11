using System;

namespace GroupGiving.Core.Domain
{
    public class GroupGivingEvent : IDomainObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
    }
}
