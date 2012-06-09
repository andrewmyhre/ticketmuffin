using TicketMuffin.Core.Domain;

namespace TicketMuffin.Web.Models
{
    public class OrderConfirmationViewModel
    {
        public GroupGivingEvent Event { get; set; }

        public int PledgesRequired { get; set; }

        public EventPledge Pledge { get; set; }
    }
}