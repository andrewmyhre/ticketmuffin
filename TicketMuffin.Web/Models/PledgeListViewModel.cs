using System.Collections.Generic;
using GroupGiving.Core.Domain;

namespace GroupGiving.Web.Models
{
    public class PledgeListViewModel
    {
        public IEnumerable<EventPledge> Pledges { get; set; }

        public string ShortUrl { get; set; }

        public string EventName { get; set; }
    }
}