using System;

namespace GroupGiving.Web.Models
{
    public class EventViewModel
    {
        public string EventId { get; set; }

        public DateTime StartDate { get; set; }

        public string AdditionalBenefits { get; set; }

        public string AddressLine { get; set; }

        public string City { get; set; }

        public string Description { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsPrivate { get; set; }

        public int? MaximumParticipants { get; set; }

        public int MinimumParticipants { get; set; }

        public string PaypalAccountEmailAddress { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime SalesEndDateTime { get; set; }

        public string ShortUrl { get; set; }

        public string Title { get; set; }

        public decimal TicketPrice { get; set; }

        public string Venue { get; set; }
    }
}