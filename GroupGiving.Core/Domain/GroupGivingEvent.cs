using System;

namespace GroupGiving.Core.Domain
{
    public class GroupGivingEvent : DomainBase
    {
        public new Guid Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
    }
}
