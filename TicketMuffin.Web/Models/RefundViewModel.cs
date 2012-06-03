using System;
using GroupGiving.Core.Domain;

namespace GroupGiving.Web.Models
{
    public class RefundViewModel
    {
        public GroupGivingEvent Event { get; set; }

        public EventPledge PledgeToBeRefunded { get; set; }

        public bool RefundFailed { get; set; }
    }
}