using TicketMuffin.Core.Domain;

namespace TicketMuffin.Web.Models
{
    public class RefundViewModel
    {
        public GroupGivingEvent Event { get; set; }

        public EventPledge PledgeToBeRefunded { get; set; }

        public bool RefundFailed { get; set; }
    }
}