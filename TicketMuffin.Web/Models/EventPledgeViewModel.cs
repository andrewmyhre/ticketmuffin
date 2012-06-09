using System.Collections.Generic;

namespace TicketMuffin.Web.Models
{
    public class EventPledgeViewModel : EventViewModel
    {
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string EmailAddress { get; set; }

        public bool AcceptTerms { get; set; }

        public IEnumerable<string> AttendeeName { get; set; }

        public bool OptInForOffers { get; set; }

        public bool UserIsEventOwner { get; set; }
    }
}