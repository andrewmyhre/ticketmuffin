using System;

namespace GroupGiving.Core.Services
{
    public class CreateEventRequest
    {
        public string Title { get; set; }

        public DateTime DateAndTime { get; set; }

        public string Venue { get; set; }

        public string AddressLine { get; set; }

        public string City { get; set; }

        public string PostCode { get; set; }

        public string Description { get; set; }

        public string ShortUrl { get; set; }

        public bool IsPrivate { get; set; }

        public bool IsFeatured { get; set; }

        public string PhoneNumber { get; set; }
    }
}