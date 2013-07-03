using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TicketMuffin.Core.Domain;
using TicketMuffin.Core.Payments;

namespace TicketMuffin.Web.Areas.Api.Models
{
    [DataContract(Name="event", Namespace="http://schemas.ticketmuffin.com/2011")]
    public class EventModel
    {
        [DataMember(Name="id")]
        public string Id { get; set; }
        [DataMember(Name = "creatorId")]
        public string CreatorId { get; set; }
        [DataMember(Name = "title")]
        public string Title { get; set; }
        [DataMember(Name = "city")]
        public string City { get; set; }
        [DataMember(Name = "country")]
        public string Country { get; set; }
        [DataMember(Name = "description")]
        public string Description { get; set; }
        [DataMember(Name = "startDate")]
        public DateTime StartDate { get; set; }
        [DataMember(Name = "venue")]
        public string Venue { get; set; }
        [DataMember(Name = "addressLine")]
        public string AddressLine { get; set; }
        [DataMember(Name = "shortUrl")]
        public string ShortUrl { get; set; }
        [DataMember(Name = "isPrivate")]
        public bool IsPrivate { get; set; }
        [DataMember(Name = "isFeatured")]
        public bool IsFeatured { get; set; }
        [DataMember(Name = "phoneNumber")]
        public string PhoneNumber { get; set; }
        [DataMember(Name = "latitude")]
        public float Latitude { get; set; }
        [DataMember(Name = "longitude")]
        public float Longitude { get; set; }
        [DataMember(Name = "ticketPrice")]
        public decimal TicketPrice { get; set; }
        [DataMember(Name = "minimumParticipants")]
        public int MinimumParticipants { get; set; }
        [DataMember(Name = "maximumParticipants")]
        public int? MaximumParticipants { get; set; }
        [DataMember(Name = "salesEndDateTime")]
        public DateTime SalesEndDateTime { get; set; }
        [DataMember(Name = "additionalBenefits")]
        public string AdditionalBenefits { get; set; }
        [DataMember(Name = "paypalAccountEmailAddress")]
        public string PaypalAccountEmailAddress { get; set; }
        [DataMember(Name = "pledges")]
        public List<EventPledge> Pledges { get; set; }
        [DataMember(Name = "isOn")]
        public bool IsOn
        {
            get { return PledgeCount >= MinimumParticipants; }
        }
        [DataMember(Name = "isFull")]
        public bool IsFull
        {
            get { return PledgeCount >= MaximumParticipants; }
        }
        [DataMember(Name = "pledgeCount")]
        public int PledgeCount
        {
            get
            {
                return Pledges.Where(p => p.Paid
                    && (p.PaymentStatus == PaymentStatus.Unsettled
                    || p.PaymentStatus == PaymentStatus.Settled)).Sum(p => p.Attendees.Count());
            }
        }
        [DataMember(Name = "spacesLeft")]
        public int SpacesLeft
        {
            get { return (MaximumParticipants ?? 0) - PledgeCount; }
        }
        [DataMember(Name = "organiserName")]
        public string OrganiserName { get; set; }
        [DataMember(Name = "postCode")]
        public string Postcode { get; set; }
    }
}