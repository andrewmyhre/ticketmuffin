using System;
using System.Collections.Generic;

namespace GroupGiving.Web.Models
{
    public class PurchaseDetails
    {
        public string PayPalEmailAddress { get; set; }
        public int Quantity { get; set; }
        public string ShortUrl { get; set; }
        public string EventId { get; set; }

        public string EmailAddress { get; set; }

        public string Password { get; set; }
        public bool HaveAccount { get; set; }

        public IEnumerable<string> AttendeeName { get; set; }
    }
}