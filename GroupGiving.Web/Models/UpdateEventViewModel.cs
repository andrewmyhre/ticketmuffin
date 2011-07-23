using System;
using System.Collections.Generic;
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
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }
        public string Venue { get; set; }
        public string AddressLine { get; set; }
        public string ShortUrl { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsFeatured { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public decimal TicketPrice { get; set; }

        public int MinimumParticipants { get; set; }

        public int? MaximumParticipants { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime SalesEndDateTime { get; set; }

        [DataType(DataType.MultilineText)]
        public string AdditionalBenefits { get; set; }

        public string PaypalAccountEmailAddress { get; set; }

        public List<EventPledge> Pledges { get; set; }
    }
}