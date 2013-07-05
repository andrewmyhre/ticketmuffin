using System;
using System.Collections.Generic;
using System.Linq;
using TicketMuffin.Core.Payments;

namespace TicketMuffin.Core.Domain
{
    public class EventPledge
    {
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public DateTime DatePledged { get; set; }
        public DateTime? DateRefunded { get; set; }
        public bool Paid { get { return Payments.Any(x => x.PaymentStatus == PaymentStatus.Settled || x.PaymentStatus == PaymentStatus.AuthorisedUnsettled); } }
        public bool Refunded { get; set; }
        public string PayPalEmailAddress { get; set; }
        public string OrderNumber { get; set; }
        public List<EventPledgeAttendee> Attendees { get; set; }

        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxRateApplied { get; set; }

        public decimal ServiceChargeRateApplied { get; set; }

        public decimal ServiceCharge { get; set; }

        public List<DialogueHistoryEntry> PaymentGatewayHistory { get; set; }

        public string Notes { get; set; }

        public List<Payment> Payments { get; set; }

        public string AccountEmailAddress { get; set; }

        public EventPledge()
        {
            Attendees = new List<EventPledgeAttendee>();
            Payments = new List<Payment>();
            PaymentGatewayHistory = new List<DialogueHistoryEntry>();
        }
    }
}
