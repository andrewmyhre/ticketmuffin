using System;

namespace TicketMuffin.Core.Actions.CreatePledge
{
    public class CreatePledgeActionResult
    {
        public bool Succeeded { get; set; }

        public Exception Exception { get; set; }

        public string TransactionId { get; set; }

        public string PaymentPageUrl { get; private set; }
    }
}