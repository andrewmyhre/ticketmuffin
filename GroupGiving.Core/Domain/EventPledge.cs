﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GroupGiving.Core.Domain
{
    public class EventPledge
    {
        public DateTime DatePledged { get; set; }
        public DateTime? DateRefunded { get; set; }
        public bool Paid { get; set;}
        public bool Refunded { get; set; }
        public string EmailAddress { get; set; }
        public string TransactionId { get; set; }
        public string OrderNumber { get; set; }
        public List<EventPledgeAttendee> Attendees { get; set; }

        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxRateApplied { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public EventPledge()
        {
            Attendees = new List<EventPledgeAttendee>();
        }
    }
}
