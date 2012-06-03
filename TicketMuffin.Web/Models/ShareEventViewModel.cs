namespace GroupGiving.Web.Models
{
    public class ShareEventViewModel
    {
        public GroupGiving.Core.Domain.GroupGivingEvent Event { get; set; }

        public string ShareUrl { get; set; }

        public bool? EmailSentSuccessfully { get; set; }

        public ShareViaEmailViewModel ShareViaEmail { get; set; }

        public bool EmailSent { get; set; }
    }
}