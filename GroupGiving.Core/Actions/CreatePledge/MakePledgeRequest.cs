using System;
using System.Collections.Generic;

namespace GroupGiving.Core.Actions.CreatePledge
{
    public class MakePledgeRequest
    {
        public IEnumerable<string> AttendeeNames { get; set; }
        public string PayPalEmailAddress { get; set; }

        public bool OptInForOffers { get; set; }
    }
}