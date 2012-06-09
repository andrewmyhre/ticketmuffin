using TicketMuffin.PayPal.Model;

namespace TicketMuffin.Core.Dto
{
    public class PaymentDetails
    {
        public string SenderEmailAddress { get; set; }

        public string Status { get; set; }

        public object RawResponse { get; set; }

        public DialogueHistoryEntry DialogueEntry { get; set; }
    }
}