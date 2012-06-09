using TicketMuffin.Core.Domain;

namespace TicketMuffin.Web.Models
{
    public class ActivateEventViewModel
    {
        public GroupGivingEvent Event { get; set; }

        public decimal TotalAmountOwedToFundraiser { get; set; }
    }
}