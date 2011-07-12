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

        public string PostCode { get; set; }

        public string Country { get; set; }

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

        public string ContactName { get; set; }

        public int DaysLeft { get; set; }

        public int HoursLeft { get; set; }

        public int MinutesLeft { get; set; }

        public int SecondsLeft { get; set; }

        public bool CountDown { get; set; }

        public double VenueLatitude { get; set; }

        public double VenueLongitude { get; set; }
    }
}