﻿using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Imports.Newtonsoft.Json;

namespace TicketMuffin.Core.Domain
{
    public class GroupGivingEvent : IDomainObject
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public string Venue { get; set; }
        public string AddressLine { get; set; }
        public string ShortUrl { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsFeatured { get; set; }
        public string PhoneNumber { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public EventState State { get; set; }

        public decimal TicketPrice { get; set; }

        public int MinimumParticipants { get; set; }

        public int? MaximumParticipants { get; set; }

        public DateTime SalesEndDateTime { get; set; }

        public string AdditionalBenefits { get; set; }

        public string PaypalAccountEmailAddress { get; set; }

        public List<EventPledge> Pledges { get; set; }

        [JsonIgnore]
        public bool IsOn
        {
            get { return PaidAttendeeCount >= MinimumParticipants; }
        }
        [JsonIgnore]
        public bool IsFull
        {
            get { return PaidAttendeeCount >= MaximumParticipants; }
        }

        [JsonIgnore]
        public int PaidAttendeeCount { 
            get { 
                return Pledges.Where(p=>p.Paid).Sum(p => p.Attendees.Count()); 
            } 
        }
        [JsonIgnore]
        public int PaidPledgeCount
        {
            get
            {
                return Pledges.Count(p => p.Paid);
            }
        }
        [JsonIgnore]
        public int PledgeCount
        {
            get
            {
                return Pledges.Count;
            }
        }

        [JsonIgnore]
        public int AttendeeCount
        {
            get
            {
                return Pledges.Sum(p=>p.Attendees.Count());
            }
        }

        [JsonIgnore]
        public int SpacesLeft
        {
            get { return (MaximumParticipants??0) - PaidAttendeeCount; }
        }

        public string OrganiserName { get; set; }

        public string Postcode { get; set; }

        public string PayPalAccountFirstName { get; set; }

        public string PayPalAccountLastName { get; set; }

        public string OrganiserId { get; set; }

        public string ImageFilename { get; set; }

        public string ImageUrl { get; set; }
        public Charity CharityDetails { get; set; }

        [JsonIgnore]
        public bool ReadyToActivate
        {
            get { return this.PaidAttendeeCount >= this.MinimumParticipants
                && this.State == EventState.SalesReady; }
        }

        public bool SalesEnded
        {
            get { return SalesEndDateTime < DateTime.Now; }
        }

        public int CurrencyNumericCode { get; set; }

        public GroupGivingEvent()
        {
            Pledges = new List<EventPledge>();
        }
    }

    public enum EventState
    {
        Creating,
        SalesReady,
        Activated,
        SalesClosed,
        Completed,
        Cancelled,
        Deleted
    }
}
