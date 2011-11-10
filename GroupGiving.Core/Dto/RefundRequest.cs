using System.Collections.Generic;

namespace GroupGiving.Core.Dto
{
    public class RefundRequest
    {
        public string TransactionId { get; set; }

        public List<PaymentRecipient> Receivers { get; set; }
    }
}