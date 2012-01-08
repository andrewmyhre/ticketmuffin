using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GroupGiving.Core.Domain;

namespace GroupGiving.Web.Models
{
    public class UpdateEventViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Postcode { get; set; }
        public string LatLong { get; set; }
        [DataType(DataType.MultilineText)]
        [UIHint("markdown")]
        public string Description { get; set; }
        [DataType(DataType.DateTime)]
        [Display(Name="Event Date")]
        public DateTime StartDate { get; set; }
        public string Venue { get; set; }
        [Display(Name="Street address")]
        public string AddressLine { get; set; }
        [Display(Name="http://www.ticketmuffin.com/", Description="The url for your event")]
        public string ShortUrl { get; set; }
        [Display(Name="Invite only?")]
        public bool IsPrivate { get; set; }
        [Display(Name="Feature the event on the homepage?")]
        public bool IsFeatured { get; set; }
        [DataType(DataType.PhoneNumber)]
        [Display(Name="Contact phone.")]
        public string PhoneNumber { get; set; }
        
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [Display(Name="Ticket price")]
        public decimal TicketPrice { get; set; }

        [Display(Name="Minimum participants")]
        public int MinimumParticipants { get; set; }

        [Display(Name="Maximum participants")]
        public int? MaximumParticipants { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name="Date sales should end")]
        public DateTime SalesEndDateTime { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name="Other information e.g: the benefits of attending")]
        [UIHint("markdown")]
        public string AdditionalBenefits { get; set; }

        public string PaypalAccountEmailAddress { get; set; }

        public List<EventPledge> Pledges { get; set; }

        public EventState State { get; set; }

        [Display(Name="Event organiser (email address)")]
        public string EventOrganiser { get; set; }

        public string ImageUrl { get; set; }

        public bool IsOn { get; set; }
        public int AttendeeCount { get; set; }
        public bool ReadyToActivate { get; set; }
    }
}