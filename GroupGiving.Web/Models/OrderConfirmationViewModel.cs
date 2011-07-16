using System;
using System.Collections.Generic;
using GroupGiving.Core.Domain;

namespace GroupGiving.Web.Models
{
    public class OrderConfirmationViewModel
    {
        public GroupGivingEvent Event { get; set; }

        public int PledgesRequired { get; set; }

        public EventPledge Pledge { get; set; }
    }
}