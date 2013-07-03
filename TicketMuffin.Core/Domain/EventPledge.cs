using System;
using System.Collections.Generic;
using TicketMuffin.Core.Payments;

namespace TicketMuffin.Core.Domain
{
    public class EventPledge
    {
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public DateTime DatePledged { get; set; }
        public DateTime? DateRefunded { get; set; }
        public bool Paid { get; set;}
        public bool Refunded { get; set; }
        public string AccountEmailAddress { get; set; }
        public string TransactionId { get; set; }
        public string OrderNumber { get; set; }
        public List<EventPledgeAttendee> Attendees { get; set; }

        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxRateApplied { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public decimal ServiceChargeRateApplied { get; set; }

        public decimal ServiceCharge { get; set; }

        public List<DialogueHistoryEntry> PaymentGatewayHistory { get; set; }

        public string Notes { get; set; }

        public EventPledge()
        {
            Attendees = new List<EventPledgeAttendee>();
        }
    }
}
