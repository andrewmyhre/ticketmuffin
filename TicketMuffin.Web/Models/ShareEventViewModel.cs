using TicketMuffin.Core.Domain;

namespace TicketMuffin.Web.Models
{
    public class ShareEventViewModel
    {
        public GroupGivingEvent Event { get; set; }

        public string ShareUrl { get; set; }

        public bool? EmailSentSuccessfully { get; set; }

        public ShareViaEmailViewModel ShareViaEmail { get; set; }

        public bool EmailSent { get; set; }
    }
}