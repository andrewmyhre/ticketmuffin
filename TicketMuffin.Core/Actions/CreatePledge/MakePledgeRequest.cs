using System.Collections.Generic;

namespace TicketMuffin.Core.Actions.CreatePledge
{
    public class MakePledgeRequest
    {
        public IEnumerable<string> AttendeeNames { get; set; }
        public string PayPalEmailAddress { get; set; }

        public bool OptInForOffers { get; set; }

        public string WebsiteUrlBase { get; set; }
    }
}