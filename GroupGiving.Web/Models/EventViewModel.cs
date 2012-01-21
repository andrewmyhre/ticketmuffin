using System;
using System.Collections.Generic;
using GroupGiving.Core.Domain;
using GroupGiving.Web.Areas.Admin.Controllers;
using Raven.Client.Linq;

namespace GroupGiving.Web.Models
{
    public class EventViewModel
    {
        public bool CanAcceptPledges
        {
            get
            {
                return !EventIsFull 
                    && SalesEndDateTime > DateTime.Now
                    && (State == EventState.SalesReady
                    || State == EventState.Activated);
            }
        }

        public string Id { get; set; }

        public DateTime StartDate { get; set; }

        public string AdditionalBenefitsMarkedDown { get; set; }

        public string AddressLine { get; set; }

        public string City { get; set; }

        public string PostCode { get; set; }

        public string Country { get; set; }

        public string DescriptionMarkedDown { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsPrivate { get; set; }

        public int? MaximumParticipants { get; set; }

        public int MinimumParticipants { get; set; }

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

        public IEnumerable<EventPledge> Pledges { get; set; }

        public int PledgeCount { get; set; }

        public decimal RequiredPledgesPercentage { get; set; }

        public bool EventIsOn { get; set; }

        public bool EventIsFull { get; set; }

        public bool UserIsEventOwner { get; set; }

        public EventState State { get; set; }

        public string ImageUrl { get; set; }

        public decimal TotalPledgesPercentage { get; set; }
        public Charity Charity { get; set; }

        public IEnumerable<TransactionHistoryEntry> TransactionHistory { get; set; }

        public bool ReadyToActivate { get; set; }

        public int AttendeeCount { get; set; }

        public int PaidAttendeeCount { get; set; }

        public string TicketCurrency { get; set; }

        public GroupGivingEvent Event { get; set; }
    }
}
