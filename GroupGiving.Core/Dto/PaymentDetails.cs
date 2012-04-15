using GroupGiving.PayPal.Model;

namespace GroupGiving.Core.Dto
{
    public class PaymentDetails
    {
        public string SenderEmailAddress { get; set; }

        public string Status { get; set; }

        public object RawResponse { get; set; }

        public DialogueHistoryEntry DialogueEntry { get; set; }
    }
}