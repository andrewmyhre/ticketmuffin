using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GroupGiving.Web.Models
{
    public class EventPledgeViewModel : EventViewModel
    {
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide your contact email address")]
        public string EmailAddress { get; set; }

        public bool AcceptTerms { get; set; }

        public IEnumerable<string> AttendeeName { get; set; }

        public bool OptInForOffers { get; set; }
    }
}