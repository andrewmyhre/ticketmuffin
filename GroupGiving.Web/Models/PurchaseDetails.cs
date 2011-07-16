using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GroupGiving.Web.Models
{
    public class PurchaseDetails
    {
        [Range(1,3, ErrorMessage="You may purchase between 1 and 3 tickets")]
        public int Quantity { get; set; }
        public string ShortUrl { get; set; }
        public string EventId { get; set; }
        [Required(ErrorMessage = "")]
        public string EmailAddress { get; set; }

        public string Password { get; set; }
        public bool HaveAccount { get; set; }

        public IEnumerable<string> AttendeeName { get; set; }

        public bool AcceptTerms { get; set; }
        public bool OptInForOffers { get; set; }
    }
}