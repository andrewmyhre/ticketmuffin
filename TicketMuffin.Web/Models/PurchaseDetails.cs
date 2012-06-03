using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GroupGiving.Web.Models
{
    public class PurchaseDetails
    {
        [Range(1,99999, ErrorMessage="You must select at least 1 ticket")]
        public int Quantity { get; set; }
        public string ShortUrl { get; set; }
        public string EventId { get; set; }

        public string Password { get; set; }
        public bool HaveAccount { get; set; }

        public IEnumerable<string> AttendeeName { get; set; }

        public bool AcceptTerms { get; set; }
        public bool OptInForOffers { get; set; }
    }
}