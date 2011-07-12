using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GroupGiving.Core.Domain
{
    public class EventPledge : IDomainObject
    {
        public string Id { get; set; }
        public string EventId { get; set; }
        public string EventTitle { get; set; }
        public decimal TicketPrice { get; set; }
        public DateTime DatePledged { get; set; }
        public DateTime? DateRefunded { get; set; }
        public bool Paid { get; set;}
        public bool Refunded { get; set; }
        public string EmailAddress { get; set; }
        public string PayPalPayKey { get; set; }

        public decimal AmountPaid { get; set; }
    }
}
