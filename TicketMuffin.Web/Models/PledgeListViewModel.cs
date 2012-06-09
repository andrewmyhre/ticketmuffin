using System.Collections.Generic;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Web.Models
{
    public class PledgeListViewModel
    {
        public IEnumerable<EventPledge> Pledges { get; set; }

        public string ShortUrl { get; set; }

        public string EventName { get; set; }
    }
}