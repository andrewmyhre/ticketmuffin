using System;

namespace GroupGiving.Core.Domain
{
    public class GroupGivingEvent : IDomainObject
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string City { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public string Venue { get; set; }
        public string AddressLine { get; set; }
        public string ShortUrl { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsFeatured { get; set; }
        public string PhoneNumber { get; set; }
    }
}
